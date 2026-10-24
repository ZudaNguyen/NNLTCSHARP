using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public sealed class TheLoaiDto
{
    public int MaTheLoai { get; init; }
    public string TenTheLoai { get; init; } = string.Empty;
    public string? MoTa { get; init; }
    public bool IsDeleted { get; init; }
}

public sealed class TheLoaiWriteDto : IValidatableObject
{
    [Required, StringLength(150)]
    public string TenTheLoai { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? MoTa { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(TenTheLoai))
            yield return new ValidationResult("Tên thể loại không được để trống.", [nameof(TenTheLoai)]);
    }
}
