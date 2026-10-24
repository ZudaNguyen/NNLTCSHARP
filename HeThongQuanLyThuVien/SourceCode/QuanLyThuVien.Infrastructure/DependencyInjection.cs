using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyThuVien.Infrastructure.Persistence;

namespace QuanLyThuVien.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
