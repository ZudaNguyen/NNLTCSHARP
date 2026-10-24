
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Infrastructure;
using QuanLyThuVien.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Controller
builder.Services.AddControllers();

// Database is optional so the API can still start and serve its ping endpoint
// while the local SQL Server or connection string is unavailable.
var connectionString = builder.Configuration
    .GetConnectionString("LibraryDb");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddInfrastructure(connectionString);
}

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    app.MapGet("/api/sach", async (
        LibraryDbContext db,
        CancellationToken cancellationToken) =>
    {
        var books = await db.Sach
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Results.Ok(books);
    });
}
else
{
    app.MapGet("/api/sach", () => Results.Problem(
        "Chưa cấu hình ConnectionStrings:LibraryDb.",
        statusCode: StatusCodes.Status503ServiceUnavailable));
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
    });

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
            });
    }
}

// Ánh xạ Controller
app.MapControllers();

// Chạy ứng dụng
app.Run();
