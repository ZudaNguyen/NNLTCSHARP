namespace QuanLyThuVien.Domain.Entities;

public sealed class NhanVienTrangThaiLichSu
{
    public int Id { get; set; }
    public int MaNhanVien { get; set; }
    public string TrangThaiCu { get; set; } = string.Empty;
    public string TrangThaiMoi { get; set; } = string.Empty;
    public DateTime ThayDoiLuc { get; set; }
}
