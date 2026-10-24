using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class NhanVienRepository(LibraryDbContext db) : INhanVienRepository
{
    public async Task<IReadOnlyList<NhanVien>> SearchAsync(string? search, string? status, string? jobTitle, CancellationToken ct)
    {
        var query = db.NhanVien.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            int? id = int.TryParse(keyword, out var parsed) ? parsed : null;
            query = query.Where(x => x.HoTen.Contains(keyword)
                || (x.SoDienThoai != null && x.SoDienThoai.Contains(keyword))
                || (id.HasValue && x.MaNhanVien == id.Value));
        }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.TrangThai == status.Trim());
        if (!string.IsNullOrWhiteSpace(jobTitle)) query = query.Where(x => x.ChucVu == jobTitle.Trim());
        return await query.OrderBy(x => x.MaNhanVien).Take(1000).ToListAsync(ct);
    }

    public Task<NhanVien?> GetByIdAsync(int id, CancellationToken ct) =>
        db.NhanVien.FirstOrDefaultAsync(x => x.MaNhanVien == id, ct);

    public Task<bool> EmailExistsAsync(string email, int? excludingId, CancellationToken ct) =>
        db.NhanVien.AnyAsync(x => x.Email == email && (!excludingId.HasValue || x.MaNhanVien != excludingId), ct);

    public Task<bool> PhoneExistsAsync(string phone, int? excludingId, CancellationToken ct) =>
        db.NhanVien.AnyAsync(x => x.SoDienThoai == phone && (!excludingId.HasValue || x.MaNhanVien != excludingId), ct);

    public async Task AddAsync(NhanVien employee, CancellationToken ct)
    {
        db.NhanVien.Add(employee);
        await SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new DuplicateNhanVienDataException("email hoặc số điện thoại"); }
    }

    public async Task<NhanVien?> UpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var employee = await db.NhanVien.FirstOrDefaultAsync(x => x.MaNhanVien == id, ct);
        if (employee is null) return null;

        if (!string.Equals(employee.TrangThai, status, StringComparison.Ordinal))
        {
            db.NhanVienTrangThaiLichSu.Add(new NhanVienTrangThaiLichSu
            {
                MaNhanVien = employee.MaNhanVien,
                TrangThaiCu = employee.TrangThai,
                TrangThaiMoi = status
            });
            employee.TrangThai = status;
            await db.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return employee;
    }

    public async Task<IReadOnlyList<NhanVienTrangThaiLichSu>> GetStatusHistoryAsync(int id, CancellationToken ct) =>
        await db.NhanVienTrangThaiLichSu.AsNoTracking()
            .Where(x => x.MaNhanVien == id)
            .OrderByDescending(x => x.ThayDoiLuc)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
}
