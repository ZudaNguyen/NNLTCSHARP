<<<<<<< HEAD
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

=======

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Api.Authentication;
using QuanLyThuVien.Api.Services;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure;
using QuanLyThuVien.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Controller
builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});
var connectionString = builder.Configuration.GetConnectionString("LibraryDb");
var authentication = builder.Services.AddAuthentication(LibraryAuthenticationDefaults.Scheme);
if (!string.IsNullOrWhiteSpace(connectionString))
{
    authentication.AddScheme<AuthenticationSchemeOptions, OpaqueBearerAuthenticationHandler>(
        LibraryAuthenticationDefaults.Scheme, _ => { });
}
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Database is optional so the API can still start and serve its ping endpoint
// while the local SQL Server or connection string is unavailable.

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddScoped<ISachService, SachService>();
    builder.Services.AddScoped<ITheLoaiService, TheLoaiService>();
    builder.Services.AddScoped<ITacGiaService, TacGiaService>();
    builder.Services.AddScoped<IDocGiaService, DocGiaService>();
    builder.Services.AddScoped<INhanVienService, NhanVienService>();
    builder.Services.AddScoped<IMuonTraService, MuonTraService>();
    builder.Services.AddScoped<IThongKeService, ThongKeService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddSingleton<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();
    builder.Services.AddSingleton<IPasswordHashingService, IdentityPasswordHashingService>();
}

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(connectionString))
    await BootstrapAdminInitializer.InitializeAsync(app.Services, app.Configuration);

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (string.IsNullOrWhiteSpace(connectionString))
{
    app.MapGet("/api/sach", () => Results.Problem(
        "Chưa cấu hình ConnectionStrings:LibraryDb.",
        statusCode: StatusCodes.Status503ServiceUnavailable)).AllowAnonymous();
}
else
{
    app.MapControllers();
}

// Các API kiểm tra chỉ sử dụng trong Development
if (app.Environment.IsDevelopment())
{
    // Kiểm tra API
    app.MapGet("/api/dev/ping", () =>
    {
        return Results.Ok(new
        {
            message = "API dang hoat dong"
        });
    }).AllowAnonymous();

    // Only expose the database check when a connection string is configured.
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        app.MapGet("/api/dev/db-check",
            async (LibraryDbContext db) =>
            {
                var totalBooks = await db.Sach
                    .AsNoTracking()
                    .CountAsync();

                return Results.Ok(new
                {
                    connected = true,
                    database = "QuanLyThuVienDB",
                    totalBooks
                });
            }).RequireAuthorization();
    }
}

// Chạy ứng dụng
>>>>>>> origin/feat/desktop-ui-theme
app.Run();
