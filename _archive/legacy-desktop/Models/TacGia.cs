
namespace QuanLyThuVien.Desktop.Models
{
    public class TacGia
    {
        public int MaTacGia { get; set; }
        public string TenTacGia { get; set; } = string.Empty;

        public override string ToString()
        {
            return TenTacGia;
        }
    }
}
