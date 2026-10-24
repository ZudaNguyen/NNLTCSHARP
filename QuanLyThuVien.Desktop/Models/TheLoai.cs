
namespace QuanLyThuVien.Desktop.Models
{
    public class TheLoai
    {
        public int MaTheLoai { get; set; }
        public string TenTheLoai { get; set; } = string.Empty;

        public override string ToString()
        {
            return TenTheLoai;
        }
    }
}
