using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Desktop.Services;

internal sealed class AuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private static readonly HttpClient AnonymousClient = CreateAnonymousClient();

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct)
    {
        using var response = await AnonymousClient.PostAsJsonAsync("api/auth/login", request, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions, ct)
            ?? throw new InvalidOperationException("API không trả về thông tin phiên đăng nhập.");
        ApiSession.SetAccessToken(result.AccessToken);
        return result;
    }

    public async Task<AuthUserDto> GetMeAsync(CancellationToken ct)
    {
        using var response = await ApiSession.Client.GetAsync("api/auth/me", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<AuthUserDto>(JsonOptions, ct)
            ?? throw new InvalidOperationException("API không trả về thông tin tài khoản.");
    }

    public async Task LogoutAsync(CancellationToken ct)
    {
        try
        {
            using var response = await ApiSession.Client.PostAsync("api/auth/logout", content: null, ct);
            await EnsureSuccessAsync(response, ct);
        }
        finally { ApiSession.ClearAccessToken(); }
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken ct)
    {
        using var response = await ApiSession.Client.PostAsJsonAsync("api/auth/change-password", request, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        ApiSession.ClearAccessToken();
    }

    private static HttpClient CreateAnonymousClient()
    {
        var configured = Environment.GetEnvironmentVariable("QUANLYTHUVIEN_API_URL");
        var address = string.IsNullOrWhiteSpace(configured) ? "http://localhost:5058/" : configured.TrimEnd('/') + "/";
        return new HttpClient { BaseAddress = new Uri(address, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(20) };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Tên đăng nhập/mật khẩu không đúng hoặc tài khoản không hoạt động.",
            HttpStatusCode.Forbidden => "Tài khoản không có quyền thực hiện thao tác này.",
            HttpStatusCode.TooManyRequests => "Bạn đã thử đăng nhập quá nhiều lần. Hãy chờ rồi thử lại.",
            _ => $"API trả về lỗi {(int)response.StatusCode}."
        };
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var json = JsonDocument.Parse(body);
                var root = json.RootElement;
                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String
                    && response.StatusCode != HttpStatusCode.Unauthorized)
                    fallback = title.GetString() ?? fallback;
            }
            catch (JsonException) { }
        }
        throw new HttpRequestException(fallback, null, response.StatusCode);
    }
}
