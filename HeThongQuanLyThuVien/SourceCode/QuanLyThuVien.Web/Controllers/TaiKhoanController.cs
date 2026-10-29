using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Web.Services;
using QuanLyThuVien.Web.ViewModels;

namespace QuanLyThuVien.Web.Controllers;

[Route("")]
public sealed class TaiKhoanController(LibraryApiClient api, ReaderSession session) : Controller
{
    private const string ApiUnavailable =
        "Không kết nối được API thư viện. Hãy chắc chắn API đang chạy ở http://localhost:5058.";

    // ---------- Đăng nhập / đăng xuất ----------

    [HttpGet("dang-nhap")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Đăng nhập độc giả";

        if (session.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("dang-nhap")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Đăng nhập độc giả";

        if (!ModelState.IsValid)
            return View(model);

        LoginResult result;
        try
        {
            result = await api.LoginAsync(model.TenDangNhap.Trim(), model.MatKhau, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ModelState.AddModelError(string.Empty, ApiUnavailable);
            return View(model);
        }

        if (!result.Succeeded || result.Payload is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Không đăng nhập được.");
            return View(model);
        }

        var user = result.Payload.User;
        if (!ReaderSession.IsReaderRole(user.VaiTro))
        {
            ModelState.AddModelError(string.Empty,
                "Tài khoản này không phải độc giả. Vui lòng dùng ứng dụng Desktop để quản trị.");
            return View(model);
        }

        await session.SignInAsync(user, result.Payload.AccessToken, result.Payload.ExpiresAtUtc);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("dang-ky")]
    public IActionResult Register()
    {
        ViewData["Title"] = "Đăng ký độc giả";

        if (session.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost("dang-ky")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Đăng ký độc giả";

        if (!ModelState.IsValid)
            return View(model);

        LoginResult result;
        try
        {
            result = await api.RegisterReaderAsync(new RegisterReaderRequest
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                MatKhau = model.MatKhau,
                HoTen = model.HoTen.Trim(),
                NgaySinh = model.NgaySinh.Date,
                GioiTinh = model.GioiTinh,
                SoDienThoai = model.SoDienThoai.Trim(),
                Email = model.Email.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim()
            }, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ModelState.AddModelError(string.Empty, ApiUnavailable);
            return View(model);
        }

        if (!result.Succeeded || result.Payload is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Không đăng ký được.");
            return View(model);
        }

        var user = result.Payload.User;
        if (!ReaderSession.IsReaderRole(user.VaiTro))
        {
            ModelState.AddModelError(string.Empty,
                "Tài khoản vừa tạo không phải độc giả. Vui lòng liên hệ thủ thư.");
            return View(model);
        }

        await session.SignInAsync(user, result.Payload.AccessToken, result.Payload.ExpiresAtUtc);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost("dang-xuat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (session.Token is { Length: > 0 } token)
        {
            try
            {
                await api.LogoutAsync(token);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // API không phản hồi thì vẫn xóa cookie ở phía Web.
            }
        }

        await session.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ---------- Lịch sử mượn ----------

    [Authorize]
    [HttpGet("lich-su-muon")]
    public async Task<IActionResult> LichSuMuon(string? trangThai, CancellationToken ct)
    {
        ViewData["Title"] = "Lịch sử mượn trả";
        ViewData["Role"] = "reader";

        var token = session.Token;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login), new { returnUrl = "/lich-su-muon" });

        IReadOnlyList<MuonTraDto>? loans;
        try
        {
            loans = await api.GetMyLoansAsync(token, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return View(new LoanListViewModel
            {
                Filter = NormalizeFilter(trangThai),
                ApiError = ApiUnavailable
            });
        }

        if (loans is null)
        {
            // Phiên API đã hết hạn → xóa cookie và yêu cầu đăng nhập lại.
            await session.SignOutAsync();
            return RedirectToAction(nameof(Login), new { returnUrl = "/lich-su-muon" });
        }

        var filter = NormalizeFilter(trangThai);
        var rows = loans
            .Where(l => filter != "dang-muon" || l.SoLuongConMuon > 0)
            .Select(l => new LoanRowViewModel
            {
                MaPhieu = l.MaPhieu,
                SoPhieu = l.SoPhieu,
                NgayMuon = l.NgayMuon,
                HanTra = l.HanTra,
                SoLuongConMuon = l.SoLuongConMuon,
                TrangThai = l.TrangThai
            })
            .OrderByDescending(l => l.IsActive)
            .ThenBy(l => l.HanTra)
            .ToList();

        ViewData["NotificationCount"] = rows.Count(l => l.IsOverdue);
        await SetPendingRequestBadgeAsync(token, ct);

        return View(new LoanListViewModel { Loans = rows, Filter = filter });
    }

    [Authorize]
    [HttpGet("lich-su-muon/{id:int}")]
    public async Task<IActionResult> ChiTietMuon(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Chi tiết phiếu mượn";
        ViewData["Role"] = "reader";

        var token = session.Token;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login), new { returnUrl = $"/lich-su-muon/{id}" });

        try
        {
            var loan = await api.GetMyLoanAsync(id, token, ct);
            if (loan is null)
            {
                await session.SignOutAsync();
                return RedirectToAction(nameof(Login), new { returnUrl = $"/lich-su-muon/{id}" });
            }

            var fine = await api.GetMyFineAsync(id, token, ct);
            ViewData["NotificationCount"] = fine?.ConNo > 0 ? 1 : 0;
            await SetPendingRequestBadgeAsync(token, ct);
            return View(new LoanDetailViewModel { Loan = loan, Fine = fine });
        }
        catch (HttpRequestException)
        {
            return NotFound();
        }
        catch (TaskCanceledException)
        {
            return NotFound();
        }
    }

    // ---------- Thông tin cá nhân ----------

    [Authorize]
    [HttpGet("tai-khoan")]
    public async Task<IActionResult> HoSo(CancellationToken ct)
    {
        ViewData["Title"] = "Thông tin cá nhân";
        ViewData["Role"] = "reader";

        var token = session.Token;
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login), new { returnUrl = "/tai-khoan" });

        try
        {
            var profile = await api.GetReaderProfileAsync(token, ct);
            if (profile is null)
            {
                await session.SignOutAsync();
                return RedirectToAction(nameof(Login), new { returnUrl = "/tai-khoan" });
            }

            var loans = await api.GetMyLoansAsync(token, ct) ?? [];
            await SetPendingRequestBadgeAsync(token, ct);
            return View(new ProfileViewModel
            {
                Profile = profile,
                RecentLoans = loans.OrderByDescending(l => l.NgayMuon).Take(5).ToList()
            });
        }
        catch (HttpRequestException)
        {
            return NotFound();
        }
        catch (TaskCanceledException)
        {
            return NotFound();
        }
    }

    /// <summary>Badge yêu cầu chờ duyệt ở sidebar; lỗi tạm thời không chặn trang.</summary>
    private async Task SetPendingRequestBadgeAsync(string token, CancellationToken ct)
    {
        try
        {
            var requests = await api.GetMyBorrowRequestsAsync(token, ct: ct);
            ViewData["PendingRequestCount"] = requests?.Count(r => r.CoTheHuy) ?? 0;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ViewData["PendingRequestCount"] = 0;
        }
    }

    private static string? NormalizeFilter(string? trangThai) =>
        string.Equals(trangThai, "dang-muon", StringComparison.OrdinalIgnoreCase) ? "dang-muon" : null;
}
