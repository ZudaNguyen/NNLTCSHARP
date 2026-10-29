using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class YeuCauMuonApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private static readonly HttpClient Client = ApiSession.Client;

    public Task<List<YeuCauMuonDto>> SearchAsync(string? status, string? search, CancellationToken ct) =>
        GetAsync<List<YeuCauMuonDto>>("api/yeucau" + Query(status, search), ct);

    public Task<YeuCauMuonDto> ApproveAsync(int id, CancellationToken ct) =>
        SendAsync<YeuCauMuonDto>(HttpMethod.Post, $"api/yeucau/{id}/duyet", new { }, ct);

    public Task<YeuCauMuonDto> RejectAsync(int id, string? lyDo, CancellationToken ct) =>
        SendAsync<YeuCauMuonDto>(
            HttpMethod.Post, $"api/yeucau/{id}/tu-choi", new TuChoiYeuCauMuonDto { LyDo = lyDo }, ct);

    private static string Query(string? status, string? search)
    {
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) values.Add("trangThai=" + Uri.EscapeDataString(status.Trim()));
        if (!string.IsNullOrWhiteSpace(search)) values.Add("search=" + Uri.EscapeDataString(search.Trim()));
        return values.Count == 0 ? string.Empty : "?" + string.Join("&", values);
    }

    private static async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await Client.GetAsync(path, ct);
        await EnsureAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct)
            ?? throw new InvalidOperationException("API không trả về dữ liệu yêu cầu mượn.");
    }

    private static async Task<T> SendAsync<T>(HttpMethod method, string path, object body, CancellationToken ct)
    {
        using var message = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body, options: Json) };
        using var response = await Client.SendAsync(message, ct);
        await EnsureAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct)
            ?? throw new InvalidOperationException("API không trả về dữ liệu yêu cầu mượn.");
    }

    private static async Task EnsureAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.",
            HttpStatusCode.Forbidden => "Bạn không có quyền thực hiện thao tác này.",
            HttpStatusCode.NotFound => "Không tìm thấy yêu cầu mượn.",
            _ => $"API trả về lỗi {(int)response.StatusCode}."
        };

        var text = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                message = m.GetString() ?? message;
            else if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                message = detail.GetString() ?? message;
            else if (root.TryGetProperty("errors", out var errors))
                message = errors.ToString();
        }
        catch (JsonException) { }

        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
