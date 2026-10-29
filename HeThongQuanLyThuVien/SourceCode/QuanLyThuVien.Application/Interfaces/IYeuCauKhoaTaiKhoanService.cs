using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IYeuCauKhoaTaiKhoanService
{
    /// <summary>Thủ thư xem yêu cầu của mình; Admin truyền <paramref name="maNhanVien"/> null để xem tất cả.</summary>
    Task<IReadOnlyList<YeuCauKhoaTaiKhoanDto>> SearchAsync(
        int? maNhanVien, string? trangThai, string? search, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoanDto?> GetByIdAsync(
        int maYeuCau, int? maNhanVien, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaiKhoanDocGiaOptionDto>> FindReaderAccountsAsync(
        string? search, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoanDto> CreateAsync(
        int maNhanVien, TaoYeuCauKhoaTaiKhoanDto request, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoanDto?> CancelAsync(
        int maNhanVien, int maYeuCau, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoanDto?> ApproveAsync(
        int maYeuCau, string? nguoiDuyet, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoanDto?> RejectAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken);
}
