using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface ITacGiaService
{
    Task<IReadOnlyList<TacGiaDto>> SearchAsync(string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken);
    Task<TacGiaDto?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
    Task<TacGiaDto> CreateAsync(TacGiaWriteDto request, CancellationToken cancellationToken);
    Task<TacGiaDto?> UpdateAsync(int id, TacGiaWriteDto request, CancellationToken cancellationToken);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken);
}
