using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class ThongKeRepository(LibraryDbContext db) : IThongKeRepository
{
    public async Task<DashboardTongQuanDto> GetDashboardTongQuanAsync(DateTime ngayHienTai, DateTime dauThang, DateTime dauThangSau, CancellationToken ct)
    {
        var activeBooks = db.Sach.AsNoTracking().Where(x => !x.IsDeleted);
        var loanDetails = db.ChiTietPhieuMuon.AsNoTracking().Where(x => x.SoLuongMuon > x.SoLuongDaXuLy);
        var activeLoans = db.PhieuMuon.AsNoTracking().Where(x => x.ChiTiets.Any(d => d.SoLuongMuon > d.SoLuongDaXuLy));

        return new DashboardTongQuanDto
        {
            SoDauSach = await activeBooks.CountAsync(ct),
            TongSoBanSach = await activeBooks.SumAsync(x => (int?)x.SoLuong, ct) ?? 0,
            SoBanConTrong = await activeBooks.SumAsync(x => (int?)x.SoLuongCon, ct) ?? 0,
            SoBanDangMuon = await loanDetails.SumAsync(x => (int?)(x.SoLuongMuon - x.SoLuongDaXuLy), ct) ?? 0,
            SoDocGiaHoatDong = await db.DocGia.AsNoTracking().CountAsync(x => x.TrangThai == "Hoạt động", ct),
            SoPhieuDangMuon = await activeLoans.CountAsync(ct),
            SoPhieuQuaHan = await activeLoans.CountAsync(x => x.HanTra < ngayHienTai, ct),
            SoLuotMuonTrongThang = await db.PhieuMuon.AsNoTracking().CountAsync(x => x.NgayMuon >= dauThang && x.NgayMuon < dauThangSau, ct)
        };
    }

    public async Task<IReadOnlyList<DashboardHoatDongDto>> GetDashboardHoatDongAsync(DateTime ngayHienTai, int take, CancellationToken ct)
    {
        var rows = await db.ChiTietPhieuMuon.AsNoTracking()
            .Where(x => x.SoLuongMuon > x.SoLuongDaXuLy)
            .OrderByDescending(x => x.PhieuMuon.HanTra < ngayHienTai)
            .ThenBy(x => x.PhieuMuon.HanTra)
            .ThenByDescending(x => x.PhieuMuon.NgayMuon)
            .Select(x => new
            {
                x.MaPhieu,
                x.MaChiTiet,
                TenDocGia = x.PhieuMuon.DocGia.HoTen,
                TenSach = x.Sach.TenSach,
                x.PhieuMuon.NgayMuon,
                x.PhieuMuon.HanTra,
                SoLuongConMuon = x.SoLuongMuon - x.SoLuongDaXuLy
            })
            .Take(take)
            .ToListAsync(ct);

        return rows.Select(x => new DashboardHoatDongDto
        {
            MaPhieu = x.MaPhieu,
            MaChiTiet = x.MaChiTiet,
            TenDocGia = x.TenDocGia,
            TenSach = x.TenSach,
            NgayMuon = x.NgayMuon,
            HanTra = x.HanTra,
            SoLuongConMuon = x.SoLuongConMuon,
            SoNgayQuaHan = x.HanTra.Date < ngayHienTai.Date ? (ngayHienTai.Date - x.HanTra.Date).Days : 0
        }).ToList();
    }

    public async Task<IReadOnlyList<SachDangMuonBaoCaoDto>> GetSachDangMuonAsync(DateTime from, DateTime toExclusive, DateTime ngayHienTai, CancellationToken ct)
    {
        var rows = await db.ChiTietPhieuMuon.AsNoTracking()
            .Where(x => x.SoLuongMuon > x.SoLuongDaXuLy
                && x.PhieuMuon.NgayMuon >= from && x.PhieuMuon.NgayMuon < toExclusive)
            .OrderBy(x => x.PhieuMuon.HanTra)
            .ThenBy(x => x.MaPhieu)
            .ThenBy(x => x.MaChiTiet)
            .Select(x => new
            {
                x.MaPhieu,
                x.MaChiTiet,
                TenDocGia = x.PhieuMuon.DocGia.HoTen,
                x.MaSach,
                TenSach = x.Sach.TenSach,
                x.PhieuMuon.NgayMuon,
                x.PhieuMuon.HanTra,
                x.SoLuongMuon,
                SoLuongConMuon = x.SoLuongMuon - x.SoLuongDaXuLy
            })
            .Take(5000)
            .ToListAsync(ct);

        return rows.Select(x => new SachDangMuonBaoCaoDto
        {
            MaPhieu = x.MaPhieu,
            MaChiTiet = x.MaChiTiet,
            TenDocGia = x.TenDocGia,
            MaSach = x.MaSach,
            TenSach = x.TenSach,
            NgayMuon = x.NgayMuon,
            HanTra = x.HanTra,
            SoLuongMuon = x.SoLuongMuon,
            SoLuongConMuon = x.SoLuongConMuon,
            SoNgayQuaHan = x.HanTra.Date < ngayHienTai.Date ? (ngayHienTai.Date - x.HanTra.Date).Days : 0
        }).ToList();
    }

    public async Task<IReadOnlyList<SachMuonNhieuDto>> GetSachMuonNhieuAsync(DateTime from, DateTime toExclusive, int top, CancellationToken ct)
    {
        var grouped = await db.ChiTietPhieuMuon.AsNoTracking()
            .Where(x => !x.Sach.IsDeleted && x.PhieuMuon.NgayMuon >= from && x.PhieuMuon.NgayMuon < toExclusive)
            .GroupBy(x => new { x.MaSach, x.Sach.ISBN, x.Sach.TenSach, TenTheLoai = x.Sach.TheLoai == null ? "" : x.Sach.TheLoai.TenTheLoai })
            .Select(g => new
            {
                g.Key.MaSach,
                g.Key.ISBN,
                g.Key.TenSach,
                g.Key.TenTheLoai,
                SoLuotMuon = g.Count(),
                TongSoBanMuon = g.Sum(x => x.SoLuongMuon)
            })
            .OrderByDescending(x => x.SoLuotMuon)
            .ThenByDescending(x => x.TongSoBanMuon)
            .ThenBy(x => x.TenSach)
            .Take(top)
            .ToListAsync(ct);

        return grouped.Select((x, index) => new SachMuonNhieuDto
        {
            XepHang = index + 1,
            MaSach = x.MaSach,
            ISBN = x.ISBN,
            TenSach = x.TenSach,
            TenTheLoai = x.TenTheLoai,
            SoLuotMuon = x.SoLuotMuon,
            TongSoBanMuon = x.TongSoBanMuon
        }).ToList();
    }

    public async Task<TienPhatBaoCaoDto> GetTienPhatAsync(DateOnly from, DateOnly to, DateTime fromDateTime, DateTime toExclusive, CancellationToken ct)
    {
        var query = db.ChiTietTraSach.AsNoTracking()
            .Where(x => x.SoTienPhatPhaiThu > 0 &&
                ((x.NgayTra >= fromDateTime && x.NgayTra < toExclusive) ||
                 x.CacKhoanThu.Any(p => p.NgayThu >= fromDateTime && p.NgayThu < toExclusive)));

        var rows = await query
            .OrderBy(x => x.NgayTra)
            .ThenBy(x => x.MaLanTra)
            .Select(x => new
            {
                x.MaLanTra,
                MaPhieu = x.ChiTiet.MaPhieu,
                TenDocGia = x.ChiTiet.PhieuMuon.DocGia.HoTen,
                TenSach = x.ChiTiet.Sach.TenSach,
                x.NgayTra,
                x.SoLuongXuLy,
                x.TinhTrangSach,
                SoNgayTre = x.SoNgayTre,
                PhatSinhTrongKy = x.NgayTra >= fromDateTime && x.NgayTra < toExclusive ? x.SoTienPhatPhaiThu ?? 0m : 0m,
                SoTienPhatPhaiThu = x.SoTienPhatPhaiThu ?? 0m,
                SoTienDaThuTrongKy = x.CacKhoanThu.Where(p => p.NgayThu >= fromDateTime && p.NgayThu < toExclusive).Sum(p => (decimal?)p.SoTien) ?? 0m,
                SoTienDaThuLuyKe = x.CacKhoanThu.Sum(p => (decimal?)p.SoTien) ?? 0m,
                NgayThuGanNhat = x.CacKhoanThu.Max(p => (DateTime?)p.NgayThu)
            })
            .Take(5000)
            .ToListAsync(ct);

        var details = rows.Select(x =>
        {
            var outstanding = Math.Max(0m, x.SoTienPhatPhaiThu - x.SoTienDaThuLuyKe);
            return new TienPhatBaoCaoDongDto
            {
                MaLanTra = x.MaLanTra,
                MaPhieu = x.MaPhieu,
                TenDocGia = x.TenDocGia,
                TenSach = x.TenSach,
                NgayPhatSinh = x.NgayTra,
                SoLuongXuLy = x.SoLuongXuLy,
                LyDo = $"{x.TinhTrangSach}{(x.SoNgayTre > 0 ? $"; trễ {x.SoNgayTre} ngày" : "")}",
                PhatSinhTrongKy = x.PhatSinhTrongKy,
                SoTienPhatPhaiThu = x.SoTienPhatPhaiThu,
                SoTienDaThuTrongKy = x.SoTienDaThuTrongKy,
                SoTienDaThuLuyKe = x.SoTienDaThuLuyKe,
                SoTienConNo = outstanding,
                NgayThuGanNhat = x.NgayThuGanNhat,
                TrangThaiThanhToan = outstanding == 0 ? "Đã thu đủ" : x.SoTienDaThuLuyKe > 0 ? "Đã thu một phần" : "Chưa thu"
            };
        }).ToList();

        return new TienPhatBaoCaoDto
        {
            TuNgay = from,
            DenNgay = to,
            SoKhoanPhat = details.Count(x => x.PhatSinhTrongKy > 0),
            TongPhatSinh = details.Sum(x => x.PhatSinhTrongKy),
            TongDaThuTrongKy = details.Sum(x => x.SoTienDaThuTrongKy),
            TongConNo = details.Where(x => x.PhatSinhTrongKy > 0).Sum(x => x.SoTienConNo),
            ChiTiet = details
        };
    }
}
