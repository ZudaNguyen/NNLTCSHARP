using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Web.Services;
using QuanLyThuVien.Web.ViewModels;

namespace QuanLyThuVien.Web.Controllers;

[Route("sach")]
public sealed class SachController(LibraryApiClient api, ReaderSession session) : Controller
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

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest(string? q, CancellationToken ct)
    {
        var keyword = q?.Trim();
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());

        try
        {
            var books = await api.GetBooksAsync(keyword, null, ct) ?? [];
            var suggestions = books
                .Where(book => !book.IsDeleted)
                .Take(6)
                .Select(book => new
                {
                    maSach = book.MaSach,
                    tenSach = book.TenSach,
                    tenTacGia = book.TenTacGiaHienThi,
                    tenTheLoai = book.TenTheLoai,
                    soLuongCon = book.SoLuongCon,
                    conHang = book.ConHang,
                    url = Url.Action(nameof(Details), "Sach", new { id = book.MaSach }) ?? $"/sach/{book.MaSach}"
                });
            return Json(suggestions);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return Json(Array.Empty<object>());
        }
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

        if (book is null) return NotFound();

        var isAuthenticated = session.IsAuthenticated;
        YeuCauMuonDto? pendingRequest = null;
        var pendingCount = 0;
        MuonTraDto? activeLoan = null;

        if (isAuthenticated && session.Token is { Length: > 0 } token)
        {
            try
            {
                var requests = await api.GetMyBorrowRequestsAsync(token, ct: ct);
                if (requests is null)
                {
                    // Token đã bị thu hồi / hết hạn ở phía API: coi như khách.
                    await session.SignOutAsync();
                    isAuthenticated = false;
                }
                else
                {
                    pendingCount = requests.Count(request => request.CoTheHuy);
                    pendingRequest = requests.FirstOrDefault(
                        request => request.MaSach == id && request.CoTheHuy);
                }

                if (isAuthenticated)
                {
                    var loans = await api.GetMyLoansAsync(token, ct);
                    if (loans is null)
                    {
                        await session.SignOutAsync();
                        isAuthenticated = false;
                    }
                    else
                    {
                        // Chỉ phiếu còn sách chưa trả mới chặn việc đặt mượn lại.
                        foreach (var loan in loans.Where(loan => loan.SoLuongConMuon > 0))
                        {
                            var detail = await api.GetMyLoanAsync(loan.MaPhieu, token, ct);
                            if (detail?.ChiTiet.Any(item => item.MaSach == id && item.SoLuongConMuon > 0) == true)
                            {
                                activeLoan = loan;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Trang chi tiết sách vẫn hiển thị được dù API yêu cầu mượn lỗi tạm thời.
            }
        }

        ViewData["PendingRequestCount"] = pendingCount;

        return View(new BookDetailsViewModel
        {
            Book = book,
            IsAuthenticated = isAuthenticated,
            PendingRequest = pendingRequest,
            PendingRequestCount = pendingCount,
            ActiveLoan = activeLoan
        });
    }
}
