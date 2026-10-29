using System.ComponentModel.DataAnnotations;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Application.DTOs;

/// <summary>Yêu cầu khóa tài khoản độc giả, trả về cho thủ thư và Admin.</summary>
public sealed class YeuCauKhoaTaiKhoanDto
{
    public int MaYeuCau { get; init; }
    public string SoYeuCau => $"YCK-{MaYeuCau:D6}";
    public int MaTaiKhoan { get; init; }
    public string TenDangNhap { get; init; } = string.Empty;
    public string TenHienThi { get; init; } = string.Empty;
    public int? MaDocGia { get; init; }
    public string? TenDocGia { get; init; }
    public int MaNhanVien { get; init; }
    public string TenNhanVien { get; init; } = string.Empty;
    public string LyDo { get; init; } = string.Empty;
    public int? ThoiHanKhoaNgay { get; init; }
    public string TrangThai { get; init; } = string.Empty;
    public string? NguoiDuyet { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime? NgayDuyet { get; init; }
    public string? LyDoTuChoi { get; init; }
    public bool CoTheHuy => TrangThai == YeuCauKhoaTaiKhoanTrangThai.ChoDuyet;
}

/// <summary>Tài khoản độc giả đang hoạt động, dùng cho ô chọn của thủ thư.</summary>
public sealed class TaiKhoanDocGiaOptionDto
{
    public int MaTaiKhoan { get; init; }
    public string TenDangNhap { get; init; } = string.Empty;
    public string TenHienThi { get; init; } = string.Empty;
    public int? MaDocGia { get; init; }
    public string? TenDocGia { get; init; }
    public string TrangThaiTaiKhoan { get; init; } = string.Empty;
    public string? TrangThaiDocGia { get; init; }
}

public sealed class TaoYeuCauKhoaTaiKhoanDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã tài khoản không hợp lệ.")]
    public int MaTaiKhoan { get; init; }

    [Required(ErrorMessage = "Vui lòng nhập lý do khóa tài khoản.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do phải từ 10 đến 500 ký tự.")]
    public string LyDo { get; init; } = string.Empty;

    [Range(1, 365, ErrorMessage = "Thời hạn khóa phải từ 1 đến 365 ngày.")]
    public int? ThoiHanKhoaNgay { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(LyDo))
            yield return new ValidationResult("Vui lòng nhập lý do khóa tài khoản.", [nameof(LyDo)]);
        else if (LyDo.Trim().Length < 10)
            yield return new ValidationResult("Lý do phải có ít nhất 10 ký tự.", [nameof(LyDo)]);
    }
}

public sealed class TuChoiYeuCauKhoaTaiKhoanDto
{
    [StringLength(500, ErrorMessage = "Lý do từ chối tối đa 500 ký tự.")]
    public string? LyDo { get; init; }
}
