using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class DocGiaRepository(LibraryDbContext dbContext) : IDocGiaRepository
{
    public async Task<IReadOnlyList<DocGia>> SearchAsync(
        string? search, string? trangThai, CancellationToken cancellationToken)
    {
        var query = dbContext.DocGia.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            int? parsedId = int.TryParse(keyword, out var id) ? id : null;
            query = query.Where(x => x.HoTen.Contains(keyword)
                || x.SoDienThoai.Contains(keyword)
                || (x.Email != null && x.Email.Contains(keyword))
                || (parsedId.HasValue && x.MaDocGia == parsedId.Value));
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(x => x.TrangThai == trangThai);

        return await query.OrderBy(x => x.MaDocGia).ToListAsync(cancellationToken);
    }

    public Task<DocGia?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.DocGia.FirstOrDefaultAsync(x => x.MaDocGia == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludingId, CancellationToken cancellationToken) =>
        dbContext.DocGia.AnyAsync(x => x.Email == email && (!excludingId.HasValue || x.MaDocGia != excludingId.Value), cancellationToken);

    public async Task AddAsync(DocGia entity, CancellationToken cancellationToken)
    {
        dbContext.DocGia.Add(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DuplicateDocGiaEmailException();
        }
    }
}
