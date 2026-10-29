using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Web.Services;

/// <summary>
/// Quản lý phiên đăng nhập của độc giả trên Web.
/// Token của API nằm trong claim của cookie auth (HttpOnly), không nằm trong localStorage.
/// </summary>
public sealed class ReaderSession(IHttpContextAccessor accessor)
{
    public const string TokenClaim = "api_token";
    public const string DisplayNameClaim = "display_name";
    public const string MaDocGiaClaim = "ma_doc_gia";

    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? Token => Principal?.FindFirstValue(TokenClaim);

    public string DisplayName =>
        Principal?.FindFirstValue(DisplayNameClaim)
        ?? Principal?.Identity?.Name
        ?? "Độc giả";

    public string TenDangNhap => Principal?.Identity?.Name ?? string.Empty;

    public int? MaDocGia =>
        int.TryParse(Principal?.FindFirstValue(MaDocGiaClaim), out var id) ? id : null;

    /// <summary>Chữ viết tắt hiển thị trong avatar, ví dụ "Nguyễn Văn An" → "NA".</summary>
    public string Initials
    {
        get
        {
            var words = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length switch
            {
                0 => "?",
                1 => words[0][..1].ToUpperInvariant(),
                _ => (words[^2][..1] + words[^1][..1]).ToUpperInvariant()
            };
        }
    }

    public async Task SignInAsync(AuthUserDto user, string token, DateTime expiresAtUtc)
    {
        var httpContext = accessor.HttpContext
            ?? throw new InvalidOperationException("Không có HttpContext để tạo phiên đăng nhập.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name, user.TenDangNhap),
            new(ClaimTypes.Role, user.VaiTro),
            new(DisplayNameClaim, user.TenHienThi),
            new(TokenClaim, token)
        };

        if (user.MaDocGia is int maDocGia)
            claims.Add(new Claim(MaDocGiaClaim, maDocGia.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                ExpiresUtc = expiresAtUtc,
                IsPersistent = false,
                AllowRefresh = true
            });
    }

    public async Task SignOutAsync()
    {
        var httpContext = accessor.HttpContext
            ?? throw new InvalidOperationException("Không có HttpContext để kết thúc phiên đăng nhập.");

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>Chỉ tài khoản vai trò độc giả mới được dùng Web.</summary>
    public static bool IsReaderRole(string vaiTro) =>
        string.Equals(vaiTro, AccountRoles.Reader, StringComparison.Ordinal);
}
