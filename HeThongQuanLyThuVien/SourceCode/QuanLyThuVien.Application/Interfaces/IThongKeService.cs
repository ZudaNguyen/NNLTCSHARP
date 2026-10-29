using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IThongKeService
{
    Task<DashboardTongQuanDto> GetDashboardTongQuanAsync(CancellationToken ct);
    Task<IReadOnlyList<DashboardHoatDongDto>> GetDashboardHoatDongAsync(CancellationToken ct);
    Task<IReadOnlyList<SachDangMuonBaoCaoDto>> GetSachDangMuonAsync(DateOnly? from, DateOnly? to, CancellationToken ct);
    Task<IReadOnlyList<SachMuonNhieuDto>> GetSachMuonNhieuAsync(DateOnly? from, DateOnly? to, int top, CancellationToken ct);
    Task<TienPhatBaoCaoDto> GetTienPhatAsync(DateOnly? from, DateOnly? to, CancellationToken ct);
}
