using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public static class AccountRoles
{
    public const string Admin = "ADMIN";
    public const string Librarian = "THU_THU";
    public const string Reader = "DOC_GIA";
}

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
