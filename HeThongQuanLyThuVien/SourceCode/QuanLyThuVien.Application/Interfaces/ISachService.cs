using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface ISachService
{
    Task<IReadOnlyList<SachDto>> SearchAsync(
        string? keyword,
        int? maTheLoai,
        bool includeDeleted,
        bool onlyDeleted,
        CancellationToken cancellationToken);

    Task<SachDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<SachDto> CreateAsync(SachWriteDto request, CancellationToken cancellationToken);
    Task<SachDto?> UpdateAsync(int id, SachWriteDto request, CancellationToken cancellationToken);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken);
    Task<SachLookupsDto> GetLookupsAsync(CancellationToken cancellationToken);
}
