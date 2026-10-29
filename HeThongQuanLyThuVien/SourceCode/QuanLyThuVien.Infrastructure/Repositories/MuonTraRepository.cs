using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class MuonTraRepository(LibraryDbContext db) : IMuonTraRepository
{
    private IQueryable<PhieuMuon> LoanQuery() => db.PhieuMuon.AsNoTracking()
        .Include(x => x.DocGia).Include(x => x.ChiTiets).ThenInclude(x => x.Sach)
        .Include(x => x.ChiTiets).ThenInclude(x => x.LichSuTra).ThenInclude(x => x.CacKhoanThu);

    public async Task<IReadOnlyList<PhieuMuon>> SearchAsync(string? search, string? trangThai, string? loaiNgay,
        DateTime? tuNgay, DateTime? denNgay, bool chiQuaHan, CancellationToken ct)
    {
        var q = LoanQuery();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var number = term.StartsWith("PM-", StringComparison.OrdinalIgnoreCase) && int.TryParse(term[3..], out var parsed)
                ? parsed.ToString() : term;
            q = q.Where(x => x.MaPhieu.ToString() == number || x.DocGia.HoTen.Contains(term) ||
                (x.DocGia.SoDienThoai != null && x.DocGia.SoDienThoai.Contains(term)));
        }
        if (tuNgay.HasValue || denNgay.HasValue)
        {
            q = loaiNgay == "HanTra"
                ? q.Where(x => (!tuNgay.HasValue || x.HanTra >= tuNgay.Value.Date) && (!denNgay.HasValue || x.HanTra <= denNgay.Value.Date))
                : q.Where(x => (!tuNgay.HasValue || x.NgayMuon >= tuNgay.Value.Date) && (!denNgay.HasValue || x.NgayMuon <= denNgay.Value.Date));
        }
        if (chiQuaHan || trangThai == "Quá hạn") q = q.Where(x => x.HanTra < DateTime.Today && x.ChiTiets.Any(d => d.SoLuongDaXuLy < d.SoLuongMuon));
        if (trangThai == "Đã hoàn tất") q = q.Where(x => x.ChiTiets.All(d => d.SoLuongDaXuLy >= d.SoLuongMuon));
        if (trangThai == "Đang mượn") q = q.Where(x => x.HanTra >= DateTime.Today && x.ChiTiets.Any(d => d.SoLuongDaXuLy < d.SoLuongMuon));
        return await q.OrderByDescending(x => x.MaPhieu).Take(1000).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PhieuMuon>> GetForReaderAsync(int readerId, CancellationToken ct) =>
        await LoanQuery().Where(x => x.MaDocGia == readerId)
            .OrderByDescending(x => x.NgayMuon).ThenByDescending(x => x.MaPhieu).Take(1000).ToListAsync(ct);

    public Task<PhieuMuon?> GetByIdForReaderAsync(int readerId, int maPhieu, CancellationToken ct) =>
        LoanQuery().FirstOrDefaultAsync(x => x.MaDocGia == readerId && x.MaPhieu == maPhieu, ct);

    public Task<PhieuMuon?> GetByIdAsync(int id, CancellationToken ct) => LoanQuery().FirstOrDefaultAsync(x => x.MaPhieu == id, ct);

    public async Task<IReadOnlyList<SachMuonOptionDto>> GetSachConHangAsync(CancellationToken ct) => await db.Sach.AsNoTracking()
        .Where(x => !x.IsDeleted && x.SoLuongCon > 0).OrderBy(x => x.TenSach)
        .Select(x => new SachMuonOptionDto { MaSach = x.MaSach, TenSach = x.TenSach, ISBN = x.ISBN, SoLuongCon = x.SoLuongCon }).ToListAsync(ct);

    public async Task<IReadOnlyList<ChiTietPhieuMuon>> GetQuaHanAsync(string? search, CancellationToken ct)
    {
        var q = db.ChiTietPhieuMuon.AsNoTracking().Include(x => x.Sach).Include(x => x.PhieuMuon).ThenInclude(x => x.DocGia)
            .Where(x => x.SoLuongDaXuLy < x.SoLuongMuon && x.PhieuMuon.HanTra < DateTime.Today);
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Sach.TenSach.Contains(search) || x.PhieuMuon.DocGia.HoTen.Contains(search));
        return await q.OrderBy(x => x.PhieuMuon.HanTra).ToListAsync(ct);
    }

    public async Task<PhieuMuon> CreateAsync(TaoMuonTraDto request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var existing = await LoanQuery().FirstOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey, ct);
        if (existing is not null) { await tx.CommitAsync(ct); return existing; }
        var reader = await db.DocGia.FirstOrDefaultAsync(x => x.MaDocGia == request.MaDocGia, ct)
            ?? throw new BorrowingRuleException("Không tìm thấy độc giả.");
        if (reader.TrangThai != "Hoạt động") throw new BorrowingRuleException("Độc giả không ở trạng thái hoạt động.");
        if (request.NgayMuon.Date > DateTime.Today) throw new BorrowingRuleException("Ngày mượn không được ở tương lai.");
        var loan = new PhieuMuon { MaDocGia = request.MaDocGia, NguoiLap = request.NguoiLap?.Trim(), NgayMuon = request.NgayMuon.Date, HanTra = request.HanTra.Date, IdempotencyKey = request.IdempotencyKey };
        foreach (var line in request.ChiTiet)
        {
            var book = await db.Sach.FirstOrDefaultAsync(x => x.MaSach == line.MaSach && !x.IsDeleted, ct)
                ?? throw new BorrowingRuleException($"Không tìm thấy sách đang hoạt động (mã {line.MaSach}).");
            if (book.SoLuongCon < line.SoLuong) throw new BorrowingConflictException($"Sách '{book.TenSach}' chỉ còn {book.SoLuongCon} bản.");
            book.SoLuongCon -= line.SoLuong;
            loan.ChiTiets.Add(new ChiTietPhieuMuon { MaSach = line.MaSach, SoLuongMuon = line.SoLuong });
        }
        db.PhieuMuon.Add(loan);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return (await GetByIdAsync(loan.MaPhieu, ct))!;
    }

    public async Task<PhieuMuon?> GiaHanAsync(int maPhieu, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var loan = await db.PhieuMuon
            .Include(x => x.ChiTiets).ThenInclude(x => x.LichSuTra).ThenInclude(x => x.CacKhoanThu)
            .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu, ct);
        if (loan is null) return null;

        var outstanding = loan.ChiTiets.Sum(x => x.SoLuongMuon - x.SoLuongDaXuLy);
        var conNo = GiaHanMuonPolicy.TinhConNo(loan.ChiTiets.SelectMany(x => x.LichSuTra));
        var error = GiaHanMuonPolicy.KiemTra(outstanding, loan.HanTra, loan.SoLanGiaHan, conNo, DateTime.Today);
        if (error is not null) throw new BorrowingRuleException(error);

        loan.HanTra = loan.HanTra.AddDays(GiaHanMuonPolicy.SoNgayMoiLan);
        loan.SoLanGiaHan += 1;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await GetByIdAsync(maPhieu, ct);
    }

    public async Task<PhieuMuon?> TraChiTietAsync(int maPhieu, int maChiTiet, TraChiTietDto request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var duplicate = await db.ChiTietTraSach.AnyAsync(x => x.IdempotencyKey == request.IdempotencyKey, ct);
        if (!duplicate)
        {
            var line = await db.ChiTietPhieuMuon.Include(x => x.PhieuMuon).Include(x => x.Sach)
                .FirstOrDefaultAsync(x => x.MaChiTiet == maChiTiet && x.MaPhieu == maPhieu, ct);
            if (line is null) return null;
            if (request.NgayTra.Date > DateTime.Today) throw new BorrowingRuleException("Ngày trả không được ở tương lai.");
            if (request.SoLuongXuLy > line.SoLuongMuon - line.SoLuongDaXuLy) throw new BorrowingConflictException("Số lượng trả vượt quá số lượng còn mượn.");
            var damaged = request.TinhTrangSach is "Hỏng nặng" or "Mất";
            if (!damaged && line.Sach.SoLuongCon + request.SoLuongXuLy > line.Sach.SoLuong)
                throw new BorrowingConflictException("Tồn kho sẽ vượt quá tổng số lượng sách.");
            line.SoLuongDaXuLy += request.SoLuongXuLy;
            if (damaged) line.Sach.SoLuong -= request.SoLuongXuLy;
            else line.Sach.SoLuongCon += request.SoLuongXuLy;
            var lateDays = Math.Max(0, (request.NgayTra.Date - line.PhieuMuon.HanTra.Date).Days);
            var rate = await db.CauHinhTienPhat.AsNoTracking().Where(x => x.Id == 1).Select(x => (decimal?)x.MucPhatMoiNgay).FirstOrDefaultAsync(ct);
            var fineAmount = rate.HasValue ? lateDays * request.SoLuongXuLy * rate.Value : (decimal?)null;
            if (request.SoTienThu > 0 && !fineAmount.HasValue)
                throw new BorrowingRuleException("Chưa cấu hình mức phạt nên không thể thu tiền phạt.");
            if (request.SoTienThu > fineAmount.GetValueOrDefault())
                throw new BorrowingConflictException("Số tiền thu vượt quá tiền phạt của lần trả này.");

            var returnRecord = new ChiTietTraSach { MaChiTiet = maChiTiet, SoLuongXuLy = request.SoLuongXuLy,
                NgayTra = request.NgayTra.Date, TinhTrangSach = request.TinhTrangSach, SoNgayTre = lateDays,
                MucPhatMoiNgayApDung = rate, SoTienPhatPhaiThu = fineAmount,
                IdempotencyKey = request.IdempotencyKey };
            db.ChiTietTraSach.Add(returnRecord);
            await db.SaveChangesAsync(ct);
            if (request.SoTienThu > 0)
            {
                db.ThuTienPhat.Add(new ThuTienPhat
                {
                    MaLanTra = returnRecord.MaLanTra,
                    SoTien = request.SoTienThu,
                    NgayThu = DateTime.Now,
                    NguoiThu = request.NguoiThu?.Trim(),
                    IdempotencyKey = request.IdempotencyKey
                });
                await db.SaveChangesAsync(ct);
            }
        }
        await tx.CommitAsync(ct);
        return await GetByIdAsync(maPhieu, ct);
    }

    public async Task<TienPhatDto?> GetTienPhatAsync(int maPhieu, CancellationToken ct)
    {
        if (!await db.PhieuMuon.AnyAsync(x => x.MaPhieu == maPhieu, ct)) return null;
        var items = await db.ChiTietTraSach.AsNoTracking().Include(x => x.CacKhoanThu).Include(x => x.ChiTiet)
            .Where(x => x.ChiTiet.MaPhieu == maPhieu).OrderBy(x => x.MaLanTra).ToListAsync(ct);
        var amounts = items.Where(x => x.SoTienPhatPhaiThu.HasValue).Sum(x => x.SoTienPhatPhaiThu!.Value);
        var unknown = items.Any(x => !x.SoTienPhatPhaiThu.HasValue && x.SoNgayTre > 0);
        return new TienPhatDto { MaPhieu = maPhieu, CacKhoanPhat = items.Select(ToReturnDto).ToList(), TongPhaiThu = unknown ? null : amounts,
            DaThu = items.SelectMany(x => x.CacKhoanThu).Sum(x => x.SoTien), ChuaCauHinhMucPhat = unknown };
    }

    public async Task<TienPhatDto?> GetTienPhatForReaderAsync(int readerId, int maPhieu, CancellationToken ct)
    {
        if (!await db.PhieuMuon.AsNoTracking().AnyAsync(x => x.MaPhieu == maPhieu && x.MaDocGia == readerId, ct))
            return null;
        return await GetTienPhatAsync(maPhieu, ct);
    }

    public async Task<TienPhatDto?> UpdateTienPhatAsync(int maPhieu, CapNhatTienPhatDto request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await db.ChiTietTraSach.Include(x => x.CacKhoanThu).Include(x => x.ChiTiet)
            .FirstOrDefaultAsync(x => x.MaLanTra == request.MaLanTra && x.ChiTiet.MaPhieu == maPhieu, ct);
        if (item is null) return null;
        if (request.SoTienPhatPhaiThu < item.CacKhoanThu.Sum(x => x.SoTien)) throw new BorrowingConflictException("Không thể giảm tiền phạt thấp hơn số tiền đã thu.");
        item.SoTienPhatPhaiThu = request.SoTienPhatPhaiThu;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return await GetTienPhatAsync(maPhieu, ct);
    }

    public async Task<KhoanThuTienPhatDto> ThuTienPhatAsync(int maPhieu, ThuTienPhatDto request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var repeated = await db.ThuTienPhat.FirstOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey, ct);
        if (repeated is not null) { await tx.CommitAsync(ct); return ToPaymentDto(repeated); }
        var item = await db.ChiTietTraSach.Include(x => x.CacKhoanThu).Include(x => x.ChiTiet)
            .FirstOrDefaultAsync(x => x.MaLanTra == request.MaLanTra && x.ChiTiet.MaPhieu == maPhieu, ct)
            ?? throw new BorrowingRuleException("Không tìm thấy khoản phạt.");
        if (!item.SoTienPhatPhaiThu.HasValue) throw new BorrowingRuleException("Chưa cấu hình hoặc chưa xác định tiền phạt.");
        var debt = item.SoTienPhatPhaiThu.Value - item.CacKhoanThu.Sum(x => x.SoTien);
        if (request.SoTien > debt) throw new BorrowingConflictException("Số thu vượt quá khoản còn nợ.");
        var payment = new ThuTienPhat { MaLanTra = item.MaLanTra, SoTien = request.SoTien, NgayThu = DateTime.Now,
            NguoiThu = request.NguoiThu?.Trim(), IdempotencyKey = request.IdempotencyKey };
        db.ThuTienPhat.Add(payment); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ToPaymentDto(payment);
    }

    public async Task<CauHinhTienPhatDto> GetCauHinhTienPhatAsync(CancellationToken ct)
    {
        var row = await db.CauHinhTienPhat.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1, ct);
        return new() { MucPhatMoiNgay = row?.MucPhatMoiNgay, CapNhatLuc = row?.CapNhatLuc };
    }

    public async Task<CauHinhTienPhatDto> SetCauHinhTienPhatAsync(decimal rate, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await db.CauHinhTienPhat.FirstOrDefaultAsync(x => x.Id == 1, ct);
        if (row is null) { row = new CauHinhTienPhat { Id = 1, MucPhatMoiNgay = rate, CapNhatLuc = DateTime.Now }; db.CauHinhTienPhat.Add(row); }
        else { row.MucPhatMoiNgay = rate; row.CapNhatLuc = DateTime.Now; }
        var pending = await db.ChiTietTraSach.Where(x => x.SoTienPhatPhaiThu == null && x.SoNgayTre > 0).ToListAsync(ct);
        foreach (var item in pending) { item.MucPhatMoiNgayApDung = rate; item.SoTienPhatPhaiThu = item.SoNgayTre * item.SoLuongXuLy * rate; }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return new() { MucPhatMoiNgay = row.MucPhatMoiNgay, CapNhatLuc = row.CapNhatLuc };
    }

    private static LanTraSachDto ToReturnDto(ChiTietTraSach x) => new() { MaLanTra = x.MaLanTra, SoLuongXuLy = x.SoLuongXuLy,
        NgayTra = x.NgayTra, TinhTrangSach = x.TinhTrangSach, SoNgayTre = x.SoNgayTre, MucPhatMoiNgayApDung = x.MucPhatMoiNgayApDung,
        SoTienPhatPhaiThu = x.SoTienPhatPhaiThu, DaThu = x.CacKhoanThu.Sum(p => p.SoTien) };
    private static KhoanThuTienPhatDto ToPaymentDto(ThuTienPhat x) => new() { MaKhoanThu = x.MaKhoanThu, MaLanTra = x.MaLanTra, SoTien = x.SoTien, NgayThu = x.NgayThu, NguoiThu = x.NguoiThu };
}
