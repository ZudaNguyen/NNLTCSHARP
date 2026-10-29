
namespace QuanLyThuVien.Domain.Entities;

public class TacGia
{
    public int MaTacGia { get; set; }

    public string TenTacGia { get; set; } = string.Empty;

    public string? QuocTich { get; set; }

    public string? GioiThieu { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<SachTacGia> SachTacGias { get; set; } = new List<SachTacGia>();
}
