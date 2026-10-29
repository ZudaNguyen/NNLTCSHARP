using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Application.DTOs;

public class MuonTraDto
{
    public int MaPhieu { get; init; }
    public string SoPhieu => $"PM-{MaPhieu:D6}";
    public int MaDocGia { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public string? NguoiLap { get; init; }
    public DateTime NgayMuon { get; init; }
    public DateTime HanTra { get; init; }
    public int SoLuongConMuon { get; init; }
    public string TrangThai { get; init; } = string.Empty;
}

public sealed class MuonTraChiTietDto : MuonTraDto
{
    public IReadOnlyList<ChiTietMuonTraDto> ChiTiet { get; init; } = [];
}

public sealed class ChiTietMuonTraDto
{
    public int MaChiTiet { get; init; }
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public int SoLuongMuon { get; init; }
    public int SoLuongDaXuLy { get; init; }
    public int SoLuongConMuon => SoLuongMuon - SoLuongDaXuLy;
    public IReadOnlyList<LanTraSachDto> LichSuTra { get; init; } = [];
}

public sealed class LanTraSachDto
{
    public int MaLanTra { get; init; }
    public int SoLuongXuLy { get; init; }
    public DateTime NgayTra { get; init; }
    public string TinhTrangSach { get; init; } = string.Empty;
    public int SoNgayTre { get; init; }
    public decimal? MucPhatMoiNgayApDung { get; init; }
    public decimal? SoTienPhatPhaiThu { get; init; }
    public decimal DaThu { get; init; }
    public IReadOnlyList<KhoanThuTienPhatDto> LichSuThu { get; init; } = [];
    public decimal? ConNo => SoTienPhatPhaiThu is null ? null : Math.Max(0, SoTienPhatPhaiThu.Value - DaThu);
    public string TrangThaiThanhToan => SoTienPhatPhaiThu is null
        ? "Chưa xác định"
        : ConNo is <= 0 ? "Đã thu đủ" : DaThu > 0 ? "Đã thu một phần" : "Chưa thu";
}

public sealed class TaoMuonTraDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int MaDocGia { get; init; }

    public DateTime NgayMuon { get; init; }
    public DateTime HanTra { get; init; }

    [StringLength(100)]
    public string? NguoiLap { get; init; }

    public Guid IdempotencyKey { get; init; }

    [Required, MinLength(1)]
    public List<TaoChiTietMuonDto> ChiTiet { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IdempotencyKey == Guid.Empty)
            yield return new ValidationResult("IdempotencyKey không được rỗng.", [nameof(IdempotencyKey)]);
        if (HanTra.Date < NgayMuon.Date)
            yield return new ValidationResult("Hạn trả không được trước ngày mượn.", [nameof(HanTra)]);
        if (ChiTiet.Count == 0)
            yield return new ValidationResult("Phiếu mượn phải có ít nhất một đầu sách.", [nameof(ChiTiet)]);
        if (ChiTiet.Any(x => x.MaSach <= 0 || x.SoLuong <= 0))
            yield return new ValidationResult("Mã sách và số lượng mượn phải lớn hơn 0.", [nameof(ChiTiet)]);
        if (ChiTiet.Select(x => x.MaSach).Distinct().Count() != ChiTiet.Count)
            yield return new ValidationResult("Mỗi đầu sách chỉ được xuất hiện một lần trong phiếu.", [nameof(ChiTiet)]);
        if (string.IsNullOrWhiteSpace(NguoiLap))
            yield return new ValidationResult("Chưa xác định nhân viên lập phiếu.", [nameof(NguoiLap)]);
    }
}

public sealed class TaoChiTietMuonDto
{
    [Range(1, int.MaxValue)]
    public int MaSach { get; init; }

    [Range(1, 1000)]
    public int SoLuong { get; init; }
}

public sealed class TraChiTietDto : IValidatableObject
{
    [Range(1, 1000)]
    public int SoLuongXuLy { get; init; }

    public DateTime NgayTra { get; init; }

    [Required, RegularExpression("^(Nguyên vẹn|Rách hoặc bẩn nhẹ|Hỏng nặng|Mất)$")]
    public string TinhTrangSach { get; init; } = string.Empty;

    // Optional collection made in the same transaction as the return record.
    public decimal SoTienThu { get; init; }

    [StringLength(100)]
    public string? NguoiThu { get; init; }

    public Guid IdempotencyKey { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IdempotencyKey == Guid.Empty)
            yield return new ValidationResult("IdempotencyKey không được rỗng.", [nameof(IdempotencyKey)]);
        if (SoTienThu < 0 || SoTienThu > 9999999999999999.99m)
            yield return new ValidationResult("Số tiền thu phải không âm và nằm trong giới hạn.", [nameof(SoTienThu)]);
    }
}

public sealed class SachMuonOptionDto
{
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public string ISBN { get; init; } = string.Empty;
    public int SoLuongCon { get; init; }
}

public sealed class QuaHanDto
{
    public int MaPhieu { get; init; }
    public string SoPhieu => $"PM-{MaPhieu:D6}";
    public int MaChiTiet { get; init; }
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public int MaDocGia { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public DateTime NgayMuon { get; init; }
    public DateTime HanTra { get; init; }
    public int SoLuongConMuon { get; init; }
    public int SoNgayTre { get; init; }
}

public sealed class TienPhatDto
{
    public int MaPhieu { get; init; }
    public IReadOnlyList<LanTraSachDto> CacKhoanPhat { get; init; } = [];
    public decimal? TongPhaiThu { get; init; }
    public decimal DaThu { get; init; }
    public decimal? ConNo => TongPhaiThu is null ? null : Math.Max(0, TongPhaiThu.Value - DaThu);
    public bool ChuaCauHinhMucPhat { get; init; }
}

public sealed class CapNhatTienPhatDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int MaLanTra { get; init; }

    public decimal SoTienPhatPhaiThu { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SoTienPhatPhaiThu < 0 || SoTienPhatPhaiThu > 9999999999999999.99m)
            yield return new ValidationResult("Tiền phạt phải nằm trong khoảng cho phép.", [nameof(SoTienPhatPhaiThu)]);
    }
}

public sealed class ThuTienPhatDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int MaLanTra { get; init; }

    public decimal SoTien { get; init; }

    [StringLength(100)]
    public string? NguoiThu { get; init; }

    public Guid IdempotencyKey { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SoTien <= 0 || SoTien > 9999999999999999.99m)
            yield return new ValidationResult("Số tiền thu phải lớn hơn 0 và nằm trong giới hạn.", [nameof(SoTien)]);
        if (IdempotencyKey == Guid.Empty)
            yield return new ValidationResult("IdempotencyKey không được rỗng.", [nameof(IdempotencyKey)]);
    }
}

public sealed class KhoanThuTienPhatDto
{
    public int MaKhoanThu { get; init; }
    public int MaLanTra { get; init; }
    public decimal SoTien { get; init; }
    public DateTime NgayThu { get; init; }
    public string? NguoiThu { get; init; }
}

public sealed class CauHinhTienPhatDto
{
    public decimal? MucPhatMoiNgay { get; init; }
    public DateTime? CapNhatLuc { get; init; }
}

public sealed class CapNhatCauHinhTienPhatDto : IValidatableObject
{
    public decimal MucPhatMoiNgay { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MucPhatMoiNgay <= 0 || MucPhatMoiNgay > 9999999999999999.99m)
            yield return new ValidationResult("Mức phạt mỗi ngày phải lớn hơn 0 và nằm trong giới hạn.", [nameof(MucPhatMoiNgay)]);
    }
}
