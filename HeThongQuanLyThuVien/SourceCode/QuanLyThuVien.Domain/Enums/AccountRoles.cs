namespace QuanLyThuVien.Domain.Enums;

/// <summary>
/// Giá trị vai trò lưu trong cột <c>TaiKhoan.VaiTro</c>.
/// Dùng các hằng số này thay vì gõ chuỗi "ADMIN", "THU_THU", "DOC_GIA" trong code,
/// để mọi tầng (Domain, Application, Api, Desktop, Web) luôn khớp nhau và khớp
/// check constraint <c>CK_TaiKhoan_VaiTro</c> trong database.
/// </summary>
public static class AccountRoles
{
    /// <summary>Quản trị viên: toàn quyền, kể cả quản lý tài khoản và nhân viên.</summary>
    public const string Admin = "ADMIN";

    /// <summary>Thủ thư / quản lý nghiệp vụ: sách, độc giả, mượn trả, báo cáo.</summary>
    public const string Librarian = "THU_THU";

    /// <summary>Độc giả: chỉ tra cứu sách và xem hồ sơ, lịch sử mượn của chính mình.</summary>
    public const string Reader = "DOC_GIA";

    /// <summary>Tập vai trò của nhân viên thư viện, dùng cho <c>[Authorize(Roles = ...)]</c>.</summary>
    public const string Staff = Admin + "," + Librarian;

    /// <summary>Tập tất cả vai trò hợp lệ.</summary>
    public const string All = Admin + "," + Librarian + "," + Reader;
}
