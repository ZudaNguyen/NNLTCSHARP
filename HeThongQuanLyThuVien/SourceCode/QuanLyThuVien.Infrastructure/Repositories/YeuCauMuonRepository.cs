using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class YeuCauMuonRepository(LibraryDbContext db) : IYeuCauMuonRepository
{
    private IQueryable<YeuCauMuon> Query() => db.YeuCauMuon.AsNoTracking()
        .Include(x => x.DocGia)
        .Include(x => x.Sach);

    public async Task<IReadOnlyList<YeuCauMuon>> GetForReaderAsync(
        int readerId, string? trangThai, CancellationToken ct)
    {
        var query = Query().Where(x => x.MaDocGia == readerId);
        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(x => x.TrangThai == trangThai);
        return await query
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaYeuCau)
            .Take(500)
            .ToListAsync(ct);
    }

    public Task<YeuCauMuon?> GetByIdForReaderAsync(
        int readerId, int maYeuCau, CancellationToken ct) =>
        Query().FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau && x.MaDocGia == readerId, ct);

    public Task<YeuCauMuon?> GetByIdAsync(int maYeuCau, CancellationToken ct) =>
        Query().FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau, ct);

    public async Task<IReadOnlyList<YeuCauMuon>> SearchAsync(
        string? search, string? trangThai, CancellationToken ct)
    {
        var query = Query();
        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(x => x.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var number = term.StartsWith("YC-", StringComparison.OrdinalIgnoreCase)
                         && int.TryParse(term[3..], out var parsed)
                ? parsed.ToString()
                : term;
            query = query.Where(x =>
                x.MaYeuCau.ToString() == number ||
                x.DocGia.HoTen.Contains(term) ||
                x.DocGia.SoDienThoai.Contains(term) ||
                x.Sach.TenSach.Contains(term) ||
                x.Sach.ISBN.Contains(term));
        }

        return await query
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.MaYeuCau)
            .Take(1000)
            .ToListAsync(ct);
    }

    public async Task<YeuCauMuon> CreateAsync(
        int readerId, TaoYeuCauMuonDto request, Guid idempotencyKey, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var existing = await Query().FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);
        if (existing is not null)
        {
            await tx.CommitAsync(ct);
            return existing;
        }

        var reader = await db.DocGia.FirstOrDefaultAsync(x => x.MaDocGia == readerId, ct)
            ?? throw new BorrowingRuleException("Không tìm thấy độc giả.");
        if (reader.TrangThai != "Hoạt động")
            throw new BorrowingRuleException("Thẻ độc giả không ở trạng thái hoạt động.");

        var book = await db.Sach.FirstOrDefaultAsync(x => x.MaSach == request.MaSach && !x.IsDeleted, ct)
            ?? throw new BorrowingRuleException($"Không tìm thấy sách đang lưu hành (mã {request.MaSach}).");
        if (request.SoLuong > book.SoLuong)
            throw new BorrowingRuleException($"Sách '{book.TenSach}' chỉ có {book.SoLuong} bản trong thư viện.");

        if (await db.YeuCauMuon.AnyAsync(x =>
                x.MaDocGia == readerId &&
                x.MaSach == request.MaSach &&
                x.TrangThai == YeuCauMuonTrangThai.ChoDuyet, ct))
            throw new BorrowingConflictException("Bạn đã có một yêu cầu đang chờ duyệt cho cuốn sách này.");

        if (await db.ChiTietPhieuMuon.AnyAsync(x =>
                x.MaSach == request.MaSach &&
                x.PhieuMuon.MaDocGia == readerId &&
                x.SoLuongDaXuLy < x.SoLuongMuon, ct))
            throw new BorrowingConflictException("Bạn đang mượn cuốn sách này nên chưa thể gửi yêu cầu mới.");

        var entity = new YeuCauMuon
        {
            MaDocGia = readerId,
            MaSach = request.MaSach,
            SoLuong = request.SoLuong,
            NgayTao = DateTime.Now,
            TrangThai = YeuCauMuonTrangThai.ChoDuyet,
            GhiChu = string.IsNullOrWhiteSpace(request.GhiChu) ? null : request.GhiChu.Trim(),
            IdempotencyKey = idempotencyKey
        };

        db.YeuCauMuon.Add(entity);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await Query().FirstAsync(x => x.MaYeuCau == entity.MaYeuCau, ct);
    }

    public async Task<YeuCauMuon?> CancelAsync(int readerId, int maYeuCau, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.YeuCauMuon.FirstOrDefaultAsync(
            x => x.MaYeuCau == maYeuCau && x.MaDocGia == readerId, ct);
        if (entity is null) return null;

        if (entity.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
            throw new BorrowingRuleException("Chỉ có thể hủy yêu cầu đang chờ duyệt.");

        entity.TrangThai = YeuCauMuonTrangThai.DaHuy;
        entity.NgayDuyet = DateTime.Now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
    }

    public async Task<YeuCauMuon> MarkApprovedAsync(
        int maYeuCau, string? nguoiDuyet, int maPhieu, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.YeuCauMuon.FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau, ct)
            ?? throw new BorrowingRuleException("Không tìm thấy yêu cầu mượn.");

        if (entity.TrangThai == YeuCauMuonTrangThai.DaDuyet)
        {
            await tx.CommitAsync(ct);
            return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
        }

        if (entity.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
            throw new BorrowingRuleException(
                $"Yêu cầu đang ở trạng thái '{entity.TrangThai}' nên không thể duyệt.");

        entity.TrangThai = YeuCauMuonTrangThai.DaDuyet;
        entity.MaPhieu = maPhieu;
        entity.NguoiDuyet = string.IsNullOrWhiteSpace(nguoiDuyet) ? null : nguoiDuyet.Trim();
        entity.NgayDuyet = DateTime.Now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
    }

    public async Task<YeuCauMuon?> MarkRejectedAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.YeuCauMuon.FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau, ct);
        if (entity is null) return null;

        if (entity.TrangThai == YeuCauMuonTrangThai.TuChoi)
        {
            await tx.CommitAsync(ct);
            return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
        }

        if (entity.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
            throw new BorrowingRuleException(
                $"Yêu cầu đang ở trạng thái '{entity.TrangThai}' nên không thể từ chối.");

        entity.TrangThai = YeuCauMuonTrangThai.TuChoi;
        entity.NguoiDuyet = string.IsNullOrWhiteSpace(nguoiDuyet) ? null : nguoiDuyet.Trim();
        entity.NgayDuyet = DateTime.Now;
        entity.LyDoTuChoi = string.IsNullOrWhiteSpace(lyDo)
            ? "Thủ thư từ chối yêu cầu."
            : lyDo.Trim();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await Query().FirstAsync(x => x.MaYeuCau == maYeuCau, ct);
    }
}
