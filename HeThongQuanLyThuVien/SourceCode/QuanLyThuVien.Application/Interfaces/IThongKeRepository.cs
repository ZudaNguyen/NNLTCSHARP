using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IThongKeRepository
{
    Task<DashboardTongQuanDto> GetDashboardTongQuanAsync(DateTime ngayHienTai, DateTime dauThang, DateTime dauThangSau, CancellationToken ct);
    Task<IReadOnlyList<DashboardHoatDongDto>> GetDashboardHoatDongAsync(DateTime ngayHienTai, int take, CancellationToken ct);
    Task<IReadOnlyList<SachDangMuonBaoCaoDto>> GetSachDangMuonAsync(DateTime from, DateTime toExclusive, DateTime ngayHienTai, CancellationToken ct);
    Task<IReadOnlyList<SachMuonNhieuDto>> GetSachMuonNhieuAsync(DateTime from, DateTime toExclusive, int top, CancellationToken ct);
    Task<TienPhatBaoCaoDto> GetTienPhatAsync(DateOnly from, DateOnly to, DateTime fromDateTime, DateTime toExclusive, CancellationToken ct);
}
