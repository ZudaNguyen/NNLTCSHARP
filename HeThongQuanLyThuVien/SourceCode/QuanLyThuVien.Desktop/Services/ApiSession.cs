using System.Net;
using System.Net.Http.Headers;

namespace QuanLyThuVien.Desktop.Services;

internal static class ApiSession
{
    private static string? _accessToken;
    private static readonly HttpClient SharedClient = CreateClient();

    public static event Action? SessionExpired;
    public static HttpClient Client => SharedClient;
    public static bool IsAuthenticated => !string.IsNullOrWhiteSpace(Volatile.Read(ref _accessToken));

    public static void SetAccessToken(string token) => Volatile.Write(ref _accessToken, token);

    public static void ClearAccessToken() => Interlocked.Exchange(ref _accessToken, null);

    private static HttpClient CreateClient()
    {
        var configured = Environment.GetEnvironmentVariable("QUANLYTHUVIEN_API_URL");
        var address = string.IsNullOrWhiteSpace(configured) ? "http://localhost:5058/" : configured.TrimEnd('/') + "/";
        return new HttpClient(new SessionBearerHandler())
        {
            BaseAddress = new Uri(address, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    private sealed class SessionBearerHandler() : DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = Volatile.Read(ref _accessToken);
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                throw new HttpRequestException(
                    "Kết nối API bị gián đoạn hoặc API không phản hồi trong thời gian chờ. " +
                    "Hãy kiểm tra API đang chạy tại localhost:5058 và SQL Server Docker đang hoạt động.",
                    exception);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrWhiteSpace(token)
                && ReferenceEquals(Interlocked.CompareExchange(ref _accessToken, null, token), token))
                SessionExpired?.Invoke();
            return response;
        }
    }
}
