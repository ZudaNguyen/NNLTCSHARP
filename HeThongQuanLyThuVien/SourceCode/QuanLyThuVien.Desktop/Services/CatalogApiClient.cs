using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class CatalogApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HttpClient Client = CreateClient();

    public Task<IReadOnlyList<TheLoaiDto>> GetTheLoaiAsync(string? search, bool includeDeleted, bool onlyDeleted, CancellationToken token) =>
        GetListAsync<TheLoaiDto>("api/theloai", search, includeDeleted, onlyDeleted, token);

    public Task<IReadOnlyList<TacGiaDto>> GetTacGiaAsync(string? search, bool includeDeleted, bool onlyDeleted, CancellationToken token) =>
        GetListAsync<TacGiaDto>("api/tacgia", search, includeDeleted, onlyDeleted, token);

    public Task<TheLoaiDto> CreateTheLoaiAsync(TheLoaiWriteDto request, CancellationToken token) =>
        SendAsync<TheLoaiDto>(HttpMethod.Post, "api/theloai", request, token);

    public Task<TheLoaiDto> UpdateTheLoaiAsync(int id, TheLoaiWriteDto request, CancellationToken token) =>
        SendAsync<TheLoaiDto>(HttpMethod.Put, $"api/theloai/{id}", request, token);

    public Task DeleteTheLoaiAsync(int id, CancellationToken token) =>
        SendAsync(HttpMethod.Delete, $"api/theloai/{id}", token);

    public Task RestoreTheLoaiAsync(int id, CancellationToken token) =>
        SendAsync(HttpMethod.Post, $"api/theloai/{id}/restore", token);

    public Task<TacGiaDto> CreateTacGiaAsync(TacGiaWriteDto request, CancellationToken token) =>
        SendAsync<TacGiaDto>(HttpMethod.Post, "api/tacgia", request, token);

    public Task<TacGiaDto> UpdateTacGiaAsync(int id, TacGiaWriteDto request, CancellationToken token) =>
        SendAsync<TacGiaDto>(HttpMethod.Put, $"api/tacgia/{id}", request, token);

    public Task DeleteTacGiaAsync(int id, CancellationToken token) =>
        SendAsync(HttpMethod.Delete, $"api/tacgia/{id}", token);

    public Task RestoreTacGiaAsync(int id, CancellationToken token) =>
        SendAsync(HttpMethod.Post, $"api/tacgia/{id}/restore", token);

    private static async Task<IReadOnlyList<T>> GetListAsync<T>(string path, string? search, bool includeDeleted, bool onlyDeleted, CancellationToken token)
    {
        var query = new List<string>();
        if (includeDeleted) query.Add("includeDeleted=true");
        if (onlyDeleted) query.Add("onlyDeleted=true");
        if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
        var suffix = query.Count == 0 ? string.Empty : "?" + string.Join("&", query);
        using var response = await Client.GetAsync(path + suffix, token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, token) ?? [];
    }

    private static async Task<T> SendAsync<T>(HttpMethod method, string path, object request, CancellationToken token)
    {
        using var message = new HttpRequestMessage(method, path) { Content = JsonContent.Create(request, options: JsonOptions) };
        using var response = await Client.SendAsync(message, token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, token)
            ?? throw new InvalidOperationException("API không trả về dữ liệu.");
    }

    private static async Task SendAsync(HttpMethod method, string path, CancellationToken token)
    {
        using var response = await Client.SendAsync(new HttpRequestMessage(method, path), token);
        await EnsureSuccessAsync(response, token);
    }

    private static HttpClient CreateClient()
    {
        var configuredAddress = Environment.GetEnvironmentVariable("QUANLYTHUVIEN_API_URL");
        var address = string.IsNullOrWhiteSpace(configuredAddress) ? "http://localhost:5058/" : configuredAddress.TrimEnd('/') + "/";
        return new HttpClient { BaseAddress = new Uri(address, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(20) };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken token)
    {
        if (response.IsSuccessStatusCode) return;
        var fallback = response.StatusCode == HttpStatusCode.NotFound
            ? "Không tìm thấy dữ liệu yêu cầu."
            : $"API trả về lỗi {(int)response.StatusCode}.";
        var body = await response.Content.ReadAsStringAsync(token);
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var first = errors.EnumerateObject().SelectMany(p => p.Value.EnumerateArray())
                        .FirstOrDefault(v => v.ValueKind == JsonValueKind.String);
                    if (first.ValueKind == JsonValueKind.String) fallback = first.GetString() ?? fallback;
                }
                if (fallback == $"API trả về lỗi {(int)response.StatusCode}.")
                {
                    if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                        fallback = detail.GetString() ?? fallback;
                    else if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        fallback = title.GetString() ?? fallback;
                }
            }
            catch (JsonException) { }
        }
        throw new HttpRequestException(fallback, null, response.StatusCode);
    }
}
