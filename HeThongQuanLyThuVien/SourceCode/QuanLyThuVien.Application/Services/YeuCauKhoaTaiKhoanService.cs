using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Application.Services;

public sealed class AccountLockRequestException(string message) : Exception(message);
public sealed class AccountLockRequestConflictException(string message) : Exception(message);

/// <summary>
/// Nghiệp vụ yêu cầu khóa tài khoản độc giả: thủ thư gửi / hủy, Admin duyệt / từ chối.
/// Chỉ Admin duyệt yêu cầu thì tài khoản mới bị khóa và các phiên đăng nhập bị thu hồi.
/// </summary>
public sealed class YeuCauKhoaTaiKhoanService(IYeuCauKhoaTaiKhoanRepository repository)
    : IYeuCauKhoaTaiKhoanService
{
    public async Task<IReadOnlyList<YeuCauKhoaTaiKhoanDto>> SearchAsync(
        int? maNhanVien, string? trangThai, string? search, CancellationToken cancellationToken)
    {
        var items = await repository.SearchAsync(
            maNhanVien, Normalize(trangThai), Normalize(search), cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<YeuCauKhoaTaiKhoanDto?> GetByIdAsync(
        int maYeuCau, int? maNhanVien, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(maYeuCau, cancellationToken);
        if (item is null) return null;
        if (maNhanVien.HasValue && item.MaNhanVien != maNhanVien.Value) return null;
        return Map(item);
    }

    public Task<IReadOnlyList<TaiKhoanDocGiaOptionDto>> FindReaderAccountsAsync(
        string? search, CancellationToken cancellationToken) =>
        repository.FindReaderAccountsAsync(Normalize(search), cancellationToken);

    public async Task<YeuCauKhoaTaiKhoanDto> CreateAsync(
        int maNhanVien, TaoYeuCauKhoaTaiKhoanDto request, CancellationToken cancellationToken)
    {
        if (request.ThoiHanKhoaNgay is <= 0 or > 365)
            throw new AccountLockRequestException("Thời hạn khóa phải từ 1 đến 365 ngày.");

        var entity = await repository.CreateAsync(maNhanVien, request, Guid.NewGuid(), cancellationToken);
        return Map(entity);
    }

    public async Task<YeuCauKhoaTaiKhoanDto?> CancelAsync(
        int maNhanVien, int maYeuCau, CancellationToken cancellationToken)
    {
        var entity = await repository.CancelAsync(maNhanVien, maYeuCau, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<YeuCauKhoaTaiKhoanDto?> ApproveAsync(
        int maYeuCau, string? nguoiDuyet, CancellationToken cancellationToken)
    {
        var entity = await repository.ApproveAsync(maYeuCau, nguoiDuyet, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<YeuCauKhoaTaiKhoanDto?> RejectAsync(
        int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken)
    {
        var entity = await repository.RejectAsync(maYeuCau, nguoiDuyet, lyDo, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static YeuCauKhoaTaiKhoanDto Map(YeuCauKhoaTaiKhoan item) => new()
    {
        MaYeuCau = item.MaYeuCau,
        MaTaiKhoan = item.MaTaiKhoan,
        TenDangNhap = item.TaiKhoan?.TenDangNhap ?? string.Empty,
        TenHienThi = item.TaiKhoan?.TenHienThi ?? string.Empty,
        MaDocGia = item.TaiKhoan?.MaDocGia,
        TenDocGia = item.TaiKhoan?.DocGia?.HoTen,
        MaNhanVien = item.MaNhanVien,
        TenNhanVien = item.NhanVien?.HoTen ?? string.Empty,
        LyDo = item.LyDo,
        ThoiHanKhoaNgay = item.ThoiHanKhoaNgay,
        TrangThai = item.TrangThai,
        NguoiDuyet = item.NguoiDuyet,
        NgayTao = item.NgayTao,
        NgayDuyet = item.NgayDuyet,
        LyDoTuChoi = item.LyDoTuChoi
    };
}
