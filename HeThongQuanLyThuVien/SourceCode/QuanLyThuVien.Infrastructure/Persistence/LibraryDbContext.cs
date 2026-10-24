using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Infrastructure.Persistence;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : DbContext(options)
{
    public DbSet<Sach> Sach => Set<Sach>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sach>()
            .HasKey(sach => sach.Id);

        modelBuilder.Entity<Sach>()
            .ToTable("Sach");

        modelBuilder.Entity<Sach>()
            .Property(sach => sach.Id)
            .ValueGeneratedOnAdd();
    }
}
