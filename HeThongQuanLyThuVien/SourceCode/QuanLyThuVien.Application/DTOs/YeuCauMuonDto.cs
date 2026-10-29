using System.ComponentModel.DataAnnotations;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Application.DTOs;

public sealed class YeuCauMuonDto
{
    public int MaYeuCau { get; init; }
    public string SoYeuCau => $"YC-{MaYeuCau:D6}";
    public int MaDocGia { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public string ISBN { get; init; } = string.Empty;
    public int SoLuong { get; init; }
    public DateTime NgayTao { get; init; }
    public string TrangThai { get; init; } = string.Empty;
    public string? GhiChu { get; init; }
    public string? LyDoTuChoi { get; init; }
    public string? NguoiDuyet { get; init; }
    public DateTime? NgayDuyet { get; init; }
    public int? MaPhieu { get; init; }
    public string? SoPhieu => MaPhieu is null ? null : $"PM-{MaPhieu.Value:D6}";
    public bool CoTheHuy => TrangThai == YeuCauMuonTrangThai.ChoDuyet;
}

public sealed class TaoYeuCauMuonDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã sách không hợp lệ.")]
    public int MaSach { get; init; }

    [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000.")]
    public int SoLuong { get; init; } = 1;

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string? GhiChu { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaSach <= 0)
            yield return new ValidationResult("Mã sách không hợp lệ.", [nameof(MaSach)]);
        if (SoLuong <= 0)
            yield return new ValidationResult("Số lượng phải lớn hơn 0.", [nameof(SoLuong)]);
    }
}

public sealed class TuChoiYeuCauMuonDto
{
    [StringLength(500, ErrorMessage = "Lý do từ chối tối đa 500 ký tự.")]
    public string? LyDo { get; init; }
}
