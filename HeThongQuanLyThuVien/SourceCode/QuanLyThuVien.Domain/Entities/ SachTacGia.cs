
namespace QuanLyThuVien.Domain.Entities;

public class SachTacGia
{
    public int MaSach { get; set; }

    public int MaTacGia { get; set; }

    public Sach Sach { get; set; } = null!;

    public TacGia TacGia { get; set; } = null!;
}
