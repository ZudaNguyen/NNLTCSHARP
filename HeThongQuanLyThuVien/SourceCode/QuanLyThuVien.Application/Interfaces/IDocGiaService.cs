using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IDocGiaService
{
    Task<IReadOnlyList<DocGiaDto>> SearchAsync(string? search, string? trangThai, CancellationToken cancellationToken);
    Task<DocGiaDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<DocGiaDto> CreateAsync(CreateDocGiaDto request, CancellationToken cancellationToken);
    Task<DocGiaDto?> UpdateAsync(int id, UpdateDocGiaDto request, CancellationToken cancellationToken);
    Task<DocGiaDto?> UpdateTrangThaiAsync(int id, string trangThai, CancellationToken cancellationToken);
}
