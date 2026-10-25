namespace QuanLyThuVien.Application.DTOs;

public sealed class DashboardTongQuanDto
{
    public int SoDauSach { get; init; }
    public int TongSoBanSach { get; init; }
    public int SoBanConTrong { get; init; }
    public int SoBanDangMuon { get; init; }
    public int SoDocGiaHoatDong { get; init; }
    public int SoPhieuDangMuon { get; init; }
    public int SoPhieuQuaHan { get; init; }
    public int SoLuotMuonTrongThang { get; init; }
}

public sealed class DashboardHoatDongDto
{
    public int MaPhieu { get; init; }
    public int MaChiTiet { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public string TenSach { get; init; } = string.Empty;
    public DateTime NgayMuon { get; init; }
    public DateTime HanTra { get; init; }
    public int SoLuongConMuon { get; init; }
    public int SoNgayQuaHan { get; init; }
    public string TrangThai => SoNgayQuaHan > 0 ? $"Quá hạn {SoNgayQuaHan} ngày" : "Đang mượn";
}

public sealed class SachDangMuonBaoCaoDto
{
    public int MaPhieu { get; init; }
    public int MaChiTiet { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public DateTime NgayMuon { get; init; }
    public DateTime HanTra { get; init; }
    public int SoLuongMuon { get; init; }
    public int SoLuongConMuon { get; init; }
    public int SoNgayQuaHan { get; init; }
    public string TrangThai => SoNgayQuaHan > 0 ? $"Quá hạn {SoNgayQuaHan} ngày" : "Đang mượn";
}

public sealed class SachMuonNhieuDto
{
    public int XepHang { get; init; }
    public int MaSach { get; init; }
    public string ISBN { get; init; } = string.Empty;
    public string TenSach { get; init; } = string.Empty;
    public string TenTheLoai { get; init; } = string.Empty;
    public int SoLuotMuon { get; init; }
    public int TongSoBanMuon { get; init; }
}

public sealed class TienPhatBaoCaoDongDto
{
    public int MaLanTra { get; init; }
    public int MaPhieu { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public string TenSach { get; init; } = string.Empty;
    public DateTime NgayPhatSinh { get; init; }
    public int SoLuongXuLy { get; init; }
    public string LyDo { get; init; } = string.Empty;
    public decimal PhatSinhTrongKy { get; init; }
    public decimal SoTienPhatPhaiThu { get; init; }
    public decimal SoTienDaThuTrongKy { get; init; }
    public decimal SoTienDaThuLuyKe { get; init; }
    public decimal SoTienConNo { get; init; }
    public DateTime? NgayThuGanNhat { get; init; }
    public string TrangThaiThanhToan { get; init; } = string.Empty;
}

public sealed class TienPhatBaoCaoDto
{
    public DateOnly TuNgay { get; init; }
    public DateOnly DenNgay { get; init; }
    public string TruongNgayLoc { get; init; } = "Ngày phát sinh phạt";
    public int SoKhoanPhat { get; init; }
    public decimal TongPhatSinh { get; init; }
    public decimal TongDaThuTrongKy { get; init; }
    public decimal TongConNo { get; init; }
    public IReadOnlyList<TienPhatBaoCaoDongDto> ChiTiet { get; init; } = [];
}
