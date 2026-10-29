using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface ITacGiaRepository
{
    Task<IReadOnlyList<TacGia>> SearchAsync(string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken);
    Task<TacGia?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, int? excludingId, CancellationToken cancellationToken);
    Task<bool> IsLinkedToBooksAsync(int id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(TacGia entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
