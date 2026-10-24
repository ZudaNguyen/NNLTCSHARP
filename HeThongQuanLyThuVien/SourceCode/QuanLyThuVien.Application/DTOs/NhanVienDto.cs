using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public sealed class NhanVienDto
{
    public int MaNhanVien { get; init; }
    public string HoTen { get; init; } = string.Empty;
    public string? SoDienThoai { get; init; }
    public string? Email { get; init; }
    public string ChucVu { get; init; } = string.Empty;
    public string TrangThai { get; init; } = string.Empty;
    public DateTime NgayTao { get; init; }
}

public sealed class NhanVienTrangThaiLichSuDto
{
    public int Id { get; init; }
    public int MaNhanVien { get; init; }
    public string TrangThaiCu { get; init; } = string.Empty;
    public string TrangThaiMoi { get; init; } = string.Empty;
    public DateTime ThayDoiLuc { get; init; }
}

public class NhanVienWriteDto : IValidatableObject
{
    [Required, StringLength(200)]
    public string HoTen { get; init; } = string.Empty;

    [RegularExpression("^$|^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 hoặc 11 chữ số.")]
    public string? SoDienThoai { get; init; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [Required, StringLength(100)]
    public string ChucVu { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(HoTen))
            yield return new ValidationResult("Họ tên nhân viên không được để trống.", [nameof(HoTen)]);
        if (string.IsNullOrWhiteSpace(ChucVu))
            yield return new ValidationResult("Chức vụ không được để trống.", [nameof(ChucVu)]);
        if (Email is not null && string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult("Email phải để trống hoặc có định dạng hợp lệ.", [nameof(Email)]);
    }
}

public sealed class CreateNhanVienDto : NhanVienWriteDto { }
public sealed class UpdateNhanVienDto : NhanVienWriteDto { }

public sealed class UpdateTrangThaiNhanVienDto
{
    [Required, RegularExpression("^(Hoạt động|Ngừng hoạt động)$", ErrorMessage = "Trạng thái phải là Hoạt động hoặc Ngừng hoạt động.")]
    public string TrangThai { get; init; } = string.Empty;
}
