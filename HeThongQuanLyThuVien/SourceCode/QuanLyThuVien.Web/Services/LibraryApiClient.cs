using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyThuVien.Web.Services;

/// <summary>
/// Client gọi REST API dùng chung. Mọi lời gọi đều đi qua đây để Web không tự
/// truy cập database và không lặp lại logic nghiệp vụ đã có trong API.
/// </summary>
public sealed class LibraryApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ---------- Xác thực ----------

    public async Task<LoginResult> LoginAsync(string tenDangNhap, string matKhau, CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            new { tenDangNhap, matKhau },
            JsonOptions,
            ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return LoginResult.Failed("Tên đăng nhập hoặc mật khẩu không đúng.");

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            return LoginResult.Failed("Bạn đã thử đăng nhập quá nhiều lần. Vui lòng chờ một phút.");

        if (!response.IsSuccessStatusCode)
            return LoginResult.Failed(await ReadProblemAsync(response, ct) ?? "Không đăng nhập được.");

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions, ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            return LoginResult.Failed("API trả về dữ liệu đăng nhập không hợp lệ.");

        return LoginResult.Success(payload);
    }

    public Task<AuthUserDto?> GetMeAsync(string token, CancellationToken ct = default) =>
        GetAsync<AuthUserDto>("api/auth/me", token, ct);

    public async Task<LoginResult> RegisterReaderAsync(
        RegisterReaderRequest request, CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/register",
            request,
            JsonOptions,
            ct);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            return LoginResult.Failed("Bạn đã thử đăng ký quá nhiều lần. Vui lòng chờ một phút.");

        if (!response.IsSuccessStatusCode)
            return LoginResult.Failed(await ReadProblemAsync(response, ct) ?? "Không đăng ký được.");

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions, ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            return LoginResult.Failed("API trả về dữ liệu đăng ký không hợp lệ.");

        return LoginResult.Success(payload);
    }

    public async Task LogoutAsync(string token, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await httpClient.SendAsync(request, ct);
        // 401 nghĩa là session đã hết hạn ở phía API; phía Web vẫn xóa cookie bình thường.
    }

    // ---------- Danh mục ----------

    public Task<IReadOnlyList<SachDto>?> GetBooksAsync(
        string? search = null,
        int? maTheLoai = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (maTheLoai is > 0) query.Add($"maTheLoai={maTheLoai}");
        var url = query.Count == 0 ? "api/sach" : $"api/sach?{string.Join('&', query)}";
        return GetListAsync<SachDto>(url, token: null, ct);
    }

    public Task<SachDto?> GetBookAsync(int id, CancellationToken ct = default) =>
        GetAsync<SachDto>($"api/sach/{id}", token: null, ct);

    public Task<IReadOnlyList<TheLoaiDto>?> GetCategoriesAsync(CancellationToken ct = default) =>
        GetListAsync<TheLoaiDto>("api/theloai", token: null, ct);

    // ---------- Độc giả ----------

    public Task<DocGiaDto?> GetReaderProfileAsync(string token, CancellationToken ct = default) =>
        GetAsync<DocGiaDto>("api/docgia/me", token, ct);

    public Task<IReadOnlyList<MuonTraDto>?> GetMyLoansAsync(string token, CancellationToken ct = default) =>
        GetListAsync<MuonTraDto>("api/muontra/me", token, ct);

    public Task<MuonTraChiTietDto?> GetMyLoanAsync(int id, string token, CancellationToken ct = default) =>
        GetAsync<MuonTraChiTietDto>($"api/muontra/me/{id}", token, ct);

    public Task<TienPhatDto?> GetMyFineAsync(int id, string token, CancellationToken ct = default) =>
        GetAsync<TienPhatDto>($"api/muontra/me/{id}/tien-phat", token, ct);

    // ---------- Yêu cầu mượn ----------

    public Task<ApiResult<YeuCauMuonDto>> CreateBorrowRequestAsync(
        int maSach, int soLuong, string? ghiChu, string token, CancellationToken ct = default) =>
        PostAsync<YeuCauMuonDto>("api/yeucau", new { maSach, soLuong, ghiChu }, token, ct);

    public Task<IReadOnlyList<YeuCauMuonDto>?> GetMyBorrowRequestsAsync(
        string token, string? trangThai = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(trangThai)
            ? "api/yeucau/me"
            : $"api/yeucau/me?trangThai={Uri.EscapeDataString(trangThai)}";
        return GetListAsync<YeuCauMuonDto>(url, token, ct);
    }

    public Task<ApiResult<YeuCauMuonDto>> CancelBorrowRequestAsync(
        int maYeuCau, string token, CancellationToken ct = default) =>
        PostAsync<YeuCauMuonDto>($"api/yeucau/me/{maYeuCau}/huy", new { }, token, ct);

    // ---------- Hạ tầng ----------

    private async Task<T?> GetAsync<T>(string url, string? token, CancellationToken ct) where T : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private async Task<IReadOnlyList<T>?> GetListAsync<T>(string url, string? token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, ct);
    }

    private async Task<ApiResult<T>> PostAsync<T>(
        string url, object body, string token, CancellationToken ct) where T : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            return ApiResult<T>.Failed("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");

        if (!response.IsSuccessStatusCode)
            return ApiResult<T>.Failed(await ReadProblemAsync(response, ct) ?? "Không thực hiện được yêu cầu.");

        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        return value is null
            ? ApiResult<T>.Failed("API trả về dữ liệu không hợp lệ.")
            : ApiResult<T>.Ok(value);
    }

    private static async Task<string?> ReadProblemAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDto>(JsonOptions, ct);
            if (problem is null) return null;

            if (!string.IsNullOrWhiteSpace(problem.Message)) return problem.Message;

            var firstError = problem.Errors?
                .Values
                .SelectMany(errors => errors)
                .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error));
            return firstError ?? problem.Title ?? problem.Detail;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public sealed record ApiResult<T>(bool Succeeded, T? Value, string? Error)
{
    public static ApiResult<T> Ok(T value) => new(true, value, null);

    public static ApiResult<T> Failed(string error) => new(false, default, error);
}


