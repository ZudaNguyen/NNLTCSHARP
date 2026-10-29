using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IYeuCauMuonService
{
    Task<YeuCauMuonDto> CreateAsync(
        int readerId, TaoYeuCauMuonDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<YeuCauMuonDto>> GetForReaderAsync(
        int readerId, string? trangThai, CancellationToken cancellationToken);
    Task<YeuCauMuonDto?> GetByIdForReaderAsync(
        int readerId, int maYeuCau, CancellationToken cancellationToken);
    Task<YeuCauMuonDto?> CancelAsync(
        int readerId, int maYeuCau, CancellationToken cancellationToken);

    Task<IReadOnlyList<YeuCauMuonDto>> SearchAsync(
        string? search, string? trangThai, CancellationToken cancellationToken);
    Task<YeuCauMuonDto?> GetByIdAsync(int maYeuCau, CancellationToken cancellationToken);
    Task<YeuCauMuonDto?> ApproveAsync(
        int maYeuCau, string? nguoiDuyet, CancellationToken cancellationToken);
    Task<YeuCauMuonDto?> RejectAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken);
}
