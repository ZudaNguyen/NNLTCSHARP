using System.ComponentModel.DataAnnotations;
using QuanLyThuVien.Web.Services;

namespace QuanLyThuVien.Web.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 64 ký tự.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; init; } = string.Empty;

    public string? ReturnUrl { get; init; }
}

public sealed class LoanListViewModel
{
    public IReadOnlyList<LoanRowViewModel> Loans { get; init; } = [];
    /// <summary>null = tất cả, "dang-muon" = chỉ phiếu chưa trả hết.</summary>
    public string? Filter { get; init; }
    public string? ApiError { get; init; }

    public int ActiveCount => Loans.Count(l => l.IsActive);
    public int OverdueCount => Loans.Count(l => l.IsOverdue);
}

public sealed class LoanRowViewModel
{
    public required int MaPhieu { get; init; }
    public required string SoPhieu { get; init; }
    public required DateTime NgayMuon { get; init; }
    public required DateTime HanTra { get; init; }
    public required int SoLuongConMuon { get; init; }
    public required string TrangThai { get; init; }

    public bool IsActive => SoLuongConMuon > 0;
    public int DaysLeft => (HanTra.Date - DateTime.Today).Days;
    public bool IsOverdue => IsActive && DaysLeft < 0;
    public bool IsDueSoon => IsActive && DaysLeft is >= 0 and <= 3;
}

public sealed class LoanDetailViewModel
{
    public required MuonTraChiTietDto Loan { get; init; }
    public TienPhatDto? Fine { get; init; }
}

public sealed class ProfileViewModel
{
    public required DocGiaDto Profile { get; init; }
    public IReadOnlyList<MuonTraDto> RecentLoans { get; init; } = [];
}
