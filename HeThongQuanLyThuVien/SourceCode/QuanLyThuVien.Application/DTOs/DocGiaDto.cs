using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public sealed class DocGiaDto
{
    public int MaDocGia { get; init; }
    public string HoTen { get; init; } = string.Empty;
    public DateTime NgaySinh { get; init; }
    public string GioiTinh { get; init; } = string.Empty;
    public string SoDienThoai { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? DiaChi { get; init; }
    public DateTime NgayDangKy { get; init; }
    public string TrangThai { get; init; } = string.Empty;
}

public sealed class CreateDocGiaDto : IValidatableObject
{
    [Required, StringLength(200)]
    public string HoTen { get; init; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime NgaySinh { get; init; }

    [Required, RegularExpression("^(Nam|Nữ|Khác)$", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác.")]
    public string GioiTinh { get; init; } = string.Empty;

    [Required, RegularExpression("^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 hoặc 11 chữ số.")]
    public string SoDienThoai { get; init; } = string.Empty;

    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [StringLength(500)]
    public string? DiaChi { get; init; }

    [Required, RegularExpression("^(Hoạt động|Khóa thẻ)$", ErrorMessage = "Trạng thái phải là Hoạt động hoặc Khóa thẻ.")]
    public string TrangThai { get; init; } = "Hoạt động";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgaySinh.Date > DateTime.Today)
            yield return new ValidationResult("Ngày sinh không được ở tương lai.", [nameof(NgaySinh)]);
        if (string.IsNullOrWhiteSpace(HoTen))
            yield return new ValidationResult("Họ và tên không được để trống.", [nameof(HoTen)]);
    }
}

public sealed class UpdateDocGiaDto : IValidatableObject
{
    [Required, StringLength(200)]
    public string HoTen { get; init; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime NgaySinh { get; init; }

    [Required, RegularExpression("^(Nam|Nữ|Khác)$", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác.")]
    public string GioiTinh { get; init; } = string.Empty;

    [Required, RegularExpression("^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 hoặc 11 chữ số.")]
    public string SoDienThoai { get; init; } = string.Empty;

    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [StringLength(500)]
    public string? DiaChi { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgaySinh.Date > DateTime.Today)
            yield return new ValidationResult("Ngày sinh không được ở tương lai.", [nameof(NgaySinh)]);
        if (string.IsNullOrWhiteSpace(HoTen))
            yield return new ValidationResult("Họ và tên không được để trống.", [nameof(HoTen)]);
    }
}

public sealed class UpdateDocGiaTrangThaiDto
{
    [Required, RegularExpression("^(Hoạt động|Khóa thẻ)$", ErrorMessage = "Trạng thái phải là Hoạt động hoặc Khóa thẻ.")]
    public string TrangThai { get; init; } = string.Empty;
}
