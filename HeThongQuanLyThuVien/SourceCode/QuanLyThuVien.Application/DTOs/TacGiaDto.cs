using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public sealed class TacGiaDto
{
    public int MaTacGia { get; init; }
    public string TenTacGia { get; init; } = string.Empty;
    public string? QuocTich { get; init; }
    public string? GioiThieu { get; init; }
    public bool IsDeleted { get; init; }
}

public sealed class TacGiaWriteDto : IValidatableObject
{
    [Required, StringLength(200)]
    public string TenTacGia { get; init; } = string.Empty;

    [StringLength(100)]
    public string? QuocTich { get; init; }

    [StringLength(2000)]
    public string? GioiThieu { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(TenTacGia))
            yield return new ValidationResult("Tên tác giả không được để trống.", [nameof(TenTacGia)]);
    }
}
