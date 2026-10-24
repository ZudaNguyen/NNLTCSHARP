namespace QuanLyThuVien.Domain.Entities;

public sealed class Sach
{
    public int MaSach { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string TenSach { get; set; } = string.Empty;
    public int MaTheLoai { get; set; }
    public int NamXB { get; set; }
    public string? NhaXB { get; set; }
    public int SoLuong { get; set; }
    public int SoLuongCon { get; set; }
    public string? MoTa { get; set; }
    public string? HinhAnh { get; set; }
    public bool IsDeleted { get; set; }

    public TheLoai? TheLoai { get; set; }
    public ICollection<SachTacGia> SachTacGias { get; set; } = new List<SachTacGia>();
}
