using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public sealed record AuthenticatedPrincipal(
    int AccountId,
    Guid SessionId,
    string UserName,
    string DisplayName,
    string Role,
    int? EmployeeId,
    int? ReaderId);

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct);
    Task<AuthUserDto?> GetMeAsync(int accountId, CancellationToken ct);
    Task<AuthenticatedPrincipal?> ValidateSessionAsync(string rawToken, CancellationToken ct);
    Task LogoutAsync(int accountId, Guid sessionId, CancellationToken ct);
    Task ChangePasswordAsync(int accountId, ChangePasswordRequestDto request, CancellationToken ct);
}
