using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;

namespace QuanLyThuVien.Api.Services;

public static class BootstrapAdminInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        var section = configuration.GetSection("Authentication:BootstrapAdmin");
        var username = section["UserName"];
        var displayName = section["DisplayName"];
        var password = section["Password"];

        await using var scope = services.CreateAsyncScope();
        var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
        await accountService.BootstrapAdminAsync(new AccountBootstrapOptions
        {
            TenDangNhap = username,
            TenHienThi = displayName,
            MatKhau = password
        }, ct);
    }
}
