using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class DocGiaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HttpClient Client = ApiSession.Client;

    public async Task<IReadOnlyList<DocGiaDto>> GetAllAsync(string? search, string? trangThai, CancellationToken token)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(trangThai)) query.Add("trangThai=" + Uri.EscapeDataString(trangThai));
        var path = "api/docgia" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        using var response = await Client.GetAsync(path, token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<List<DocGiaDto>>(JsonOptions, token) ?? [];
    }

    public async Task<DocGiaDto> GetMeAsync(CancellationToken token)
    {
        using var response = await Client.GetAsync("api/docgia/me", token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<DocGiaDto>(JsonOptions, token)
            ?? throw new InvalidOperationException("API không trả về hồ sơ độc giả.");
    }

    public Task<DocGiaDto> CreateAsync(CreateDocGiaDto request, CancellationToken token) =>
        SendAsync<DocGiaDto>(HttpMethod.Post, "api/docgia", request, token);

    public Task<DocGiaDto> UpdateAsync(int id, UpdateDocGiaDto request, CancellationToken token) =>
        SendAsync<DocGiaDto>(HttpMethod.Put, $"api/docgia/{id}", request, token);

    public Task<DocGiaDto> UpdateTrangThaiAsync(int id, string status, CancellationToken token) =>
        SendAsync<DocGiaDto>(HttpMethod.Patch, $"api/docgia/{id}/trang-thai", new UpdateDocGiaTrangThaiDto { TrangThai = status }, token);

    private static async Task<T> SendAsync<T>(HttpMethod method, string path, object request, CancellationToken token)
    {
        using var message = new HttpRequestMessage(method, path) { Content = JsonContent.Create(request, options: JsonOptions) };
        using var response = await Client.SendAsync(message, token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, token)
            ?? throw new InvalidOperationException("API không trả về dữ liệu độc giả.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken token)
    {
        if (response.IsSuccessStatusCode) return;
        var fallback = response.StatusCode == HttpStatusCode.NotFound
            ? "Không tìm thấy độc giả yêu cầu."
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
                    var first = errors.EnumerateObject().SelectMany(property => property.Value.EnumerateArray())
                        .FirstOrDefault(value => value.ValueKind == JsonValueKind.String);
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
