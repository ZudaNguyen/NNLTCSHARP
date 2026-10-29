using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class NhanVienApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HttpClient Client = ApiSession.Client;

    public async Task<IReadOnlyList<NhanVienDto>> GetAllAsync(string? search, string? status, string? job, CancellationToken token)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(status)) query.Add("trangThai=" + Uri.EscapeDataString(status));
        if (!string.IsNullOrWhiteSpace(job)) query.Add("chucVu=" + Uri.EscapeDataString(job));
        var path = "api/nhanvien" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        using var response = await Client.GetAsync(path, token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<List<NhanVienDto>>(JsonOptions, token) ?? [];
    }

    public Task<NhanVienDto> CreateAsync(CreateNhanVienDto request, CancellationToken token) =>
        SendAsync<NhanVienDto>(HttpMethod.Post, "api/nhanvien", request, token);

    public Task<NhanVienDto> UpdateAsync(int id, UpdateNhanVienDto request, CancellationToken token) =>
        SendAsync<NhanVienDto>(HttpMethod.Put, $"api/nhanvien/{id}", request, token);

    public Task<NhanVienDto> UpdateStatusAsync(int id, string status, CancellationToken token) =>
        SendAsync<NhanVienDto>(HttpMethod.Patch, $"api/nhanvien/{id}/trang-thai", new UpdateTrangThaiNhanVienDto { TrangThai = status }, token);

    public async Task<IReadOnlyList<NhanVienTrangThaiLichSuDto>> GetStatusHistoryAsync(int id, CancellationToken token)
    {
        using var response = await Client.GetAsync($"api/nhanvien/{id}/lich-su-trang-thai", token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<List<NhanVienTrangThaiLichSuDto>>(JsonOptions, token) ?? [];
    }

    private static async Task<T> SendAsync<T>(HttpMethod method, string path, object request, CancellationToken token)
    {
        using var message = new HttpRequestMessage(method, path) { Content = JsonContent.Create(request, options: JsonOptions) };
        using var response = await Client.SendAsync(message, token);
        await EnsureSuccessAsync(response, token);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, token)
            ?? throw new InvalidOperationException("API không trả về dữ liệu nhân viên.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken token)
    {
        if (response.IsSuccessStatusCode) return;
        var fallback = response.StatusCode == HttpStatusCode.NotFound
            ? "Không tìm thấy nhân viên yêu cầu."
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
                    if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String) fallback = detail.GetString() ?? fallback;
                    else if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String) fallback = title.GetString() ?? fallback;
                }
            }
            catch (JsonException) { }
        }
        throw new HttpRequestException(fallback, null, response.StatusCode);
    }
}
