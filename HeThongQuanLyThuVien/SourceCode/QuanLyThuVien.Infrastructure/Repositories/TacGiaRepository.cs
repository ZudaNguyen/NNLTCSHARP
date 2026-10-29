using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class TacGiaRepository(LibraryDbContext dbContext) : ITacGiaRepository
{
    public async Task<IReadOnlyList<TacGia>> SearchAsync(
        string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken)
    {
        var query = dbContext.TacGia.AsNoTracking().AsQueryable();
        if (onlyDeleted)
            query = query.Where(entity => entity.IsDeleted);
        else if (!includeDeleted)
            query = query.Where(entity => !entity.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(entity =>
                entity.TenTacGia.Contains(term)
                || (entity.QuocTich != null && entity.QuocTich.Contains(term))
                || (entity.GioiThieu != null && entity.GioiThieu.Contains(term)));
        }

        return await query.OrderBy(entity => entity.TenTacGia)
            .ThenBy(entity => entity.MaTacGia)
            .ToListAsync(cancellationToken);
    }

    public Task<TacGia?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = dbContext.TacGia.AsQueryable();
        if (!includeDeleted)
            query = query.Where(entity => !entity.IsDeleted);
        return query.FirstOrDefaultAsync(entity => entity.MaTacGia == id, cancellationToken);
    }

    public Task<bool> NameExistsAsync(string name, int? excludingId, CancellationToken cancellationToken) =>
        dbContext.TacGia.AnyAsync(
            entity => entity.TenTacGia == name && (!excludingId.HasValue || entity.MaTacGia != excludingId.Value),
            cancellationToken);

    public Task<bool> IsLinkedToBooksAsync(int id, CancellationToken cancellationToken) =>
        dbContext.SachTacGia.AnyAsync(link => link.MaTacGia == id, cancellationToken);

    public async Task<bool> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TacGia.FirstOrDefaultAsync(x => x.MaTacGia == id, cancellationToken);
        if (entity is null) return false;
        if (entity.IsDeleted)
        {
            entity.IsDeleted = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return true;
    }

    public async Task AddAsync(TacGia entity, CancellationToken cancellationToken)
    {
        dbContext.TacGia.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
