using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class BorrowingRuleException(string message) : Exception(message);
public sealed class BorrowingConflictException(string message) : Exception(message);

public sealed class MuonTraService(IMuonTraRepository repository) : IMuonTraService
{
    public async Task<IReadOnlyList<MuonTraDto>> SearchAsync(
        string? search, string? trangThai, string? loaiNgay, DateTime? tuNgay, DateTime? denNgay,
        bool chiQuaHan, CancellationToken cancellationToken)
    {
        var loans = await repository.SearchAsync(search, trangThai, loaiNgay, tuNgay, denNgay, chiQuaHan, cancellationToken);
        return loans.Select(MapHeader).ToList();
    }

    public async Task<MuonTraChiTietDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var loan = await repository.GetByIdAsync(id, cancellationToken);
        return loan is null ? null : MapDetail(loan);
    }

    public async Task<IReadOnlyList<MuonTraDto>> GetForReaderAsync(int readerId, CancellationToken cancellationToken)
    {
        var loans = await repository.GetForReaderAsync(readerId, cancellationToken);
        return loans.Select(MapHeader).ToList();
    }

    public async Task<MuonTraChiTietDto?> GetByIdForReaderAsync(int readerId, int maPhieu, CancellationToken cancellationToken)
    {
        var loan = await repository.GetByIdForReaderAsync(readerId, maPhieu, cancellationToken);
        return loan is null ? null : MapDetail(loan);
    }

    public Task<IReadOnlyList<SachMuonOptionDto>> GetSachConHangAsync(CancellationToken cancellationToken) =>
        repository.GetSachConHangAsync(cancellationToken);

    public async Task<IReadOnlyList<QuaHanDto>> GetQuaHanAsync(string? search, CancellationToken cancellationToken)
    {
        var items = await repository.GetQuaHanAsync(search, cancellationToken);
        var today = DateTime.Today;
        return items.Select(item => new QuaHanDto
        {
            MaPhieu = item.MaPhieu,
            MaChiTiet = item.MaChiTiet,
            MaSach = item.MaSach,
            TenSach = item.Sach.TenSach,
            MaDocGia = item.PhieuMuon.MaDocGia,
            TenDocGia = item.PhieuMuon.DocGia.HoTen,
            NgayMuon = item.PhieuMuon.NgayMuon,
            HanTra = item.PhieuMuon.HanTra,
            SoLuongConMuon = item.SoLuongMuon - item.SoLuongDaXuLy,
            SoNgayTre = Math.Max(0, (today - item.PhieuMuon.HanTra.Date).Days)
        }).ToList();
    }

    public async Task<MuonTraChiTietDto> CreateAsync(TaoMuonTraDto request, CancellationToken cancellationToken) =>
        MapDetail(await repository.CreateAsync(request, cancellationToken));

    public async Task<MuonTraChiTietDto?> TraChiTietAsync(
        int maPhieu, int maChiTiet, TraChiTietDto request, CancellationToken cancellationToken)
    {
        var loan = await repository.TraChiTietAsync(maPhieu, maChiTiet, request, cancellationToken);
        return loan is null ? null : MapDetail(loan);
    }

    public Task<TienPhatDto?> GetTienPhatAsync(int maPhieu, CancellationToken cancellationToken) =>
        repository.GetTienPhatAsync(maPhieu, cancellationToken);

    public Task<TienPhatDto?> GetTienPhatForReaderAsync(int readerId, int maPhieu, CancellationToken cancellationToken) =>
        repository.GetTienPhatForReaderAsync(readerId, maPhieu, cancellationToken);

    public Task<TienPhatDto?> UpdateTienPhatAsync(
        int maPhieu, CapNhatTienPhatDto request, CancellationToken cancellationToken) =>
        repository.UpdateTienPhatAsync(maPhieu, request, cancellationToken);

    public Task<KhoanThuTienPhatDto> ThuTienPhatAsync(
        int maPhieu, ThuTienPhatDto request, CancellationToken cancellationToken) =>
        repository.ThuTienPhatAsync(maPhieu, request, cancellationToken);

    public Task<CauHinhTienPhatDto> GetCauHinhTienPhatAsync(CancellationToken cancellationToken) =>
        repository.GetCauHinhTienPhatAsync(cancellationToken);

    public Task<CauHinhTienPhatDto> SetCauHinhTienPhatAsync(decimal mucPhatMoiNgay, CancellationToken cancellationToken) =>
        repository.SetCauHinhTienPhatAsync(mucPhatMoiNgay, cancellationToken);

    private static MuonTraDto MapHeader(PhieuMuon loan)
    {
        var outstanding = loan.ChiTiets.Sum(item => item.SoLuongMuon - item.SoLuongDaXuLy);
        return new MuonTraDto
        {
            MaPhieu = loan.MaPhieu,
            MaDocGia = loan.MaDocGia,
            TenDocGia = loan.DocGia.HoTen,
            NguoiLap = loan.NguoiLap,
            NgayMuon = loan.NgayMuon,
            HanTra = loan.HanTra,
            SoLuongConMuon = outstanding,
            TrangThai = GetStatus(loan, outstanding)
        };
    }

    private static MuonTraChiTietDto MapDetail(PhieuMuon loan)
    {
        var header = MapHeader(loan);
        return new MuonTraChiTietDto
        {
            MaPhieu = header.MaPhieu,
            MaDocGia = header.MaDocGia,
            TenDocGia = header.TenDocGia,
            NguoiLap = header.NguoiLap,
            NgayMuon = header.NgayMuon,
            HanTra = header.HanTra,
            SoLuongConMuon = header.SoLuongConMuon,
            TrangThai = header.TrangThai,
            ChiTiet = loan.ChiTiets.OrderBy(item => item.MaChiTiet).Select(item => new ChiTietMuonTraDto
            {
                MaChiTiet = item.MaChiTiet,
                MaSach = item.MaSach,
                TenSach = item.Sach.TenSach,
                SoLuongMuon = item.SoLuongMuon,
                SoLuongDaXuLy = item.SoLuongDaXuLy,
                LichSuTra = item.LichSuTra.OrderBy(history => history.NgayTra).ThenBy(history => history.MaLanTra)
                    .Select(MapReturn).ToList()
            }).ToList()
        };
    }

    private static LanTraSachDto MapReturn(ChiTietTraSach history) => new()
    {
        MaLanTra = history.MaLanTra,
        SoLuongXuLy = history.SoLuongXuLy,
        NgayTra = history.NgayTra,
        TinhTrangSach = history.TinhTrangSach,
        SoNgayTre = history.SoNgayTre,
        MucPhatMoiNgayApDung = history.MucPhatMoiNgayApDung,
        SoTienPhatPhaiThu = history.SoTienPhatPhaiThu,
        DaThu = history.CacKhoanThu.Sum(payment => payment.SoTien),
        LichSuThu = history.CacKhoanThu.OrderBy(payment => payment.NgayThu).Select(payment => new KhoanThuTienPhatDto
        {
            MaKhoanThu = payment.MaKhoanThu, MaLanTra = payment.MaLanTra, SoTien = payment.SoTien,
            NgayThu = payment.NgayThu, NguoiThu = payment.NguoiThu
        }).ToList()
    };

    private static string GetStatus(PhieuMuon loan, int outstanding) =>
        outstanding == 0 ? "Đã hoàn tất" : loan.HanTra.Date < DateTime.Today ? "Quá hạn" : "Đang mượn";
}
