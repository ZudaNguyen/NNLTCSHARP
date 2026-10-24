using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Infrastructure.Persistence;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : DbContext(options)
{
    public DbSet<Sach> Sach => Set<Sach>();
    public DbSet<TheLoai> TheLoai => Set<TheLoai>();
    public DbSet<TacGia> TacGia => Set<TacGia>();
    public DbSet<SachTacGia> SachTacGia => Set<SachTacGia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sach>(entity =>
        {
            entity.ToTable("Sach");
            entity.HasKey(sach => sach.MaSach);
            entity.Property(sach => sach.MaSach).ValueGeneratedOnAdd();
            entity.Property(sach => sach.ISBN).HasMaxLength(20).IsRequired();
            entity.Property(sach => sach.TenSach).HasMaxLength(300).IsRequired();
            entity.Property(sach => sach.NhaXB).HasMaxLength(200);
            entity.Property(sach => sach.MoTa).HasMaxLength(4000);
            entity.Property(sach => sach.HinhAnh).HasMaxLength(1000);
            entity.Property(sach => sach.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(sach => sach.ISBN).IsUnique();
            entity.HasOne(sach => sach.TheLoai)
                .WithMany(theLoai => theLoai.Sachs)
                .HasForeignKey(sach => sach.MaTheLoai)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.ToTable("TheLoai");
            entity.HasKey(theLoai => theLoai.MaTheLoai);
            entity.Property(theLoai => theLoai.TenTheLoai).HasMaxLength(150).IsRequired();
            entity.Property(theLoai => theLoai.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(theLoai => theLoai.TenTheLoai).IsUnique();
        });

        modelBuilder.Entity<TacGia>(entity =>
        {
            entity.ToTable("TacGia");
            entity.HasKey(tacGia => tacGia.MaTacGia);
            entity.Property(tacGia => tacGia.TenTacGia).HasMaxLength(200).IsRequired();
            entity.Property(tacGia => tacGia.QuocTich).HasMaxLength(100);
            entity.Property(tacGia => tacGia.GioiThieu).HasMaxLength(2000);
            entity.Property(tacGia => tacGia.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(tacGia => tacGia.TenTacGia).IsUnique();
        });

        modelBuilder.Entity<SachTacGia>(entity =>
        {
            entity.ToTable("Sach_TacGia");
            entity.HasKey(link => new { link.MaSach, link.MaTacGia });
            entity.HasOne(link => link.Sach)
                .WithMany(sach => sach.SachTacGias)
                .HasForeignKey(link => link.MaSach)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(link => link.TacGia)
                .WithMany(tacGia => tacGia.SachTacGias)
                .HasForeignKey(link => link.MaTacGia)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
