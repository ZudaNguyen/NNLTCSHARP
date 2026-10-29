using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Application.Services;

/// <summary>
/// Nghiệp vụ yêu cầu mượn: độc giả gửi / hủy, thủ thư duyệt / từ chối.
/// Khi duyệt, phiếu mượn được tạo qua <see cref="IMuonTraService"/> để dùng chung
/// toàn bộ quy tắc tồn kho, độc giả bị khóa và chống trùng.
/// </summary>
public sealed class YeuCauMuonService(
    IYeuCauMuonRepository repository,
    IMuonTraService muonTraService) : IYeuCauMuonService
{
    /// <summary>Số ngày mượn mặc định khi duyệt yêu cầu.</summary>
    public const int SoNgayMuon = 14;

    public async Task<YeuCauMuonDto> CreateAsync(
        int readerId, TaoYeuCauMuonDto request, CancellationToken cancellationToken) =>
        Map(await repository.CreateAsync(readerId, request, Guid.NewGuid(), cancellationToken));

    public async Task<IReadOnlyList<YeuCauMuonDto>> GetForReaderAsync(
        int readerId, string? trangThai, CancellationToken cancellationToken)
    {
        var items = await repository.GetForReaderAsync(readerId, NormalizeStatus(trangThai), cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<YeuCauMuonDto?> GetByIdForReaderAsync(
        int readerId, int maYeuCau, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdForReaderAsync(readerId, maYeuCau, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<YeuCauMuonDto?> CancelAsync(
        int readerId, int maYeuCau, CancellationToken cancellationToken)
    {
        var item = await repository.CancelAsync(readerId, maYeuCau, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<IReadOnlyList<YeuCauMuonDto>> SearchAsync(
        string? search, string? trangThai, CancellationToken cancellationToken)
    {
        var items = await repository.SearchAsync(search, NormalizeStatus(trangThai), cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<YeuCauMuonDto?> GetByIdAsync(int maYeuCau, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(maYeuCau, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<YeuCauMuonDto?> ApproveAsync(
        int maYeuCau, string? nguoiDuyet, CancellationToken cancellationToken)
    {
        var request = await repository.GetByIdAsync(maYeuCau, cancellationToken);
        if (request is null) return null;

        // Duyệt lại một yêu cầu đã duyệt là thao tác idempotent: trả về phiếu cũ.
        if (request.TrangThai == YeuCauMuonTrangThai.DaDuyet) return Map(request);
        if (request.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
            throw new BorrowingRuleException(
                $"Yêu cầu đang ở trạng thái '{request.TrangThai}' nên không thể duyệt.");

        var approver = string.IsNullOrWhiteSpace(nguoiDuyet) ? "Thủ thư" : nguoiDuyet.Trim();
        var today = DateTime.Today;
        var loan = await muonTraService.CreateAsync(new TaoMuonTraDto
        {
            MaDocGia = request.MaDocGia,
            NgayMuon = today,
            HanTra = today.AddDays(SoNgayMuon),
            NguoiLap = approver,
            IdempotencyKey = request.IdempotencyKey,
            ChiTiet =
            [
                new TaoChiTietMuonDto { MaSach = request.MaSach, SoLuong = request.SoLuong }
            ]
        }, cancellationToken);

        var approved = await repository.MarkApprovedAsync(
            maYeuCau, approver, loan.MaPhieu, cancellationToken);
        return Map(approved);
    }

    public async Task<YeuCauMuonDto?> RejectAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken)
    {
        var item = await repository.MarkRejectedAsync(
            maYeuCau, nguoiDuyet, lyDo, cancellationToken);
        return item is null ? null : Map(item);
    }

    private static string? NormalizeStatus(string? trangThai) =>
        string.IsNullOrWhiteSpace(trangThai) ? null : trangThai.Trim();

    private static YeuCauMuonDto Map(YeuCauMuon item) => new()
    {
        MaYeuCau = item.MaYeuCau,
        MaDocGia = item.MaDocGia,
        TenDocGia = item.DocGia?.HoTen ?? string.Empty,
        MaSach = item.MaSach,
        TenSach = item.Sach?.TenSach ?? string.Empty,
        ISBN = item.Sach?.ISBN ?? string.Empty,
        SoLuong = item.SoLuong,
        NgayTao = item.NgayTao,
        TrangThai = item.TrangThai,
        GhiChu = item.GhiChu,
        LyDoTuChoi = item.LyDoTuChoi,
        NguoiDuyet = item.NguoiDuyet,
        NgayDuyet = item.NgayDuyet,
        MaPhieu = item.MaPhieu
    };
}
