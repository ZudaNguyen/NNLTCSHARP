using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class SachRepository(LibraryDbContext dbContext) : ISachRepository
{
    public async Task<IReadOnlyList<Sach>> SearchAsync(
        string? keyword,
        int? maTheLoai,
        bool includeDeleted,
        bool onlyDeleted,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Sach
            .AsNoTracking()
            .Include(sach => sach.TheLoai)
            .Include(sach => sach.SachTacGias)
                .ThenInclude(link => link.TacGia)
            .AsQueryable();

        if (onlyDeleted)
            query = query.Where(sach => sach.IsDeleted);
        else if (!includeDeleted)
            query = query.Where(sach => !sach.IsDeleted);

        if (maTheLoai.HasValue)
            query = query.Where(sach => sach.MaTheLoai == maTheLoai.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(sach =>
                sach.TenSach.Contains(term)
                || sach.ISBN.Contains(term)
                || sach.SachTacGias.Any(link => link.TacGia.TenTacGia.Contains(term)));
        }

        return await query
            .OrderBy(sach => sach.TenSach)
            .ThenBy(sach => sach.MaSach)
            .ToListAsync(cancellationToken);
    }

    public Task<Sach?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Sach
            .Include(sach => sach.TheLoai)
            .Include(sach => sach.SachTacGias)
                .ThenInclude(link => link.TacGia)
            .FirstOrDefaultAsync(sach => sach.MaSach == id && !sach.IsDeleted, cancellationToken);

    public Task<Sach?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Sach
            .Include(sach => sach.TheLoai)
            .Include(sach => sach.SachTacGias)
                .ThenInclude(link => link.TacGia)
            .FirstOrDefaultAsync(sach => sach.MaSach == id, cancellationToken);

    public Task<bool> IsIsbnInUseAsync(
        string isbn,
        int? excludingId,
        CancellationToken cancellationToken) =>
        dbContext.Sach.AnyAsync(
            sach => sach.ISBN == isbn && (!excludingId.HasValue || sach.MaSach != excludingId.Value),
            cancellationToken);

    public Task<bool> CategoryExistsAsync(int maTheLoai, CancellationToken cancellationToken) =>
        dbContext.TheLoai.AnyAsync(
            theLoai => theLoai.MaTheLoai == maTheLoai && !theLoai.IsDeleted,
            cancellationToken);

    public async Task<IReadOnlySet<int>> GetExistingAuthorIdsAsync(
        IEnumerable<int> authorIds,
        CancellationToken cancellationToken)
    {
        var requestedIds = authorIds.Distinct().ToArray();
        return (await dbContext.TacGia
            .Where(tacGia => requestedIds.Contains(tacGia.MaTacGia) && !tacGia.IsDeleted)
            .Select(tacGia => tacGia.MaTacGia)
            .ToListAsync(cancellationToken))
            .ToHashSet();
    }

    public async Task<IReadOnlyList<TheLoai>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.TheLoai
            .AsNoTracking()
            .Where(theLoai => !theLoai.IsDeleted)
            .OrderBy(theLoai => theLoai.TenTheLoai)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TacGia>> GetAuthorsAsync(CancellationToken cancellationToken) =>
        await dbContext.TacGia
            .AsNoTracking()
            .Where(tacGia => !tacGia.IsDeleted)
            .OrderBy(tacGia => tacGia.TenTacGia)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Sach sach, IReadOnlyCollection<int> authorIds, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        sach.SachTacGias = authorIds
            .Distinct()
            .Select(maTacGia => new SachTacGia { MaTacGia = maTacGia, Sach = sach })
            .ToList();

        dbContext.Sach.Add(sach);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpdateWithAuthorsAsync(
        Sach sach,
        IReadOnlyCollection<int> authorIds,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var requestedIds = authorIds.Distinct().ToHashSet();
        var existingIds = sach.SachTacGias.Select(link => link.MaTacGia).ToHashSet();

        var removedLinks = sach.SachTacGias
            .Where(link => !requestedIds.Contains(link.MaTacGia))
            .ToList();
        dbContext.SachTacGia.RemoveRange(removedLinks);

        foreach (var link in removedLinks)
            sach.SachTacGias.Remove(link);

        foreach (var maTacGia in requestedIds.Except(existingIds))
        {
            sach.SachTacGias.Add(new SachTacGia { MaSach = sach.MaSach, MaTacGia = maTacGia, Sach = sach });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
