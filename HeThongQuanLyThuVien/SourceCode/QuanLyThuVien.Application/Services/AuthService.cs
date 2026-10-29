using System.Security.Cryptography;
using System.Text;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class InvalidLoginException : Exception
{
    public InvalidLoginException() : base("Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản không hoạt động.") { }
}

public sealed class PasswordPolicyException(string message) : Exception(message) { }

public sealed class AuthService(ITaiKhoanRepository repository, IPasswordHashingService passwordHasher) : IAuthService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const int MaxFailedAttempts = 5;
    private readonly TaiKhoan _dummyAccount = new();
    private readonly string _dummyHash = passwordHasher.Hash(new TaiKhoan(), Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct)
    {
        var normalized = NormalizeUserName(request.TenDangNhap);
        var account = await repository.FindByNormalizedUserNameAsync(normalized, ct);
        if (account is null)
        {
            passwordHasher.Verify(_dummyAccount, _dummyHash, request.MatKhau);
            throw new InvalidLoginException();
        }

        var now = DateTime.UtcNow;
        if (account.TrangThai != "Hoạt động" || account.KhoaDenUtc > now)
        {
            passwordHasher.Verify(account, account.MatKhauHash, request.MatKhau);
            throw new InvalidLoginException();
        }

        var verification = passwordHasher.Verify(account, account.MatKhauHash, request.MatKhau);
        if (verification == PasswordVerification.Failed)
        {
            await repository.RecordFailedLoginAsync(account.MaTaiKhoan, MaxFailedAttempts, LockoutDuration, ct);
            throw new InvalidLoginException();
        }

        if (verification == PasswordVerification.RehashRequired)
        {
            account.MatKhauHash = passwordHasher.Hash(account, request.MatKhau);
            account.CapNhatLucUtc = now;
            await repository.SaveCredentialChangesAndRevokeSessionsAsync(account, ct);
        }
        await repository.ResetLoginFailuresAsync(account, ct);

        var rawToken = ToBase64Url(RandomNumberGenerator.GetBytes(32));
        var expires = now.Add(SessionLifetime);
        await repository.CreateSessionAsync(new TaiKhoanSession
        {
            Id = Guid.NewGuid(),
            MaTaiKhoan = account.MaTaiKhoan,
            TokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)),
            TaoLucUtc = now,
            HetHanLucUtc = expires
        }, ct);

        return new LoginResponseDto { AccessToken = rawToken, ExpiresAtUtc = expires, User = MapUser(account) };
    }

    public async Task<AuthUserDto?> GetMeAsync(int accountId, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(accountId, ct);
        return account is null ? null : MapUser(account);
    }

    public async Task<AuthenticatedPrincipal?> ValidateSessionAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || rawToken.Length > 256) return null;
        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        var session = await repository.FindValidSessionAsync(tokenHash, DateTime.UtcNow, ct);
        if (session is null) return null;
        return new AuthenticatedPrincipal(
            session.Account.MaTaiKhoan,
            session.SessionId,
            session.Account.TenDangNhap,
            session.Account.TenHienThi,
            session.Account.VaiTro,
            session.Account.MaNhanVien,
            session.Account.MaDocGia);
    }

    public Task LogoutAsync(int accountId, Guid sessionId, CancellationToken ct) =>
        repository.RevokeSessionAsync(sessionId, accountId, DateTime.UtcNow, ct);

    public async Task ChangePasswordAsync(int accountId, ChangePasswordRequestDto request, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(accountId, ct)
            ?? throw new InvalidLoginException();
        if (passwordHasher.Verify(account, account.MatKhauHash, request.MatKhauHienTai) == PasswordVerification.Failed)
            throw new InvalidLoginException();
        if (!PasswordPolicy.IsStrong(request.MatKhauMoi)) throw new PasswordPolicyException(PasswordPolicy.Message);
        if (passwordHasher.Verify(account, account.MatKhauHash, request.MatKhauMoi) != PasswordVerification.Failed)
            throw new PasswordPolicyException("Mật khẩu mới phải khác mật khẩu hiện tại.");

        account.MatKhauHash = passwordHasher.Hash(account, request.MatKhauMoi);
        account.CapNhatLucUtc = DateTime.UtcNow;
        account.SoLanDangNhapThatBai = 0;
        account.KhoaDenUtc = null;
        await repository.SaveCredentialChangesAndRevokeSessionsAsync(account, ct);
    }

    private static AuthUserDto MapUser(TaiKhoan account) => new()
    {
        MaTaiKhoan = account.MaTaiKhoan,
        TenDangNhap = account.TenDangNhap,
        TenHienThi = account.TenHienThi,
        VaiTro = account.VaiTro,
        MaNhanVien = account.MaNhanVien,
        MaDocGia = account.MaDocGia
    };

    public static string NormalizeUserName(string userName) => userName.Trim().ToUpperInvariant();

    private static string ToBase64Url(byte[] value) => Convert.ToBase64String(value)
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');
}
