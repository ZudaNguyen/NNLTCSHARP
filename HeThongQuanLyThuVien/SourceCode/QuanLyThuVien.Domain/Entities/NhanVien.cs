namespace QuanLyThuVien.Domain.Entities;

public sealed class NhanVien
{
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string ChucVu { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "Hoạt động";
    public DateTime NgayTao { get; set; }
}
