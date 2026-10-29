using System.ComponentModel.DataAnnotations;
using QuanLyThuVien.Web.Services;

namespace QuanLyThuVien.Web.ViewModels;

public sealed class BorrowRequestListViewModel
{
    /// <summary>Các yêu cầu khớp bộ lọc hiện tại.</summary>
    public IReadOnlyList<YeuCauMuonDto> Requests { get; init; } = [];
    /// <summary>Toàn bộ yêu cầu, dùng để đếm số trên từng tab.</summary>
    public IReadOnlyList<YeuCauMuonDto> AllRequests { get; init; } = [];
    /// <summary>null = tất cả; cho-duyet / da-duyet / tu-choi / da-huy.</summary>
    public string? Filter { get; init; }
    public string? ApiError { get; init; }

    public int PendingCount => AllRequests.Count(r => r.TrangThai == "Chờ duyệt");
    public int ApprovedCount => AllRequests.Count(r => r.TrangThai == "Đã duyệt");
    public int RejectedCount => AllRequests.Count(r => r.TrangThai == "Từ chối");
    public int CancelledCount => AllRequests.Count(r => r.TrangThai == "Đã hủy");
}

public sealed class BorrowRequestFormModel
{
    [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000.")]
    public int SoLuong { get; init; } = 1;

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string? GhiChu { get; init; }
}

public sealed class BookDetailsViewModel
{
    public required SachDto Book { get; init; }
    public bool IsAuthenticated { get; init; }
    public YeuCauMuonDto? PendingRequest { get; init; }
    public int PendingRequestCount { get; init; }

    /// <summary>Phiếu mượn đang còn giữ cuốn sách này (nếu có).</summary>
    public MuonTraDto? ActiveLoan { get; init; }
}
