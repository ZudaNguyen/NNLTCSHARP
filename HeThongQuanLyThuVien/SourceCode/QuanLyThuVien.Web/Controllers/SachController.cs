using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Web.Services;
using QuanLyThuVien.Web.ViewModels;

namespace QuanLyThuVien.Web.Controllers;

[Route("sach")]
public sealed class SachController(LibraryApiClient api) : Controller
{
    private const string ApiUnavailable =
        "Không kết nối được API thư viện. Hãy chắc chắn API đang chạy ở http://localhost:5058.";

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int? maTheLoai, CancellationToken ct)
    {
        ViewData["Title"] = "Danh mục sách";
        ViewData["Role"] = "reader";

        var categories = new List<TheLoaiDto>();
        var books = new List<SachDto>();
        var apiError = default(string);

        try
        {
            categories = (await api.GetCategoriesAsync(ct) ?? []).ToList();
            books = (await api.GetBooksAsync(search, maTheLoai, ct) ?? []).ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            apiError = ApiUnavailable;
        }

        return View(new BookSearchViewModel
        {
            Search = search,
            MaTheLoai = maTheLoai,
            Books = books,
            Categories = categories,
            ApiError = apiError
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Chi tiết sách";
        ViewData["Role"] = "reader";

        SachDto? book;
        try
        {
            book = await api.GetBookAsync(id, ct);
        }
        catch (HttpRequestException)
        {
            return NotFound();
        }
        catch (TaskCanceledException)
        {
            return NotFound();
        }

        return book is null ? NotFound() : View(book);
    }
}
