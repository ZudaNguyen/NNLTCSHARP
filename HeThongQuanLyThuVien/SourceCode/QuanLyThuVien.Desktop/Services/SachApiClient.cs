using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class SachApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HttpClient HttpClient = ApiSession.Client;

    public async Task<IReadOnlyList<SachDto>> GetBooksAsync(
        string? search,
        int? maTheLoai,
        bool includeDeleted,
        bool onlyDeleted,
        CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (maTheLoai.HasValue)
            query.Add($"maTheLoai={maTheLoai.Value}");
        if (includeDeleted)
            query.Add("includeDeleted=true");
        if (onlyDeleted)
            query.Add("onlyDeleted=true");

        var path = "api/sach" + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        using var response = await HttpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<SachDto>>(JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task<SachLookupsDto> GetLookupsAsync(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync("api/sach/lookups", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<SachLookupsDto>(JsonOptions, cancellationToken)
            ?? new SachLookupsDto();
    }

    public async Task<SachDto> CreateAsync(SachWriteDto request, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.PostAsJsonAsync("api/sach", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<SachDto>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("API không trả về dữ liệu sách vừa tạo.");
    }

    public async Task<SachDto> UpdateAsync(int id, SachWriteDto request, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.PutAsJsonAsync($"api/sach/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<SachDto>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("API không trả về dữ liệu sách vừa cập nhật.");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.DeleteAsync($"api/sach/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.PostAsync($"api/sach/{id}/restore", content: null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var message = response.StatusCode == HttpStatusCode.NotFound
            ? "Không tìm thấy sách yêu cầu."
            : $"API trả về lỗi {(int)response.StatusCode}.";

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.TryGetProperty("errors", out var errors)
                    && errors.ValueKind == JsonValueKind.Object)
                {
                    var firstError = errors.EnumerateObject()
                        .SelectMany(property => property.Value.EnumerateArray())
                        .FirstOrDefault(value => value.ValueKind == JsonValueKind.String);
                    if (firstError.ValueKind == JsonValueKind.String)
                        message = firstError.GetString() ?? message;
                }

                if (message == $"API trả về lỗi {(int)response.StatusCode}.")
                {
                    if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                        message = detail.GetString() ?? message;
                    else if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        message = title.GetString() ?? message;
                }
            }
            catch (JsonException)
            {
                // Keep the status-based message if the server didn't return ProblemDetails JSON.
            }
        }

        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
