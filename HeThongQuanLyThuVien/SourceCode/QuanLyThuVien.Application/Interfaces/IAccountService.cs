using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> SearchAsync(string? search, string? role, string? status, CancellationToken ct);
    Task<AccountDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<AccountDto> CreateAsync(CreateAccountRequestDto request, CancellationToken ct);
    Task<AccountDto> RegisterReaderAsync(RegisterReaderRequestDto request, CancellationToken ct);
    Task<AccountDto?> UpdateAsync(int id, UpdateAccountRequestDto request, int actorId, CancellationToken ct);
    Task<AccountDto?> UpdateStatusAsync(int id, string status, int actorId, CancellationToken ct);
    Task<bool> ResetPasswordAsync(int id, string newPassword, CancellationToken ct);
    Task BootstrapAdminAsync(AccountBootstrapOptions options, CancellationToken ct);
}
