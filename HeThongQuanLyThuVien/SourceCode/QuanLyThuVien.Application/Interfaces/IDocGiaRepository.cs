using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface IDocGiaRepository
{
    Task<IReadOnlyList<DocGia>> SearchAsync(string? search, string? trangThai, CancellationToken cancellationToken);
    Task<DocGia?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, int? excludingId, CancellationToken cancellationToken);
    Task AddAsync(DocGia entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
