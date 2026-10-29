using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;

namespace QuanLyThuVien.Application.Services;

public sealed class ThongKeService(IThongKeRepository repository) : IThongKeService
{
    public Task<DashboardTongQuanDto> GetDashboardTongQuanAsync(CancellationToken ct)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        return repository.GetDashboardTongQuanAsync(today, monthStart, monthStart.AddMonths(1), ct);
    }

    public Task<IReadOnlyList<DashboardHoatDongDto>> GetDashboardHoatDongAsync(CancellationToken ct) =>
        repository.GetDashboardHoatDongAsync(DateTime.Today, 20, ct);

    public Task<IReadOnlyList<SachDangMuonBaoCaoDto>> GetSachDangMuonAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var range = ResolveRange(from, to);
        return repository.GetSachDangMuonAsync(range.From, range.ToExclusive, DateTime.Today, ct);
    }

    public Task<IReadOnlyList<SachMuonNhieuDto>> GetSachMuonNhieuAsync(DateOnly? from, DateOnly? to, int top, CancellationToken ct)
    {
        if (top is < 1 or > 100) throw new ReportRequestException("Tham số top phải từ 1 đến 100.");
        var range = ResolveRange(from, to);
        return repository.GetSachMuonNhieuAsync(range.From, range.ToExclusive, top, ct);
    }

    public Task<TienPhatBaoCaoDto> GetTienPhatAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var range = ResolveRange(from, to);
        return repository.GetTienPhatAsync(range.FromDate, range.ToDate, range.From, range.ToExclusive, ct);
    }

    private static (DateTime From, DateTime ToExclusive, DateOnly FromDate, DateOnly ToDate) ResolveRange(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = from ?? new DateOnly(today.Year, today.Month, 1);
        var end = to ?? today;
        if (start > end) throw new ReportRequestException("Ngày bắt đầu không được sau ngày kết thúc.");
        if (end == DateOnly.MaxValue) throw new ReportRequestException("Ngày kết thúc không hợp lệ.");
        return (start.ToDateTime(TimeOnly.MinValue), end.AddDays(1).ToDateTime(TimeOnly.MinValue), start, end);
    }
}

public sealed class ReportRequestException(string message) : Exception(message);
