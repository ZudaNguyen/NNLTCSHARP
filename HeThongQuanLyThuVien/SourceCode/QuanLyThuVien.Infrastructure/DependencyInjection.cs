using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Infrastructure.Persistence;
using QuanLyThuVien.Infrastructure.Repositories;

namespace QuanLyThuVien.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<ISachRepository, SachRepository>();
        services.AddScoped<ITheLoaiRepository, TheLoaiRepository>();
        services.AddScoped<ITacGiaRepository, TacGiaRepository>();
        services.AddScoped<IDocGiaRepository, DocGiaRepository>();
        services.AddScoped<INhanVienRepository, NhanVienRepository>();
        services.AddScoped<IMuonTraRepository, MuonTraRepository>();
        services.AddScoped<IThongKeRepository, ThongKeRepository>();
        services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();

        return services;
    }
}
