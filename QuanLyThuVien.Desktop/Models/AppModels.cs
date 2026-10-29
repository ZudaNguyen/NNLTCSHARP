namespace QuanLyThuVien.Desktop.Models
{
    /// <summary>
    /// Vai trò dùng thống nhất toàn Desktop (khớp tài liệu: Admin / ThuThu).
    /// Khi có API, vai trò lấy từ claim của JWT thay vì suy ra từ tên đăng nhập.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string ThuThu = "ThuThu";

        public static string ToDisplay(string role) => role == Admin ? "Admin" : "Thủ thư";
        public static string FromDisplay(string display) => display == "Admin" ? Admin : ThuThu;
    }

    /// <summary>
    /// Quy tắc nghiệp vụ TẠM THỜI (thay cho số ghi cứng rải trong Form).
    /// Khi có API cấu hình, các giá trị này được nạp từ server và chỉ Admin được sửa.
    /// </summary>
    public static class BusinessRules
    {
        public static decimal MucPhatMoiNgay = 5000;
        public static decimal PhiSachRach = 20000;
        public static decimal PhiSachHuHong = 100000;
        public static int SoNgayMuonMacDinh = 14;
        public static int SoNgayMuonToiDa = 30;
        public static int SoSachToiDaMoiPhieu = 5;
        public const decimal TienPhatToiDa = 5000000;
        public const int MatKhauToiThieu = 6;
    }

    /// <summary>Đối tượng sách dùng trong ô chọn (thay cho việc ghép/cắt chuỗi).</summary>
    public class SachItem
    {
        public string MaSach { get; set; } = "";
        public string TenSach { get; set; } = "";
        public int SoLuongCon { get; set; }
        public override string ToString() => $"{MaSach} - {TenSach} (Còn: {SoLuongCon})";
    }

    /// <summary>Đối tượng độc giả dùng trong ô chọn.</summary>
    public class DocGiaItem
    {
        public string MaDocGia { get; set; } = "";
        public string HoTen { get; set; } = "";
        public bool HoatDong { get; set; } = true;
        public override string ToString() => $"{MaDocGia} - {HoTen}";
    }
}
