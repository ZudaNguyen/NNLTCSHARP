using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public sealed record ValidatedSession(Guid SessionId, TaiKhoan Account);

public interface ITaiKhoanRepository
{
    Task<TaiKhoan?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken ct);
    Task<TaiKhoan?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<TaiKhoan>> SearchAsync(string? search, string? role, string? status, CancellationToken ct);
    Task<bool> UserNameExistsAsync(string normalizedUserName, int? excludingId, CancellationToken ct);
    Task<bool> EmployeeExistsAsync(int id, CancellationToken ct);
    Task<bool> ReaderExistsAsync(int id, CancellationToken ct);
    Task<bool> HasAnyAdminAsync(CancellationToken ct);
    Task AddAsync(TaiKhoan account, CancellationToken ct);
    Task AddBootstrapAdminIfNoneAsync(TaiKhoan account, CancellationToken ct);
    Task SaveAdministrativeChangesAsync(TaiKhoan account, bool guardLastAdmin, CancellationToken ct);
    Task SaveCredentialChangesAndRevokeSessionsAsync(TaiKhoan account, CancellationToken ct);
    Task RecordFailedLoginAsync(int accountId, int maxAttempts, TimeSpan lockoutDuration, CancellationToken ct);
    Task ResetLoginFailuresAsync(TaiKhoan account, CancellationToken ct);
    Task CreateSessionAsync(TaiKhoanSession session, CancellationToken ct);
    Task<ValidatedSession?> FindValidSessionAsync(byte[] tokenHash, DateTime nowUtc, CancellationToken ct);
    Task RevokeSessionAsync(Guid sessionId, int accountId, DateTime revokedAtUtc, CancellationToken ct);
    Task RevokeAllSessionsAsync(int accountId, DateTime revokedAtUtc, CancellationToken ct);
}
