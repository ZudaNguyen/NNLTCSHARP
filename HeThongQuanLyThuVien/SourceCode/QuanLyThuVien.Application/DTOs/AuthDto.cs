using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public sealed class LoginRequestDto
{
    [Required, StringLength(64, MinimumLength = 3)]
    public string TenDangNhap { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 1)]
    public string MatKhau { get; init; } = string.Empty;
}

public sealed class AuthUserDto
{
    public int MaTaiKhoan { get; init; }
    public string TenDangNhap { get; init; } = string.Empty;
    public string TenHienThi { get; init; } = string.Empty;
    public string VaiTro { get; init; } = string.Empty;
    public int? MaNhanVien { get; init; }
    public int? MaDocGia { get; init; }
}

public sealed class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAtUtc { get; init; }
    public AuthUserDto User { get; init; } = new();
}

public sealed class RegisterReaderRequestDto : IValidatableObject
{
    [Required, StringLength(64, MinimumLength = 3), RegularExpression("^[A-Za-z0-9._-]+$")]
    public string TenDangNhap { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string MatKhau { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string HoTen { get; init; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime NgaySinh { get; init; }

    [Required, RegularExpression("^(Nam|Nữ|Khác)$", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác.")]
    public string GioiTinh { get; init; } = string.Empty;

    [Required, RegularExpression("^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 hoặc 11 chữ số.")]
    public string SoDienThoai { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [StringLength(500)]
    public string? DiaChi { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!PasswordPolicy.IsStrong(MatKhau))
            yield return new ValidationResult(PasswordPolicy.Message, [nameof(MatKhau)]);
        if (NgaySinh.Date == default)
            yield return new ValidationResult("Vui lòng nhập ngày sinh.", [nameof(NgaySinh)]);
        else if (NgaySinh.Date > DateTime.Today)
            yield return new ValidationResult("Ngày sinh không được ở tương lai.", [nameof(NgaySinh)]);
        if (string.IsNullOrWhiteSpace(HoTen))
            yield return new ValidationResult("Họ và tên không được để trống.", [nameof(HoTen)]);
    }
}

public sealed class ChangePasswordRequestDto
{
    [Required, StringLength(128, MinimumLength = 1)]
    public string MatKhauHienTai { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string MatKhauMoi { get; init; } = string.Empty;
}

public sealed class AccountDto
{
    public int MaTaiKhoan { get; init; }
    public string TenDangNhap { get; init; } = string.Empty;
    public string TenHienThi { get; init; } = string.Empty;
    public string VaiTro { get; init; } = string.Empty;
    public string TrangThai { get; init; } = string.Empty;
    public int? MaNhanVien { get; init; }
    public int? MaDocGia { get; init; }
    public int SoLanDangNhapThatBai { get; init; }
    public DateTime? KhoaDenUtc { get; init; }
    public DateTime NgayTaoUtc { get; init; }
    public DateTime? CapNhatLucUtc { get; init; }
}

public sealed class CreateAccountRequestDto : IValidatableObject
{
    [Required, StringLength(64, MinimumLength = 3), RegularExpression("^[A-Za-z0-9._-]+$")]
    public string TenDangNhap { get; init; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string TenHienThi { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string MatKhau { get; init; } = string.Empty;

    [Required]
    public string VaiTro { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? MaNhanVien { get; init; }

    [Range(1, int.MaxValue)]
    public int? MaDocGia { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!PasswordPolicy.IsStrong(MatKhau))
            yield return new ValidationResult(PasswordPolicy.Message, [nameof(MatKhau)]);
    }
}

public sealed class UpdateAccountRequestDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string TenHienThi { get; init; } = string.Empty;

    [Required]
    public string VaiTro { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? MaNhanVien { get; init; }

    [Range(1, int.MaxValue)]
    public int? MaDocGia { get; init; }
}

public sealed class UpdateAccountStatusRequestDto
{
    [Required, RegularExpression("^(Hoạt động|Bị khóa)$")]
    public string TrangThai { get; init; } = string.Empty;
}

public sealed class ResetAccountPasswordRequestDto : IValidatableObject
{
    [Required, StringLength(128, MinimumLength = 12)]
    public string MatKhauMoi { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!PasswordPolicy.IsStrong(MatKhauMoi))
            yield return new ValidationResult(PasswordPolicy.Message, [nameof(MatKhauMoi)]);
    }
}

public sealed class AccountBootstrapOptions
{
    public string? TenDangNhap { get; init; }
    public string? TenHienThi { get; init; }
    public string? MatKhau { get; init; }
}

public static class PasswordPolicy
{
    public const string Message = "Mật khẩu cần ít nhất 12 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.";

    public static bool IsStrong(string? password) =>
        password is { Length: >= 12 and <= 128 }
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit)
        && password.Any(character => !char.IsLetterOrDigit(character));
}
