namespace QuanLyThuVien.Domain.Entities;

public sealed class TheLoai
{
    public int MaTheLoai { get; set; }
    public string TenTheLoai { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<Sach> Sachs { get; set; } = new List<Sach>();
}
