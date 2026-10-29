using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class AccountApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<AccountDto>> SearchAsync(string? search, string? role, string? status, CancellationToken ct)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
        if (!string.IsNullOrWhiteSpace(role)) query.Add("vaiTro=" + Uri.EscapeDataString(role));
        if (!string.IsNullOrWhiteSpace(status)) query.Add("trangThai=" + Uri.EscapeDataString(status));
        var path = "api/taikhoan" + (query.Count == 0 ? "" : "?" + string.Join("&", query));
        using var response = await ApiSession.Client.GetAsync(path, ct);
        await EnsureAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<AccountDto>>(Json, ct) ?? [];
    }

    public Task<AccountDto> CreateAsync(CreateAccountRequestDto request, CancellationToken ct) =>
        SendAsync<AccountDto>(HttpMethod.Post, "api/taikhoan", request, ct);

    public Task<AccountDto> UpdateAsync(int id, UpdateAccountRequestDto request, CancellationToken ct) =>
        SendAsync<AccountDto>(HttpMethod.Put, $"api/taikhoan/{id}", request, ct);

    public Task<AccountDto> UpdateStatusAsync(int id, string status, CancellationToken ct) =>
        SendAsync<AccountDto>(HttpMethod.Patch, $"api/taikhoan/{id}/trang-thai", new UpdateAccountStatusRequestDto { TrangThai = status }, ct);

    public async Task ResetPasswordAsync(int id, string password, CancellationToken ct)
    {
        using var response = await ApiSession.Client.PostAsJsonAsync($"api/taikhoan/{id}/reset-password", new ResetAccountPasswordRequestDto { MatKhauMoi = password }, Json, ct);
        await EnsureAsync(response, ct);
    }

    private static async Task<T> SendAsync<T>(HttpMethod method, string path, object body, CancellationToken ct)
    {
        using var response = await ApiSession.Client.SendAsync(new HttpRequestMessage(method, path) { Content = JsonContent.Create(body, options: Json) }, ct);
        await EnsureAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct) ?? throw new InvalidOperationException("API không trả về tài khoản.");
    }

    private static async Task EnsureAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.",
            HttpStatusCode.Forbidden => "Chỉ Admin được quản lý tài khoản.",
            HttpStatusCode.Conflict => "Dữ liệu bị trùng hoặc vi phạm quy tắc quản lý Admin cuối cùng.",
            _ => $"API trả về lỗi {(int)response.StatusCode}."
        };
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String
                    && response.StatusCode != HttpStatusCode.Unauthorized)
                    message = title.GetString() ?? message;
            }
            catch (JsonException) { }
        }
        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
