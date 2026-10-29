namespace QuanLyThuVien.Desktop.Models
{
    public class Sach
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public int? NamXuatBan { get; set; }
        public string? NhaXuatBan { get; set; }

        public int SoLuong { get; set; }
        public int SoLuongCon { get; set; }

        public int? MaTheLoai { get; set; }
        public string TenTheLoai { get; set; } = string.Empty;
        public string TenTacGia { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
    }
}