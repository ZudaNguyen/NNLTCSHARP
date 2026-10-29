using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class ThongKeApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private static HttpClient Client => ApiSession.Client;

    public Task<DashboardTongQuanDto> GetDashboardTongQuanAsync(CancellationToken ct) =>
        GetAsync<DashboardTongQuanDto>("api/dashboard/tong-quan", ct);

    public async Task<IReadOnlyList<DashboardHoatDongDto>> GetDashboardHoatDongAsync(CancellationToken ct) =>
        await GetAsync<List<DashboardHoatDongDto>>("api/dashboard/hoat-dong", ct);

    public Task<IReadOnlyList<SachDangMuonBaoCaoDto>> GetSachDangMuonAsync(DateOnly from, DateOnly to, CancellationToken ct) =>
        GetListAsync<SachDangMuonBaoCaoDto>("api/baocao/sach-dang-muon", from, to, null, ct);

    public Task<IReadOnlyList<SachMuonNhieuDto>> GetSachMuonNhieuAsync(DateOnly from, DateOnly to, int top, CancellationToken ct) =>
        GetListAsync<SachMuonNhieuDto>("api/baocao/sach-muon-nhieu", from, to, top, ct);

    public Task<TienPhatBaoCaoDto> GetTienPhatAsync(DateOnly from, DateOnly to, CancellationToken ct) =>
        GetAsync<TienPhatBaoCaoDto>($"api/baocao/tien-phat?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    private static async Task<IReadOnlyList<T>> GetListAsync<T>(string path, DateOnly from, DateOnly to, int? top, CancellationToken ct)
    {
        var query = $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}" + (top.HasValue ? $"&top={top.Value}" : string.Empty);
        return await GetAsync<List<T>>(path + query, ct);
    }

    private static async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await Client.GetAsync(path, ct);
        if (!response.IsSuccessStatusCode)
        {
            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                HttpStatusCode.Forbidden => "Tài khoản không có quyền xem Dashboard hoặc báo cáo.",
                _ => $"API trả về lỗi {(int)response.StatusCode}."
            };
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var json = JsonDocument.Parse(body);
                    var root = json.RootElement;
                    if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        message = title.GetString() ?? message;
                    else if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                        message = detail.GetString() ?? message;
                }
                catch (JsonException) { }
            }
            throw new HttpRequestException(message, null, response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct)
            ?? throw new InvalidOperationException("API không trả về dữ liệu thống kê.");
    }
}
