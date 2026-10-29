namespace QuanLyThuVien.Domain.Entities;

public sealed class TaiKhoan
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string TenDangNhapChuanHoa { get; set; } = string.Empty;
    public string MatKhauHash { get; set; } = string.Empty;
    public string TenHienThi { get; set; } = string.Empty;
    public string VaiTro { get; set; } = "THU_THU";
    public string TrangThai { get; set; } = "Hoạt động";
    public int? MaNhanVien { get; set; }
    public int? MaDocGia { get; set; }
    public int SoLanDangNhapThatBai { get; set; }
    public DateTime? KhoaDenUtc { get; set; }
    public DateTime NgayTaoUtc { get; set; }
    public DateTime? CapNhatLucUtc { get; set; }
}
