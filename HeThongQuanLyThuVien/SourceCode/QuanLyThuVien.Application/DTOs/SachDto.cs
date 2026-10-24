using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public sealed class SachDto
{
    public int MaSach { get; init; }
    public string ISBN { get; init; } = string.Empty;
    public string TenSach { get; init; } = string.Empty;
    public int MaTheLoai { get; init; }
    public string TenTheLoai { get; init; } = string.Empty;
    public int NamXB { get; init; }
    public string? NhaXB { get; init; }
    public int SoLuong { get; init; }
    public int SoLuongCon { get; init; }
    public string? MoTa { get; init; }
    public string? HinhAnh { get; init; }
    public bool IsDeleted { get; init; }
    public IReadOnlyList<int> MaTacGia { get; init; } = [];
    public IReadOnlyList<string> TenTacGia { get; init; } = [];
}

public sealed class SachWriteDto : IValidatableObject
{
    [Required, StringLength(20, MinimumLength = 10)]
    public string ISBN { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string TenSach { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MaTheLoai { get; init; }

    [Range(1450, 2200)]
    public int NamXB { get; init; }

    [StringLength(200)]
    public string? NhaXB { get; init; }

    [Range(0, 10000)]
    public int SoLuong { get; init; }

    [StringLength(4000)]
    public string? MoTa { get; init; }

    [StringLength(1000)]
    public string? HinhAnh { get; init; }

    [Required, MinLength(1)]
    public List<int> MaTacGia { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsValidIsbn(ISBN))
        {
            yield return new ValidationResult(
                "ISBN phải là ISBN-10 hoặc ISBN-13 hợp lệ.",
                [nameof(ISBN)]);
        }

        if (MaTacGia.Count != MaTacGia.Distinct().Count() || MaTacGia.Any(id => id <= 0))
        {
            yield return new ValidationResult(
                "Danh sách tác giả không được trùng lặp và mã tác giả phải lớn hơn 0.",
                [nameof(MaTacGia)]);
        }

        if (string.IsNullOrWhiteSpace(TenSach))
        {
            yield return new ValidationResult("Tên sách không được để trống.", [nameof(TenSach)]);
        }
    }

    public static string NormalizeIsbn(string? isbn) =>
        new((isbn ?? string.Empty)
            .Where(character => !char.IsWhiteSpace(character) && character != '-')
            .Select(char.ToUpperInvariant)
            .ToArray());

    private static bool IsValidIsbn(string? value)
    {
        var isbn = NormalizeIsbn(value);
        if (isbn.Length == 10)
        {
            var sum = 0;
            for (var i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn[i])) return false;
                sum += (isbn[i] - '0') * (10 - i);
            }

            var check = isbn[9] == 'X' ? 10 : char.IsDigit(isbn[9]) ? isbn[9] - '0' : -1;
            return check >= 0 && (sum + check) % 11 == 0;
        }

        if (isbn.Length != 13 || isbn.Any(c => !char.IsDigit(c))) return false;

        var weightedSum = 0;
        for (var i = 0; i < 12; i++)
        {
            weightedSum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);
        }

        var expectedCheckDigit = (10 - weightedSum % 10) % 10;
        return isbn[12] - '0' == expectedCheckDigit;
    }
}

public sealed class SachLookupsDto
{
    public IReadOnlyList<TheLoaiOptionDto> TheLoai { get; init; } = [];
    public IReadOnlyList<TacGiaOptionDto> TacGia { get; init; } = [];
}

public sealed class TheLoaiOptionDto
{
    public int MaTheLoai { get; init; }
    public string TenTheLoai { get; init; } = string.Empty;
}

public sealed class TacGiaOptionDto
{
    public int MaTacGia { get; init; }
    public string TenTacGia { get; init; } = string.Empty;
}
