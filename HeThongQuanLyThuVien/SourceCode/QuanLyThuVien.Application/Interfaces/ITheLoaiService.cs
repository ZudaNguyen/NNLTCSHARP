using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface ITheLoaiService
{
    Task<IReadOnlyList<TheLoaiDto>> SearchAsync(string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken);
    Task<TheLoaiDto?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
    Task<TheLoaiDto> CreateAsync(TheLoaiWriteDto request, CancellationToken cancellationToken);
    Task<TheLoaiDto?> UpdateAsync(int id, TheLoaiWriteDto request, CancellationToken cancellationToken);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken);
}