public sealed record LoginResult(bool Succeeded, string? Error, LoginResponseDto? Payload)
{
    public static LoginResult Success(LoginResponseDto payload) => new(true, null, payload);

    public static LoginResult Failed(string error) => new(false, error, null);
}

// ---------- DTO sao chép từ API (JSON camelCase) ----------

public sealed class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAtUtc { get; init; }
    public AuthUserDto User { get; init; } = new();
}

public sealed class AuthUserDto
{
    public int MaTaiKhoan { get; init; }
    public string TenDangNhap { get; init; } = string.Empty;
    public string TenHienThi { get; init; } = string.Empty;
    public string VaiTro { get; init; } = string.Empty;
    public int? MaNhanVien { get; init; }
    public int? MaDocGia { get; init; }
}

public sealed class RegisterReaderRequest
{
    public string TenDangNhap { get; init; } = string.Empty;
    public string MatKhau { get; init; } = string.Empty;
    public string HoTen { get; init; } = string.Empty;
    public DateTime NgaySinh { get; init; }
    public string GioiTinh { get; init; } = string.Empty;
    public string SoDienThoai { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DiaChi { get; init; }
}

public sealed class SachDto
{
    public int MaSach { get; init; }
    public string ISBN { get; init; } = string.Empty;
    public string TenSach { get; init; } = string.Empty;
    public int MaTheLoai { get; init; }
    public string TenTheLoai { get; init; } = string.Empty;
    public int NamXB { get; init; }
    public string? NhaXB { get; init; }
    public int SoLuong { get; init; }
    public int SoLuongCon { get; init; }
    public string? MoTa { get; init; }
    public string? HinhAnh { get; init; }
    public bool IsDeleted { get; init; }
    public IReadOnlyList<int> MaTacGia { get; init; } = [];
    public IReadOnlyList<string> TenTacGia { get; init; } = [];

