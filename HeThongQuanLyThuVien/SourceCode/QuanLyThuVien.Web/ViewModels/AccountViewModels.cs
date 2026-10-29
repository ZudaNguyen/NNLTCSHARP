using System.ComponentModel.DataAnnotations;
using QuanLyThuVien.Web.Services;

namespace QuanLyThuVien.Web.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 64 ký tự.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; init; } = string.Empty;

    public string? ReturnUrl { get; init; }
}

public sealed class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 64 ký tự.")]
    [RegularExpression("^[A-Za-z0-9._-]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm, gạch dưới hoặc gạch ngang.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Mật khẩu phải từ 12 đến 128 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [DataType(DataType.Password)]
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhanMatKhau { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(200, ErrorMessage = "Họ và tên không quá 200 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; init; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime NgaySinh { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    [RegularExpression("^(Nam|Nữ|Khác)$", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác.")]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; init; } = "Nam";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression("^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 hoặc 11 chữ số.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(254, ErrorMessage = "Email không quá 254 ký tự.")]
    [Display(Name = "Email")]
    public string Email { get; init; } = string.Empty;

    [StringLength(500, ErrorMessage = "Địa chỉ không quá 500 ký tự.")]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgaySinh.Date == default)
            yield return new ValidationResult("Vui lòng nhập ngày sinh.", [nameof(NgaySinh)]);
        else if (NgaySinh.Date > DateTime.Today)
            yield return new ValidationResult("Ngày sinh không được ở tương lai.", [nameof(NgaySinh)]);

        if (!string.IsNullOrEmpty(MatKhau) &&
            (MatKhau.Length < 12 || !MatKhau.Any(char.IsUpper) || !MatKhau.Any(char.IsLower)
             || !MatKhau.Any(char.IsDigit) || MatKhau.All(char.IsLetterOrDigit)))
            yield return new ValidationResult(
                "Mật khẩu cần ít nhất 12 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.",
                [nameof(MatKhau)]);
    }
}

public sealed class LoanListViewModel
{
    public IReadOnlyList<LoanRowViewModel> Loans { get; init; } = [];
    /// <summary>null = tất cả, "dang-muon" = chỉ phiếu chưa trả hết.</summary>
    public string? Filter { get; init; }
    public string? ApiError { get; init; }

    public int ActiveCount => Loans.Count(l => l.IsActive);
    public int OverdueCount => Loans.Count(l => l.IsOverdue);
}

public sealed class LoanRowViewModel
{
    public required int MaPhieu { get; init; }
    public required string SoPhieu { get; init; }
    public required DateTime NgayMuon { get; init; }
    public required DateTime HanTra { get; init; }
    public required int SoLuongConMuon { get; init; }
    public required string TrangThai { get; init; }

    public bool IsActive => SoLuongConMuon > 0;
    public int DaysLeft => (HanTra.Date - DateTime.Today).Days;
    public bool IsOverdue => IsActive && DaysLeft < 0;
    public bool IsDueSoon => IsActive && DaysLeft is >= 0 and <= 3;
}

public sealed class LoanDetailViewModel
{
    public required MuonTraChiTietDto Loan { get; init; }
    public TienPhatDto? Fine { get; init; }
}

public sealed class ProfileViewModel
{
    public required DocGiaDto Profile { get; init; }
    public IReadOnlyList<MuonTraDto> RecentLoans { get; init; } = [];
}
