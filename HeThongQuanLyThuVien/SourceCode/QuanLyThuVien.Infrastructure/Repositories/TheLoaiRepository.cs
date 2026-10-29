using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class TheLoaiRepository(LibraryDbContext dbContext) : ITheLoaiRepository
{
    public async Task<IReadOnlyList<TheLoai>> SearchAsync(
        string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken)
    {
        var query = dbContext.TheLoai.AsNoTracking().AsQueryable();
        if (onlyDeleted)
            query = query.Where(entity => entity.IsDeleted);
        else if (!includeDeleted)
            query = query.Where(entity => !entity.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(entity =>
                entity.TenTheLoai.Contains(term)
                || (entity.MoTa != null && entity.MoTa.Contains(term)));
        }

        return await query.OrderBy(entity => entity.TenTheLoai)
            .ThenBy(entity => entity.MaTheLoai)
            .ToListAsync(cancellationToken);
    }

    public Task<TheLoai?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = dbContext.TheLoai.AsQueryable();
        if (!includeDeleted)
            query = query.Where(entity => !entity.IsDeleted);
        return query.FirstOrDefaultAsync(entity => entity.MaTheLoai == id, cancellationToken);
    }

    public Task<bool> NameExistsAsync(string name, int? excludingId, CancellationToken cancellationToken) =>
        dbContext.TheLoai.AnyAsync(
            entity => entity.TenTheLoai == name && (!excludingId.HasValue || entity.MaTheLoai != excludingId.Value),
            cancellationToken);

    public Task<bool> IsUsedByBooksAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Sach.AnyAsync(book => book.MaTheLoai == id, cancellationToken);

    public async Task<bool> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TheLoai.FirstOrDefaultAsync(x => x.MaTheLoai == id, cancellationToken);
        if (entity is null) return false;
        if (entity.IsDeleted)
        {
            entity.IsDeleted = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return true;
    }

    public async Task AddAsync(TheLoai entity, CancellationToken cancellationToken)
    {
        dbContext.TheLoai.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
