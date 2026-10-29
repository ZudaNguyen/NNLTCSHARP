namespace QuanLyThuVien.Domain.Entities;

public sealed class ThuTienPhat
{
    public int MaKhoanThu { get; set; }
    public int MaLanTra { get; set; }
    public decimal SoTien { get; set; }
    public DateTime NgayThu { get; set; }
    public string? NguoiThu { get; set; }
    public Guid IdempotencyKey { get; set; }
    public ChiTietTraSach LanTra { get; set; } = null!;
}
