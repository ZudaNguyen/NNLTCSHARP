using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Web.Services;
using QuanLyThuVien.Web.ViewModels;

namespace QuanLyThuVien.Web.Controllers;

[Route("yeu-cau")]
public sealed class YeuCauMuonController(LibraryApiClient api, ReaderSession session) : Controller
{
    private const string ApiUnavailable =
        "Không kết nối được API thư viện. Hãy chắc chắn API đang chạy ở http://localhost:5058.";

    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> Index(string? trangThai, CancellationToken ct)
    {
        ViewData["Title"] = "Yêu cầu mượn sách";
        ViewData["Role"] = "reader";

        var token = session.Token;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Login", "TaiKhoan", new { returnUrl = "/yeu-cau" });

        IReadOnlyList<YeuCauMuonDto>? requests;
        try
        {
            requests = await api.GetMyBorrowRequestsAsync(token, ct: ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return View(new BorrowRequestListViewModel
            {
                Filter = NormalizeFilter(trangThai),
                ApiError = ApiUnavailable
            });
        }

        if (requests is null)
        {
            await session.SignOutAsync();
            return RedirectToAction("Login", "TaiKhoan", new { returnUrl = "/yeu-cau" });
        }

        var filter = NormalizeFilter(trangThai);
        var rows = filter is null
            ? requests.ToList()
            : requests.Where(r => ToStatus(filter) == r.TrangThai).ToList();

        ViewData["NotificationCount"] = requests.Count(r => r.CoTheHuy);
        ViewData["PendingRequestCount"] = requests.Count(r => r.CoTheHuy);

        return View(new BorrowRequestListViewModel
        {
            Requests = rows,
            AllRequests = requests,
            Filter = filter
        });
    }

    [Authorize]
    [HttpPost("dat/{maSach:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int maSach, BorrowRequestFormModel model, CancellationToken ct)
    {
        var token = session.Token;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Login", "TaiKhoan", new { returnUrl = $"/sach/{maSach}" });

        if (!ModelState.IsValid || model.SoLuong < 1)
        {
            TempData["BorrowMessage"] = "Số lượng không hợp lệ. Vui lòng chọn từ 1 đến 1000.";
            TempData["BorrowMessageType"] = "error";
            return RedirectToAction("Details", "Sach", new { id = maSach });
        }

        try
        {
            var result = await api.CreateBorrowRequestAsync(
                maSach, model.SoLuong, model.GhiChu, token, ct);

            TempData["BorrowMessage"] = result.Succeeded
                ? "Đã gửi yêu cầu mượn. Thủ thư sẽ duyệt và thông báo cho bạn."
                : result.Error ?? "Không gửi được yêu cầu mượn.";
            TempData["BorrowMessageType"] = result.Succeeded ? "success" : "error";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["BorrowMessage"] = ApiUnavailable;
            TempData["BorrowMessageType"] = "error";
        }

        return RedirectToAction("Details", "Sach", new { id = maSach });
    }

    [Authorize]
    [HttpPost("{id:int}/huy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? returnUrl, CancellationToken ct)
    {
        var token = session.Token;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Login", "TaiKhoan", new { returnUrl = returnUrl ?? "/yeu-cau" });

        try
        {
            var result = await api.CancelBorrowRequestAsync(id, token, ct);
            TempData["BorrowMessage"] = result.Succeeded
                ? "Đã hủy yêu cầu mượn."
                : result.Error ?? "Không hủy được yêu cầu.";
            TempData["BorrowMessageType"] = result.Succeeded ? "success" : "error";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["BorrowMessage"] = ApiUnavailable;
            TempData["BorrowMessageType"] = "error";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    private static string? NormalizeFilter(string? trangThai) => trangThai switch
    {
        "cho-duyet" => "cho-duyet",
        "da-duyet" => "da-duyet",
        "tu-choi" => "tu-choi",
        "da-huy" => "da-huy",
        _ => null
    };

    private static string ToStatus(string filter) => filter switch
    {
        "cho-duyet" => "Chờ duyệt",
        "da-duyet" => "Đã duyệt",
        "tu-choi" => "Từ chối",
        "da-huy" => "Đã hủy",
        _ => string.Empty
    };
}
