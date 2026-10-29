using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface ITheLoaiRepository
{
    Task<IReadOnlyList<TheLoai>> SearchAsync(string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken);
    Task<TheLoai?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, int? excludingId, CancellationToken cancellationToken);
    Task<bool> IsUsedByBooksAsync(int id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(TheLoai entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
