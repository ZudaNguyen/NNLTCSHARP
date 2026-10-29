namespace QuanLyThuVien.Domain.Enums;

/// <summary>
/// Trạng thái lưu trong cột <c>YeuCauKhoaTaiKhoan.TrangThai</c>.
/// Phải khớp check constraint <c>CK_YeuCauKhoaTaiKhoan_TrangThai</c> trong
/// <c>LibraryDbContext.cs</c> và <c>AccountLockRequestModuleSchema.sql</c>.
/// </summary>
public static class YeuCauKhoaTaiKhoanTrangThai
{
    /// <summary>Thủ thư vừa gửi, chờ Admin duyệt.</summary>
    public const string ChoDuyet = "Chờ duyệt";

    /// <summary>Admin đã duyệt; tài khoản và thẻ độc giả đã bị khóa.</summary>
    public const string DaDuyet = "Đã duyệt";

    /// <summary>Admin từ chối; lý do nằm ở <c>LyDoTuChoi</c>.</summary>
    public const string TuChoi = "Từ chối";

    /// <summary>Thủ thư tự hủy khi yêu cầu còn chờ duyệt.</summary>
    public const string DaHuy = "Đã hủy";

    public const string All = ChoDuyet + "," + DaDuyet + "," + TuChoi + "," + DaHuy;
}
