using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface ISachRepository
{
    Task<IReadOnlyList<Sach>> SearchAsync(
        string? keyword,
        int? maTheLoai,
        bool includeDeleted,
        bool onlyDeleted,
        CancellationToken cancellationToken);

    Task<Sach?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Sach?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken);
    Task<bool> IsIsbnInUseAsync(string isbn, int? excludingId, CancellationToken cancellationToken);
    Task<bool> CategoryExistsAsync(int maTheLoai, CancellationToken cancellationToken);
    Task<IReadOnlySet<int>> GetExistingAuthorIdsAsync(IEnumerable<int> authorIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<TheLoai>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TacGia>> GetAuthorsAsync(CancellationToken cancellationToken);
    Task AddAsync(Sach sach, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken);
    Task UpdateWithAuthorsAsync(Sach sach, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
