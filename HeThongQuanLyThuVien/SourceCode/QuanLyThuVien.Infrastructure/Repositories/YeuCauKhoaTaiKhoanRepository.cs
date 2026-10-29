using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class YeuCauKhoaTaiKhoanRepository(LibraryDbContext db) : IYeuCauKhoaTaiKhoanRepository
{
    private IQueryable<YeuCauKhoaTaiKhoan> Query() => db.YeuCauKhoaTaiKhoan.AsNoTracking()
        .Include(x => x.TaiKhoan).ThenInclude(account => account.DocGia)
        .Include(x => x.NhanVien);

    public async Task<IReadOnlyList<YeuCauKhoaTaiKhoan>> SearchAsync(
        int? maNhanVien, string? trangThai, string? search, CancellationToken ct)
    {
        var query = Query();
        if (maNhanVien.HasValue) query = query.Where(x => x.MaNhanVien == maNhanVien.Value);
        if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(x => x.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var number = term.StartsWith("YCK-", StringComparison.OrdinalIgnoreCase)
                         && int.TryParse(term[4..], out var parsed)
                ? parsed.ToString()
                : term;
            query = query.Where(x =>
                x.MaYeuCau.ToString() == number ||
                x.TaiKhoan.TenDangNhap.Contains(term) ||
                x.TaiKhoan.TenHienThi.Contains(term) ||
                x.NhanVien.HoTen.Contains(term) ||
                x.LyDo.Contains(term));
        }

        return await query
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaYeuCau)
            .Take(1000)
            .ToListAsync(ct);
    }

    public Task<YeuCauKhoaTaiKhoan?> GetByIdAsync(int maYeuCau, CancellationToken ct) =>
        Query().FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau, ct);

    public async Task<IReadOnlyList<TaiKhoanDocGiaOptionDto>> FindReaderAccountsAsync(
        string? search, CancellationToken ct)
    {
        var query = db.TaiKhoan.AsNoTracking()
            .Where(account => account.VaiTro == AccountRoles.Reader && account.MaDocGia != null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(account =>
                account.TenDangNhap.Contains(term) ||
                account.TenHienThi.Contains(term) ||
                (account.DocGia != null && account.DocGia.HoTen.Contains(term)));
        }

        return await query
            .OrderBy(account => account.TenHienThi)
            .Take(300)
            .Select(account => new TaiKhoanDocGiaOptionDto
            {
                MaTaiKhoan = account.MaTaiKhoan,
                TenDangNhap = account.TenDangNhap,
                TenHienThi = account.TenHienThi,
                MaDocGia = account.MaDocGia,
                TenDocGia = account.DocGia == null ? null : account.DocGia.HoTen,
                TrangThaiTaiKhoan = account.TrangThai,
                TrangThaiDocGia = account.DocGia == null ? null : account.DocGia.TrangThai
            })
            .ToListAsync(ct);
    }

    public async Task<YeuCauKhoaTaiKhoan> CreateAsync(
        int maNhanVien, TaoYeuCauKhoaTaiKhoanDto request, Guid idempotencyKey, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var existing = await Query().FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);
        if (existing is not null)
        {
            await tx.CommitAsync(ct);
            return existing;
        }

        var requester = await db.NhanVien.FirstOrDefaultAsync(
            x => x.MaNhanVien == maNhanVien && x.TrangThai == "Hoạt động", ct)
            ?? throw new AccountLockRequestException("Không tìm thấy nhân viên đang hoạt động để gửi yêu cầu.");

        var account = await db.TaiKhoan
            .Include(x => x.DocGia)
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == request.MaTaiKhoan, ct)
            ?? throw new AccountLockRequestException("Không tìm thấy tài khoản cần khóa.");

        if (account.VaiTro != AccountRoles.Reader)
            throw new AccountLockRequestException("Chỉ được gửi yêu cầu khóa tài khoản độc giả.");
        if (account.TrangThai != "Hoạt động")
            throw new AccountLockRequestConflictException("Tài khoản này đã bị khóa.");
        if (account.DocGia is null)
            throw new AccountLockRequestException("Tài khoản độc giả chưa liên kết hồ sơ độc giả.");
        if (account.DocGia.TrangThai != "Hoạt động")
            throw new AccountLockRequestConflictException("Thẻ độc giả này đã bị khóa.");

        if (await db.YeuCauKhoaTaiKhoan.AnyAsync(
                x => x.MaTaiKhoan == request.MaTaiKhoan
                     && x.TrangThai == YeuCauKhoaTaiKhoanTrangThai.ChoDuyet, ct))
            throw new AccountLockRequestConflictException("Tài khoản này đã có một yêu cầu khóa đang chờ duyệt.");

        var entity = new YeuCauKhoaTaiKhoan
        {
            MaTaiKhoan = request.MaTaiKhoan,
            MaNhanVien = requester.MaNhanVien,
            LyDo = request.LyDo.Trim(),
            ThoiHanKhoaNgay = request.ThoiHanKhoaNgay,
            TrangThai = YeuCauKhoaTaiKhoanTrangThai.ChoDuyet,
            NgayTao = DateTime.Now,
            IdempotencyKey = idempotencyKey
        };

        db.YeuCauKhoaTaiKhoan.Add(entity);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await Query().FirstAsync(x => x.MaYeuCau == entity.MaYeuCau, ct);
    }

    public async Task<YeuCauKhoaTaiKhoan?> CancelAsync(
        int maNhanVien, int maYeuCau, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.YeuCauKhoaTaiKhoan.FirstOrDefaultAsync(
            x => x.MaYeuCau == maYeuCau && x.MaNhanVien == maNhanVien, ct);
        if (entity is null)
        {
            await tx.CommitAsync(ct);
            return null;
        }

        if (entity.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
            throw new AccountLockRequestException("Chỉ có thể hủy yêu cầu đang chờ duyệt.");

        entity.TrangThai = YeuCauKhoaTaiKhoanTrangThai.DaHuy;
        entity.NgayDuyet = DateTime.Now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
    }

    public async Task<YeuCauKhoaTaiKhoan?> ApproveAsync(
        int maYeuCau, string? nguoiDuyet, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.YeuCauKhoaTaiKhoan
            .Include(x => x.TaiKhoan).ThenInclude(account => account.DocGia)
            .FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau, ct);

        if (entity is null)
        {
            await tx.CommitAsync(ct);
            return null;
        }

        // Duyệt lại yêu cầu đã duyệt là thao tác idempotent.
        if (entity.TrangThai == YeuCauKhoaTaiKhoanTrangThai.DaDuyet)
        {
            await tx.CommitAsync(ct);
            return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
        }

        if (entity.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
            throw new AccountLockRequestException(
                $"Yêu cầu đang ở trạng thái '{entity.TrangThai}' nên không thể duyệt.");

        var account = entity.TaiKhoan
            ?? throw new AccountLockRequestException("Không tìm thấy tài khoản cần khóa.");
        if (account.VaiTro != AccountRoles.Reader)
            throw new AccountLockRequestException("Chỉ được khóa tài khoản độc giả.");

        var now = DateTime.UtcNow;
        account.TrangThai = "Bị khóa";
        account.CapNhatLucUtc = now;
        if (account.DocGia is not null) account.DocGia.TrangThai = "Khóa thẻ";

        entity.TrangThai = YeuCauKhoaTaiKhoanTrangThai.DaDuyet;
        entity.NguoiDuyet = string.IsNullOrWhiteSpace(nguoiDuyet) ? null : nguoiDuyet.Trim();
        entity.NgayDuyet = DateTime.Now;
        await db.SaveChangesAsync(ct);

        await db.TaiKhoanSession
            .Where(x => x.MaTaiKhoan == account.MaTaiKhoan && x.ThuHoiLucUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ThuHoiLucUtc, now), ct);

        await tx.CommitAsync(ct);
        return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
    }

    public async Task<YeuCauKhoaTaiKhoan?> RejectAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.YeuCauKhoaTaiKhoan.FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau, ct);
        if (entity is null)
        {
            await tx.CommitAsync(ct);
            return null;
        }

        if (entity.TrangThai == YeuCauKhoaTaiKhoanTrangThai.TuChoi)
        {
            await tx.CommitAsync(ct);
            return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
        }

        if (entity.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
            throw new AccountLockRequestException(
                $"Yêu cầu đang ở trạng thái '{entity.TrangThai}' nên không thể từ chối.");

        entity.TrangThai = YeuCauKhoaTaiKhoanTrangThai.TuChoi;
        entity.NguoiDuyet = string.IsNullOrWhiteSpace(nguoiDuyet) ? null : nguoiDuyet.Trim();
        entity.NgayDuyet = DateTime.Now;
        entity.LyDoTuChoi = string.IsNullOrWhiteSpace(lyDo)
            ? "Admin từ chối yêu cầu khóa tài khoản."
            : lyDo.Trim();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
    }
}
