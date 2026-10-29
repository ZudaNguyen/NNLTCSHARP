using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class MuonTraApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private static readonly HttpClient Client = ApiSession.Client;

    public Task<List<MuonTraDto>> SearchAsync(string? search, string? status, CancellationToken ct) =>
        GetAsync<List<MuonTraDto>>("api/muontra" + Query(search, status), ct);
    public Task<List<MuonTraDto>> SearchHistoryAsync(string? search, string? status, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) values.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(status)) values.Add("trangThai=" + Uri.EscapeDataString(status));
        if (from.HasValue || to.HasValue) values.Add("loaiNgay=ngayMuon");
        if (from.HasValue) values.Add("tuNgay=" + Uri.EscapeDataString(from.Value.ToString("yyyy-MM-dd")));
        if (to.HasValue) values.Add("denNgay=" + Uri.EscapeDataString(to.Value.ToString("yyyy-MM-dd")));
        var query = values.Count == 0 ? string.Empty : "?" + string.Join("&", values);
        return GetAsync<List<MuonTraDto>>("api/muontra" + query, ct);
    }
    public Task<List<MuonTraDto>> GetReaderLoansAsync(int readerId, CancellationToken ct) =>
        GetAsync<List<MuonTraDto>>($"api/muontra/docgia/{readerId}", ct);
    public Task<List<MuonTraDto>> GetMyLoansAsync(CancellationToken ct) =>
        GetAsync<List<MuonTraDto>>("api/muontra/me", ct);
    public Task<MuonTraChiTietDto> GetMyLoanAsync(int id, CancellationToken ct) =>
        GetAsync<MuonTraChiTietDto>($"api/muontra/me/{id}", ct);
    public Task<TienPhatDto> GetMyFinesAsync(int loanId, CancellationToken ct) =>
        GetAsync<TienPhatDto>($"api/muontra/me/{loanId}/tien-phat", ct);
    public Task<MuonTraChiTietDto> GetAsync(int id, CancellationToken ct) => GetAsync<MuonTraChiTietDto>($"api/muontra/{id}", ct);
    public Task<List<SachMuonOptionDto>> GetBooksAsync(CancellationToken ct) => GetAsync<List<SachMuonOptionDto>>("api/muontra/sach-con-hang", ct);
    public Task<List<DocGiaDto>> GetReadersAsync(CancellationToken ct) => GetAsync<List<DocGiaDto>>("api/docgia?trangThai=Hoạt động", ct);
    public Task<MuonTraChiTietDto> CreateAsync(TaoMuonTraDto body, CancellationToken ct) => SendAsync<MuonTraChiTietDto>(HttpMethod.Post, "api/muontra", body, ct);
    public Task<MuonTraChiTietDto> ReturnAsync(int id, int detailId, TraChiTietDto body, CancellationToken ct) => SendAsync<MuonTraChiTietDto>(HttpMethod.Patch, $"api/muontra/{id}/chi-tiet/{detailId}/tra", body, ct);
    public Task<CauHinhTienPhatDto> GetRateAsync(CancellationToken ct) => GetAsync<CauHinhTienPhatDto>("api/muontra/cau-hinh/tien-phat", ct);
    public Task<CauHinhTienPhatDto> SetRateAsync(CapNhatCauHinhTienPhatDto body, CancellationToken ct) => SendAsync<CauHinhTienPhatDto>(HttpMethod.Put, "api/muontra/cau-hinh/tien-phat", body, ct);
    public Task<KhoanThuTienPhatDto> PayFineAsync(int loanId, ThuTienPhatDto body, CancellationToken ct) => SendAsync<KhoanThuTienPhatDto>(HttpMethod.Post, $"api/muontra/{loanId}/tien-phat/thu", body, ct);

    private static string Query(string? search, string? status)
    {
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) values.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(status)) values.Add("trangThai=" + Uri.EscapeDataString(status));
        return values.Count == 0 ? string.Empty : "?" + string.Join("&", values);
    }
    private static async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await Client.GetAsync(path, ct); await EnsureAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct) ?? throw new InvalidOperationException("API không trả về dữ liệu.");
    }
    private static async Task<T> SendAsync<T>(HttpMethod method, string path, object body, CancellationToken ct)
    {
        using var message = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body, options: Json) };
        using var response = await Client.SendAsync(message, ct); await EnsureAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct) ?? throw new InvalidOperationException("API không trả về dữ liệu.");
    }
    private static async Task EnsureAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var message = $"API trả lỗi {(int)response.StatusCode} ({response.StatusCode}).";
        var text = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.TryGetProperty("message", out var m) || root.TryGetProperty("detail", out m)) message = m.GetString() ?? message;
            else if (root.TryGetProperty("errors", out var errors)) message = errors.ToString();
        }
        catch (JsonException) { }
        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
