using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Web.Services;
using QuanLyThuVien.Web.ViewModels;

namespace QuanLyThuVien.Web.Controllers;

public class HomeController(LibraryApiClient api, ReaderSession session) : Controller
{
    private const string ApiUnavailable =
        "Không kết nối được API thư viện. Hãy chắc chắn API đang chạy ở http://localhost:5058.";

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var apiError = default(string);
        var categories = new List<TheLoaiDto>();
        var books = new List<SachDto>();
        var currentLoans = new List<HomeLoanItem>();
        var notices = new List<HomeNotice>();
        var overdueCount = 0;
        var badgeCount = 0;
        var isAuthenticated = session.IsAuthenticated;

        try
        {
            categories = (await api.GetCategoriesAsync(ct) ?? []).ToList();
            books = (await api.GetBooksAsync(ct: ct) ?? []).ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            apiError = ApiUnavailable;
        }

        if (isAuthenticated && session.Token is { Length: > 0 } token)
        {
            try
            {
                var loans = await api.GetMyLoansAsync(token, ct);
                if (loans is null)
                {
                    // Token đã bị thu hồi hoặc hết hạn ở phía API.
                    await session.SignOutAsync();
                    isAuthenticated = false;
                }
                else
                {
                    var active = loans
                        .Where(l => l.SoLuongConMuon > 0)
                        .OrderBy(l => l.HanTra)
                        .ToList();

                    overdueCount = active.Count(l => l.HanTra.Date < DateTime.Today);

                    foreach (var loan in active.Take(3))
                    {
                        var detail = await api.GetMyLoanAsync(loan.MaPhieu, token, ct);
                        var items = detail?.ChiTiet ?? [];
                        var first = items.FirstOrDefault(c => c.SoLuongConMuon > 0) ?? items.FirstOrDefault();
                        currentLoans.Add(new HomeLoanItem
                        {
                            MaPhieu = loan.MaPhieu,
                            SoPhieu = loan.SoPhieu,
                            Title = first?.TenSach ?? loan.SoPhieu,
                            Subtitle = items.Count > 1
                                ? $"{first?.TenSach ?? "Phiếu mượn"} và {items.Count - 1} sách khác"
                                : loan.SoPhieu,
                            HanTra = loan.HanTra,
                            DaysLeft = (loan.HanTra.Date - DateTime.Today).Days
                        });
                    }

                    foreach (var loan in active)
                    {
                        var daysLeft = (loan.HanTra.Date - DateTime.Today).Days;
                        if (daysLeft < 0)
                        {
                            notices.Add(new HomeNotice
                            {
                                Tone = "orange",
                                Icon = "◷",
                                Message = $"Phiếu {loan.SoPhieu} đã quá hạn {Math.Abs(daysLeft)} ngày",
                                Detail = "Vui lòng mang sách đến thư viện để trả."
                            });
                        }
                        else if (daysLeft <= 3)
                        {
                            notices.Add(new HomeNotice
                            {
                                Tone = "sky",
                                Icon = "◷",
                                Message = $"Phiếu {loan.SoPhieu} sắp đến hạn trả",
                                Detail = daysLeft == 0 ? "Hạn trả là hôm nay." : $"Còn {daysLeft} ngày nữa."
                            });
                        }
                    }

                    badgeCount = notices.Count;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Không chặn trang chủ chỉ vì danh sách mượn trả lỗi tạm thời.
            }
        }

        if (notices.Count == 0)
        {
            notices.Add(new HomeNotice
            {
                Tone = "green",
                Icon = "✓",
                Message = isAuthenticated ? "Bạn không có sách nào quá hạn" : "Chào mừng bạn đến với thư viện",
                Detail = isAuthenticated
                    ? "Hãy tiếp tục khám phá kho sách của thư viện."
                    : "Đăng nhập để xem sách đang mượn và lịch sử trả sách."
            });
        }

        if (books.Count > 0)
        {
            notices.Add(new HomeNotice
            {
                Tone = "blue",
                Icon = "★",
                Message = $"Thư viện đang có {books.Count} đầu sách",
                Detail = "Xem danh mục để chọn cuốn tiếp theo."
            });
        }

        ViewData["NotificationCount"] = badgeCount;
        ViewData["Role"] = "reader";

        return View(new HomeViewModel
        {
            FeaturedBooks = books.Take(5).ToList(),
            SuggestedBooks = books.Skip(5).Take(5).ToList(),
            Categories = categories.Take(6).ToList(),
            CurrentLoans = currentLoans,
            Notices = notices.Take(4).ToList(),
            IsAuthenticated = isAuthenticated,
            ReaderName = session.DisplayName,
            OverdueCount = overdueCount,
            ApiError = apiError
        });
    }

    public IActionResult Manager()
    {
        return View();
    }

    public IActionResult Admin()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
