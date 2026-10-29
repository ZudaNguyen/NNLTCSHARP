using QuanLyThuVien.Web.Services;

namespace QuanLyThuVien.Web.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyList<SachDto> FeaturedBooks { get; init; } = [];
    public IReadOnlyList<SachDto> SuggestedBooks { get; init; } = [];
    public IReadOnlyList<TheLoaiDto> Categories { get; init; } = [];
    public IReadOnlyList<HomeLoanItem> CurrentLoans { get; init; } = [];
    public IReadOnlyList<HomeNotice> Notices { get; init; } = [];
    public bool IsAuthenticated { get; init; }
    public string ReaderName { get; init; } = string.Empty;
    public string? ApiError { get; init; }
    public int OverdueCount { get; init; }
}

public sealed class HomeLoanItem
{
    public required int MaPhieu { get; init; }
    public required string SoPhieu { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required DateTime HanTra { get; init; }
    public required int DaysLeft { get; init; }

    public bool IsOverdue => DaysLeft < 0;
}

public sealed class HomeNotice
{
    public required string Tone { get; init; }
    public required string Icon { get; init; }
    public required string Message { get; init; }
    public required string Detail { get; init; }
}
