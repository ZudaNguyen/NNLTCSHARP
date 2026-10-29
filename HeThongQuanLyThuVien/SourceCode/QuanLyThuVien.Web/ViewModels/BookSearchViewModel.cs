using QuanLyThuVien.Web.Services;

namespace QuanLyThuVien.Web.ViewModels;

public sealed class BookSearchViewModel
{
    public string? Search { get; init; }
    public int? MaTheLoai { get; init; }
    public IReadOnlyList<SachDto> Books { get; init; } = [];
    public IReadOnlyList<TheLoaiDto> Categories { get; init; } = [];
    public string? ApiError { get; init; }

    public string SelectedCategoryName =>
        MaTheLoai is int id
            ? Categories.FirstOrDefault(c => c.MaTheLoai == id)?.TenTheLoai ?? "Thể loại đã chọn"
            : "Tất cả thể loại";
}
