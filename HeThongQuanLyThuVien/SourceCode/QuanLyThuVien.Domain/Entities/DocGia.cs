namespace QuanLyThuVien.Domain.Entities;

public sealed class DocGia
{
    public int MaDocGia { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; }
    public string GioiTinh { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public DateTime NgayDangKy { get; set; }
    public string TrangThai { get; set; } = "Hoạt động";
}
