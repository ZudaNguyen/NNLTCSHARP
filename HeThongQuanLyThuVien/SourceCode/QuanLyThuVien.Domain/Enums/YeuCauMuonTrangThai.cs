namespace QuanLyThuVien.Domain.Enums;

/// <summary>
/// Trạng thái lưu trong cột <c>YeuCauMuon.TrangThai</c>.
/// Phải khớp check constraint <c>CK_YeuCauMuon_TrangThai</c> trong
/// <c>LibraryDbContext.cs</c> và <c>BorrowRequestModuleSchema.sql</c>.
/// </summary>
public static class YeuCauMuonTrangThai
{
    /// <summary>Độc giả vừa gửi, chờ thủ thư xử lý.</summary>
    public const string ChoDuyet = "Chờ duyệt";

    /// <summary>Thủ thư đã duyệt và hệ thống đã tạo phiếu mượn.</summary>
    public const string DaDuyet = "Đã duyệt";

    /// <summary>Thủ thư từ chối; lý do nằm ở <c>LyDoTuChoi</c>.</summary>
    public const string TuChoi = "Từ chối";

    /// <summary>Độc giả tự hủy khi yêu cầu còn chờ duyệt.</summary>
    public const string DaHuy = "Đã hủy";

    public const string All = ChoDuyet + "," + DaDuyet + "," + TuChoi + "," + DaHuy;
}