    public string TenTacGiaHienThi => TenTacGia.Count == 0 ? "Chưa rõ tác giả" : string.Join(", ", TenTacGia);
    public bool ConHang => SoLuongCon > 0;
}

public sealed class TheLoaiDto
{
    public int MaTheLoai { get; init; }
    public string TenTheLoai { get; init; } = string.Empty;
    public string? MoTa { get; init; }
    public bool IsDeleted { get; init; }
}

public sealed class DocGiaDto
{
    public int MaDocGia { get; init; }
    public string HoTen { get; init; } = string.Empty;
    public DateTime NgaySinh { get; init; }
    public string GioiTinh { get; init; } = string.Empty;
    public string SoDienThoai { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? DiaChi { get; init; }
    public DateTime NgayDangKy { get; init; }
    public string TrangThai { get; init; } = string.Empty;
}

public class MuonTraDto
{
    public int MaPhieu { get; init; }
    public string SoPhieu { get; init; } = string.Empty;
    public int MaDocGia { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public string? NguoiLap { get; init; }
    public DateTime NgayMuon { get; init; }
    public DateTime HanTra { get; init; }
    public int SoLuongConMuon { get; init; }
    public string TrangThai { get; init; } = string.Empty;
}

public sealed class MuonTraChiTietDto : MuonTraDto
{
    public IReadOnlyList<ChiTietMuonTraDto> ChiTiet { get; init; } = [];
}

public sealed class ChiTietMuonTraDto
{
    public int MaChiTiet { get; init; }
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public int SoLuongMuon { get; init; }
    public int SoLuongDaXuLy { get; init; }
    public int SoLuongConMuon { get; init; }
    public IReadOnlyList<LanTraSachDto> LichSuTra { get; init; } = [];
}

public sealed class LanTraSachDto
{
    public int MaLanTra { get; init; }
    public int SoLuongXuLy { get; init; }
    public DateTime NgayTra { get; init; }
    public string TinhTrangSach { get; init; } = string.Empty;
    public int SoNgayTre { get; init; }
    public decimal? MucPhatMoiNgayApDung { get; init; }
    public decimal? SoTienPhatPhaiThu { get; init; }
    public decimal DaThu { get; init; }
    public decimal? ConNo { get; init; }
    public string TrangThaiThanhToan { get; init; } = string.Empty;
}

public sealed class TienPhatDto
{
    public int MaPhieu { get; init; }
    public IReadOnlyList<LanTraSachDto> CacKhoanPhat { get; init; } = [];
    public decimal TongPhaiThu { get; init; }
    public decimal DaThu { get; init; }
    public decimal ConNo { get; init; }
    public bool ChuaCauHinhMucPhat { get; init; }
}

internal sealed class ProblemDto
{
    public string? Title { get; init; }
    public string? Detail { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public int? Status { get; init; }
}

public sealed class YeuCauMuonDto
{
    public int MaYeuCau { get; init; }
    public string SoYeuCau => $"YC-{MaYeuCau:D6}";
    public int MaDocGia { get; init; }
    public string TenDocGia { get; init; } = string.Empty;
    public int MaSach { get; init; }
    public string TenSach { get; init; } = string.Empty;
    public string ISBN { get; init; } = string.Empty;
    public int SoLuong { get; init; }
    public DateTime NgayTao { get; init; }
    public string TrangThai { get; init; } = string.Empty;
    public string? GhiChu { get; init; }
    public string? LyDoTuChoi { get; init; }
    public string? NguoiDuyet { get; init; }
    public DateTime? NgayDuyet { get; init; }
    public int? MaPhieu { get; init; }
    public string? SoPhieu => MaPhieu is null ? null : $"PM-{MaPhieu.Value:D6}";
    public bool CoTheHuy => TrangThai == "Chờ duyệt";
}
