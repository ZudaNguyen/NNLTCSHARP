using System.Drawing;

namespace QuanLyThuVien.Desktop.Theme;

/// <summary>
/// Bảng màu, font và thang kích thước dùng chung cho toàn bộ giao diện Desktop.
/// Mọi màn hình chỉ lấy màu/font từ đây, không tự khai báo màu hay font riêng.
/// </summary>
internal static class UiTheme
{
    // ----- Màu nền -----
    public static readonly Color PageBg = Color.FromArgb(244, 246, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceMuted = Color.FromArgb(248, 250, 252);
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color BorderStrong = Color.FromArgb(203, 213, 225);
    public static readonly Color InputDisabled = Color.FromArgb(241, 245, 249);

    // ----- Màu chữ -----
    public static readonly Color Text = Color.FromArgb(15, 23, 42);
    public static readonly Color TextSecondary = Color.FromArgb(51, 65, 85);
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
    public static readonly Color TextOnDark = Color.FromArgb(241, 245, 249);

    // ----- Màu nhấn và trạng thái -----
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);
    public static readonly Color PrimaryPressed = Color.FromArgb(30, 64, 175);
    public static readonly Color PrimarySoft = Color.FromArgb(219, 234, 254);
    public static readonly Color Success = Color.FromArgb(22, 128, 61);
    public static readonly Color SuccessHover = Color.FromArgb(21, 101, 52);
    public static readonly Color Warning = Color.FromArgb(180, 83, 9);
    public static readonly Color Danger = Color.FromArgb(200, 36, 36);
    public static readonly Color DangerHover = Color.FromArgb(153, 27, 27);
    public static readonly Color DangerSoft = Color.FromArgb(254, 226, 226);

    // ----- Thanh bên -----
    public static readonly Color Sidebar = Color.FromArgb(15, 23, 42);
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);
    public static readonly Color SidebarActive = Color.FromArgb(30, 58, 138);
    public static readonly Color SidebarText = Color.FromArgb(203, 213, 225);
    public static readonly Color SidebarCaption = Color.FromArgb(100, 116, 139);

    // ----- Bảng dữ liệu -----
    public static readonly Color TableHeaderBg = Color.FromArgb(241, 245, 249);
    public static readonly Color TableHeaderText = Color.FromArgb(51, 65, 85);
    public static readonly Color TableAltRow = Color.FromArgb(248, 250, 252);
    public static readonly Color TableGrid = Color.FromArgb(232, 237, 244);

    // ----- Font: một họ duy nhất, thang kích thước cố định -----
    public const string FontFamily = "Segoe UI";

    private static readonly Dictionary<(float, FontStyle), Font> Cache = new();

    /// <summary>
    /// Trả về font dùng chung. Kích thước được gộp về thang 9 / 9.5 / 10 / 12 / 15 / 18
    /// để các màn hình không lệch nhau vài phần điểm.
    /// </summary>
    public static Font Font(float size, FontStyle style = FontStyle.Regular)
    {
        var snapped = Snap(size);
        lock (Cache)
        {
            if (!Cache.TryGetValue((snapped, style), out var font))
            {
                font = new Font(FontFamily, snapped, style, GraphicsUnit.Point);
                Cache[(snapped, style)] = font;
            }
            return font;
        }
    }

    public static Font Small => Font(9);
    public static Font Body => Font(10);
    public static Font BodyBold => Font(10, FontStyle.Bold);
    public static Font Heading => Font(12, FontStyle.Bold);
    public static Font Title => Font(15, FontStyle.Bold);
    public static Font Display => Font(18, FontStyle.Bold);

    private static float Snap(float size) => size switch
    {
        <= 9.25f => 9f,
        <= 9.75f => 9.5f,
        <= 10.75f => 10f,
        <= 13f => 12f,
        <= 14.5f => 15f,
        _ => 18f
    };
}
