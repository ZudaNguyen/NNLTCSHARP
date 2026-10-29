using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.UnitTests.Services;

public sealed class AccountServiceTests
{
    private const string StrongPassword = "DocGia_Test_2026!A";

    private static RegisterReaderRequestDto Request(
        string tenDangNhap = "docgia.moi",
        string email = "docgia.moi@example.invalid",
        string matKhau = StrongPassword) => new()
    {
        TenDangNhap = tenDangNhap,
        MatKhau = matKhau,
        HoTen = "  Nguyễn Độc Giả Mới  ",
        NgaySinh = new DateTime(2001, 5, 12),
        GioiTinh = "Nam",
        SoDienThoai = "0912345678",
        Email = email,
        DiaChi = "  Hà Nội  "
    };

    [Fact]
    public async Task RegisterReaderAsync_TaoDocGiaVaTaiKhoanHoatDongNgay()
    {
        var repository = new FakeRepository();
        var service = new AccountService(repository, new FakePasswordHasher());

        var account = await service.RegisterReaderAsync(Request(), CancellationToken.None);

        Assert.Equal(AccountRoles.Reader, account.VaiTro);
        Assert.Equal("Hoạt động", account.TrangThai);
        Assert.Equal("docgia.moi", account.TenDangNhap);
        Assert.Equal("Nguyễn Độc Giả Mới", account.TenHienThi);

        var reader = Assert.IsType<DocGia>(repository.AddedReader);
        Assert.Equal("Nguyễn Độc Giả Mới", reader.HoTen);
        Assert.Equal("Hoạt động", reader.TrangThai);
        Assert.Equal("Hà Nội", reader.DiaChi);

        var stored = Assert.IsType<TaiKhoan>(repository.AddedAccount);
        Assert.Equal("DOCGIA.MOI", stored.TenDangNhapChuanHoa);
        Assert.Equal(reader.MaDocGia, stored.MaDocGia);
        Assert.Equal("hash:" + StrongPassword, stored.MatKhauHash);
        Assert.Equal(reader.MaDocGia, account.MaDocGia);
    }

    [Fact]
    public async Task RegisterReaderAsync_TrungTenDangNhap_NemAccountConflict()
    {
        var repository = new FakeRepository { UserNameExists = true };
        var service = new AccountService(repository, new FakePasswordHasher());

        var exception = await Assert.ThrowsAsync<AccountConflictException>(
            () => service.RegisterReaderAsync(Request(), CancellationToken.None));

        Assert.Contains("Tên đăng nhập", exception.Message);
        Assert.Null(repository.AddedAccount);
        Assert.Null(repository.AddedReader);
    }

    [Fact]
    public async Task RegisterReaderAsync_TrungEmail_NemAccountConflict()
    {
        var repository = new FakeRepository { EmailExists = true };
        var service = new AccountService(repository, new FakePasswordHasher());

        var exception = await Assert.ThrowsAsync<AccountConflictException>(
            () => service.RegisterReaderAsync(Request(), CancellationToken.None));

        Assert.Contains("Email", exception.Message);
        Assert.Null(repository.AddedAccount);
    }

    [Fact]
    public async Task RegisterReaderAsync_MatKhauYeu_NemPasswordPolicy()
    {
        var repository = new FakeRepository();
        var service = new AccountService(repository, new FakePasswordHasher());

        var exception = await Assert.ThrowsAsync<PasswordPolicyException>(
            () => service.RegisterReaderAsync(
                Request(matKhau: "abc123456789"), CancellationToken.None));

        Assert.Equal(PasswordPolicy.Message, exception.Message);
        Assert.Null(repository.AddedAccount);
    }

    private sealed class FakePasswordHasher : IPasswordHashingService
    {
        public string Hash(TaiKhoan account, string password) => "hash:" + password;

        public PasswordVerification Verify(TaiKhoan account, string passwordHash, string password) =>
            passwordHash == "hash:" + password ? PasswordVerification.Succeeded : PasswordVerification.Failed;
    }

    private sealed class FakeRepository : ITaiKhoanRepository
    {
        public bool UserNameExists { get; init; }
        public bool EmailExists { get; init; }
        public DocGia? AddedReader { get; private set; }
        public TaiKhoan? AddedAccount { get; private set; }

        public Task<bool> UserNameExistsAsync(string normalizedUserName, int? excludingId, CancellationToken ct) =>
            Task.FromResult(UserNameExists);

        public Task<bool> ReaderEmailExistsAsync(string email, CancellationToken ct) =>
            Task.FromResult(EmailExists);

        public Task AddReaderWithAccountAsync(DocGia reader, TaiKhoan account, CancellationToken ct)
        {
            AddedReader = reader;
            reader.MaDocGia = 7;
            account.MaDocGia = reader.MaDocGia;
            account.MaTaiKhoan = 9;
            AddedAccount = account;
            return Task.CompletedTask;
        }

        public Task<TaiKhoan?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task<TaiKhoan?> GetByIdAsync(int id, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task<IReadOnlyList<TaiKhoan>> SearchAsync(string? search, string? role, string? status, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task<bool> EmployeeExistsAsync(int id, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task<bool> ReaderExistsAsync(int id, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task<bool> HasAnyAdminAsync(CancellationToken ct) =>
            throw new NotSupportedException();
        public Task AddAsync(TaiKhoan account, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task AddBootstrapAdminIfNoneAsync(TaiKhoan account, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task SaveAdministrativeChangesAsync(TaiKhoan account, bool guardLastAdmin, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task SaveCredentialChangesAndRevokeSessionsAsync(TaiKhoan account, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task RecordFailedLoginAsync(int accountId, int maxAttempts, TimeSpan lockoutDuration, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task ResetLoginFailuresAsync(TaiKhoan account, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task CreateSessionAsync(TaiKhoanSession session, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task<ValidatedSession?> FindValidSessionAsync(byte[] tokenHash, DateTime nowUtc, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task RevokeSessionAsync(Guid sessionId, int accountId, DateTime revokedAtUtc, CancellationToken ct) =>
            throw new NotSupportedException();
        public Task RevokeAllSessionsAsync(int accountId, DateTime revokedAtUtc, CancellationToken ct) =>
            throw new NotSupportedException();
    }
}
