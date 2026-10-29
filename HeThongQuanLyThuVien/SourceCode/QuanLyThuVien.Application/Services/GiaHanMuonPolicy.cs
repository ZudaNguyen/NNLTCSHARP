using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

/// <summary>
/// Chính sách gia hạn phiếu mượn: tối đa 2 lần, mỗi lần cộng thêm 7 ngày.
/// Chỉ gia hạn khi phiếu còn sách chưa trả, chưa quá hạn và không còn tiền phạt chưa thu.
/// </summary>
/// <remarks>
/// Chính sách được kiểm tra lại trong giao dịch của <c>MuonTraRepository.GiaHanAsync</c>
/// để tránh trường hợp dữ liệu thay đổi giữa lúc hiển thị và lúc bấm gia hạn.
/// </remarks>
public static class GiaHanMuonPolicy
{
    public const int SoLanToiDa = 2;
    public const int SoNgayMoiLan = 7;

    /// <summary>
    /// Trả về lý do không thể gia hạn, hoặc <c>null</c> nếu phiếu đủ điều kiện.
    /// </summary>
    /// <param name="soLuongConMuon">Số lượng sách còn chưa trả trên phiếu.</param>
    /// <param name="hanTra">Hạn trả hiện tại.</param>
    /// <param name="soLanGiaHan">Số lần đã gia hạn.</param>
    /// <param name="conNo">Tiền phạt còn nợ; <c>null</c> khi có khoản phạt chưa xác định được mức thu.</param>
    /// <param name="homNay">Ngày hiện tại dùng để kiểm tra quá hạn.</param>
    public static string? KiemTra(
        int soLuongConMuon, DateTime hanTra, int soLanGiaHan, decimal? conNo, DateTime homNay)
    {
        if (soLuongConMuon <= 0)
            return "Phiếu mượn đã hoàn tất, không thể gia hạn.";
        if (hanTra.Date < homNay.Date)
            return "Phiếu mượn đã quá hạn, không thể gia hạn. Vui lòng xử lý trả sách trước.";
        if (soLanGiaHan >= SoLanToiDa)
            return $"Phiếu mượn đã gia hạn đủ {SoLanToiDa} lần, không thể gia hạn thêm.";
        if (conNo is null)
            return "Có khoản phạt chưa xác định mức thu, không thể gia hạn. Vui lòng kiểm tra tiền phạt trước.";
        if (conNo > 0)
            return $"Độc giả còn {conNo.Value:N0} đ tiền phạt chưa thanh toán, không thể gia hạn.";
        return null;
    }

    /// <summary>
    /// Tính tiền phạt còn nợ trên một phiếu mượn.
    /// Trả về <c>null</c> nếu có lần trả trễ chưa được xác định mức phạt.
    /// </summary>
    public static decimal? TinhConNo(IEnumerable<ChiTietTraSach> lichSuTra)
    {
        var rows = lichSuTra as IReadOnlyCollection<ChiTietTraSach> ?? lichSuTra.ToList();
        if (rows.Any(x => x.SoTienPhatPhaiThu is null && x.SoNgayTre > 0)) return null;
        return rows.Sum(x => Math.Max(0, (x.SoTienPhatPhaiThu ?? 0) - x.CacKhoanThu.Sum(p => p.SoTien)));
    }
}
