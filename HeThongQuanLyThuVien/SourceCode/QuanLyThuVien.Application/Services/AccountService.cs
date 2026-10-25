using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class AccountConflictException(string message) : Exception(message) { }
public sealed class AccountForbiddenException(string message) : Exception(message) { }
public sealed class AccountNotFoundException : Exception { public AccountNotFoundException() : base("Không tìm thấy tài khoản.") { } }

public sealed class AccountService(ITaiKhoanRepository repository, IPasswordHashingService passwordHasher) : IAccountService
{
    public async Task<IReadOnlyList<AccountDto>> SearchAsync(string? search, string? role, string? status, CancellationToken ct) =>
        (await repository.SearchAsync(search, role, status, ct)).Select(Map).ToList();

    public async Task<AccountDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(id, ct);
        return account is null ? null : Map(account);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequestDto request, CancellationToken ct)
    {
        var userName = request.TenDangNhap.Trim();
        var normalized = AuthService.NormalizeUserName(userName);
        if (await repository.UserNameExistsAsync(normalized, null, ct))
            throw new AccountConflictException("Tên đăng nhập đã được sử dụng.");
        await ValidateRoleLinkAsync(request.VaiTro, request.MaNhanVien, request.MaDocGia, ct);
        EnsurePassword(request.MatKhau);

        var account = new TaiKhoan
        {
            TenDangNhap = userName,
            TenDangNhapChuanHoa = normalized,
            TenHienThi = request.TenHienThi.Trim(),
            VaiTro = request.VaiTro,
            TrangThai = "Hoạt động",
            MaNhanVien = request.MaNhanVien,
            MaDocGia = request.MaDocGia,
            MatKhauHash = string.Empty
        };
        account.MatKhauHash = passwordHasher.Hash(account, request.MatKhau);
        await repository.AddAsync(account, ct);
        return Map(account);
    }

    public async Task<AccountDto?> UpdateAsync(int id, UpdateAccountRequestDto request, int actorId, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(id, ct);
        if (account is null) return null;
        if (id == actorId && account.VaiTro != request.VaiTro)
            throw new AccountForbiddenException("Không thể tự thay đổi vai trò của tài khoản đang đăng nhập.");
        await ValidateRoleLinkAsync(request.VaiTro, request.MaNhanVien, request.MaDocGia, ct);
        account.TenHienThi = request.TenHienThi.Trim();
        account.VaiTro = request.VaiTro;
        account.MaNhanVien = request.MaNhanVien;
        account.MaDocGia = request.MaDocGia;
        account.CapNhatLucUtc = DateTime.UtcNow;
        try { await repository.SaveAdministrativeChangesAsync(account, true, ct); }
        catch (LastAdminAccountException) { throw new AccountConflictException("Không thể hạ quyền quản trị viên hoạt động cuối cùng."); }
        return Map(account);
    }

    public async Task<AccountDto?> UpdateStatusAsync(int id, string status, int actorId, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(id, ct);
        if (account is null) return null;
        if (id == actorId && status != "Hoạt động")
            throw new AccountForbiddenException("Không thể khóa tài khoản đang đăng nhập.");
        account.TrangThai = status;
        account.CapNhatLucUtc = DateTime.UtcNow;
        try { await repository.SaveAdministrativeChangesAsync(account, true, ct); }
        catch (LastAdminAccountException) { throw new AccountConflictException("Không thể khóa quản trị viên hoạt động cuối cùng."); }
        return Map(account);
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword, CancellationToken ct)
    {
        EnsurePassword(newPassword);
        var account = await repository.GetByIdAsync(id, ct);
        if (account is null) return false;
        account.MatKhauHash = passwordHasher.Hash(account, newPassword);
        account.SoLanDangNhapThatBai = 0;
        account.KhoaDenUtc = null;
        account.CapNhatLucUtc = DateTime.UtcNow;
        await repository.SaveCredentialChangesAndRevokeSessionsAsync(account, ct);
        return true;
    }

    public async Task BootstrapAdminAsync(AccountBootstrapOptions options, CancellationToken ct)
    {
        if (await repository.HasAnyAdminAsync(ct)) return;

        var userName = options.TenDangNhap?.Trim();
        var displayName = options.TenHienThi?.Trim();
        var password = options.MatKhau;
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Chưa có tài khoản Admin. Cấu hình Authentication:BootstrapAdmin:UserName, DisplayName và Password trong User Secrets hoặc biến môi trường để khởi tạo Admin đầu tiên.");
        if (!PasswordPolicy.IsStrong(password)) throw new InvalidOperationException(PasswordPolicy.Message);

        var account = new TaiKhoan
        {
            TenDangNhap = userName,
            TenDangNhapChuanHoa = AuthService.NormalizeUserName(userName),
            TenHienThi = displayName,
            VaiTro = AccountRoles.Admin,
            TrangThai = "Hoạt động"
        };
        account.MatKhauHash = passwordHasher.Hash(account, password);
        await repository.AddBootstrapAdminIfNoneAsync(account, ct);
    }

    private async Task ValidateRoleLinkAsync(string role, int? employeeId, int? readerId, CancellationToken ct)
    {
        if (role is not (AccountRoles.Admin or AccountRoles.Librarian or AccountRoles.Reader))
            throw new AccountConflictException("Vai trò không hợp lệ.");
        if (role == AccountRoles.Admin)
        {
            if (readerId is not null) throw new AccountConflictException("Admin không thể liên kết hồ sơ độc giả.");
            if (employeeId is not null && !await repository.EmployeeExistsAsync(employeeId.Value, ct))
                throw new AccountConflictException("Không tìm thấy nhân viên đang hoạt động.");
            return;
        }
        if (role == AccountRoles.Librarian)
        {
            if (employeeId is null || readerId is not null || !await repository.EmployeeExistsAsync(employeeId.Value, ct))
                throw new AccountConflictException("Tài khoản THU_THU phải liên kết đúng một nhân viên đang hoạt động.");
            return;
        }
        if (readerId is null || employeeId is not null || !await repository.ReaderExistsAsync(readerId.Value, ct))
            throw new AccountConflictException("Tài khoản DOC_GIA phải liên kết đúng một độc giả đang hoạt động.");
    }

    private static void EnsurePassword(string password)
    {
        if (!PasswordPolicy.IsStrong(password)) throw new PasswordPolicyException(PasswordPolicy.Message);
    }

    private static AccountDto Map(TaiKhoan account) => new()
    {
        MaTaiKhoan = account.MaTaiKhoan,
        TenDangNhap = account.TenDangNhap,
        TenHienThi = account.TenHienThi,
        VaiTro = account.VaiTro,
        TrangThai = account.TrangThai,
        MaNhanVien = account.MaNhanVien,
        MaDocGia = account.MaDocGia,
        SoLanDangNhapThatBai = account.SoLanDangNhapThatBai,
        KhoaDenUtc = account.KhoaDenUtc,
        NgayTaoUtc = account.NgayTaoUtc,
        CapNhatLucUtc = account.CapNhatLucUtc
    };
}
