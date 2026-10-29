using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface IYeuCauKhoaTaiKhoanRepository
{
    /// <summary>Tìm kiếm yêu cầu; <paramref name="maNhanVien"/> null nghĩa là Admin xem tất cả.</summary>
    Task<IReadOnlyList<YeuCauKhoaTaiKhoan>> SearchAsync(
        int? maNhanVien, string? trangThai, string? search, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoan?> GetByIdAsync(int maYeuCau, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaiKhoanDocGiaOptionDto>> FindReaderAccountsAsync(
        string? search, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoan> CreateAsync(
        int maNhanVien, TaoYeuCauKhoaTaiKhoanDto request, Guid idempotencyKey, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoan?> CancelAsync(
        int maNhanVien, int maYeuCau, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoan?> ApproveAsync(
        int maYeuCau, string? nguoiDuyet, CancellationToken cancellationToken);

    Task<YeuCauKhoaTaiKhoan?> RejectAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken);
}
