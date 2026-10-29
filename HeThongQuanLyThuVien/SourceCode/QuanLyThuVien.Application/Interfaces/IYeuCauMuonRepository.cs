using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface IYeuCauMuonRepository
{
    Task<IReadOnlyList<YeuCauMuon>> GetForReaderAsync(int readerId, string? trangThai, CancellationToken cancellationToken);
    Task<YeuCauMuon?> GetByIdForReaderAsync(int readerId, int maYeuCau, CancellationToken cancellationToken);
    Task<YeuCauMuon?> GetByIdAsync(int maYeuCau, CancellationToken cancellationToken);
    Task<IReadOnlyList<YeuCauMuon>> SearchAsync(string? search, string? trangThai, CancellationToken cancellationToken);

    Task<YeuCauMuon> CreateAsync(
        int readerId, TaoYeuCauMuonDto request, Guid idempotencyKey, CancellationToken cancellationToken);

    Task<YeuCauMuon?> CancelAsync(int readerId, int maYeuCau, CancellationToken cancellationToken);
    Task<YeuCauMuon> MarkApprovedAsync(
        int maYeuCau, string? nguoiDuyet, int maPhieu, CancellationToken cancellationToken);
    Task<YeuCauMuon?> MarkRejectedAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken);
}
