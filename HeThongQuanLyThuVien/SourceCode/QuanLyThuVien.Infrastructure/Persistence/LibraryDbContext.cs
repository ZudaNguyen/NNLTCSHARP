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
    public DbSet<DocGia> DocGia => Set<DocGia>();
    public DbSet<NhanVien> NhanVien => Set<NhanVien>();
    public DbSet<NhanVienTrangThaiLichSu> NhanVienTrangThaiLichSu => Set<NhanVienTrangThaiLichSu>();
    public DbSet<PhieuMuon> PhieuMuon => Set<PhieuMuon>();
    public DbSet<ChiTietPhieuMuon> ChiTietPhieuMuon => Set<ChiTietPhieuMuon>();
    public DbSet<ChiTietTraSach> ChiTietTraSach => Set<ChiTietTraSach>();
    public DbSet<ThuTienPhat> ThuTienPhat => Set<ThuTienPhat>();
    public DbSet<CauHinhTienPhat> CauHinhTienPhat => Set<CauHinhTienPhat>();

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
            entity.Property(theLoai => theLoai.MoTa).HasMaxLength(2000);
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

        modelBuilder.Entity<DocGia>(entity =>
        {
            entity.ToTable("DocGia");
            entity.HasKey(reader => reader.MaDocGia);
            entity.Property(reader => reader.MaDocGia).ValueGeneratedOnAdd();
            entity.Property(reader => reader.HoTen).HasMaxLength(200).IsRequired();
            entity.Property(reader => reader.NgaySinh).HasColumnType("date");
            entity.Property(reader => reader.GioiTinh).HasMaxLength(20).IsRequired();
            entity.Property(reader => reader.SoDienThoai).HasMaxLength(11).IsRequired();
            entity.Property(reader => reader.Email).HasMaxLength(254);
            entity.Property(reader => reader.DiaChi).HasMaxLength(500);
            entity.Property(reader => reader.NgayDangKy).HasColumnType("date").HasDefaultValueSql("CONVERT(date, GETDATE())");
            entity.Property(reader => reader.TrangThai).HasMaxLength(20).HasDefaultValue("Hoạt động").IsRequired();
            entity.HasIndex(reader => reader.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.HasIndex(reader => reader.HoTen);
            entity.HasIndex(reader => reader.SoDienThoai);
            entity.HasIndex(reader => reader.TrangThai);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.ToTable("NhanVien");
            entity.HasKey(employee => employee.MaNhanVien);
            entity.Property(employee => employee.MaNhanVien).ValueGeneratedOnAdd();
            entity.Property(employee => employee.HoTen).HasMaxLength(200).IsRequired();
            entity.Property(employee => employee.SoDienThoai).HasMaxLength(11);
            entity.Property(employee => employee.Email).HasMaxLength(254);
            entity.Property(employee => employee.ChucVu).HasMaxLength(100).IsRequired();
            entity.Property(employee => employee.TrangThai).HasMaxLength(20).HasDefaultValue("Hoạt động").IsRequired();
            entity.Property(employee => employee.NgayTao).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            entity.HasIndex(employee => employee.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.HasIndex(employee => employee.SoDienThoai).IsUnique().HasFilter("[SoDienThoai] IS NOT NULL");
            entity.HasIndex(employee => employee.HoTen);
            entity.HasIndex(employee => employee.TrangThai);
            entity.HasCheckConstraint("CK_NhanVien_TrangThai", "[TrangThai] IN (N'Hoạt động', N'Ngừng hoạt động')");
        });

        modelBuilder.Entity<NhanVienTrangThaiLichSu>(entity =>
        {
            entity.ToTable("NhanVien_TrangThaiLichSu");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Id).ValueGeneratedOnAdd();
            entity.Property(history => history.TrangThaiCu).HasMaxLength(20).IsRequired();
            entity.Property(history => history.TrangThaiMoi).HasMaxLength(20).IsRequired();
            entity.Property(history => history.ThayDoiLuc).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            entity.HasIndex(history => new { history.MaNhanVien, history.ThayDoiLuc });
            entity.HasOne<NhanVien>().WithMany().HasForeignKey(history => history.MaNhanVien).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PhieuMuon>(entity =>
        {
            entity.ToTable("PhieuMuon"); entity.HasKey(x => x.MaPhieu);
            entity.Property(x => x.MaPhieu).ValueGeneratedOnAdd();
            entity.Property(x => x.NguoiLap).HasMaxLength(100);
            entity.Property(x => x.NgayMuon).HasColumnType("date");
            entity.Property(x => x.HanTra).HasColumnType("date");
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasOne(x => x.DocGia).WithMany().HasForeignKey(x => x.MaDocGia).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ChiTietPhieuMuon>(entity =>
        {
            entity.ToTable("ChiTiet_PhieuMuon"); entity.HasKey(x => x.MaChiTiet);
            entity.Property(x => x.MaChiTiet).ValueGeneratedOnAdd();
            entity.HasIndex(x => new { x.MaPhieu, x.MaSach }).IsUnique();
            entity.HasOne(x => x.PhieuMuon).WithMany(x => x.ChiTiets).HasForeignKey(x => x.MaPhieu).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Sach).WithMany().HasForeignKey(x => x.MaSach).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ChiTietTraSach>(entity =>
        {
            entity.ToTable("ChiTietTraSach"); entity.HasKey(x => x.MaLanTra);
            entity.Property(x => x.MaLanTra).ValueGeneratedOnAdd();
            entity.Property(x => x.TinhTrangSach).HasMaxLength(40).IsRequired();
            entity.Property(x => x.MucPhatMoiNgayApDung).HasColumnType("decimal(18,2)");
            entity.Property(x => x.SoTienPhatPhaiThu).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasOne(x => x.ChiTiet).WithMany(x => x.LichSuTra).HasForeignKey(x => x.MaChiTiet).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ThuTienPhat>(entity =>
        {
            entity.ToTable("ThuTienPhat"); entity.HasKey(x => x.MaKhoanThu);
            entity.Property(x => x.MaKhoanThu).ValueGeneratedOnAdd();
            entity.Property(x => x.SoTien).HasColumnType("decimal(18,2)");
            entity.Property(x => x.NguoiThu).HasMaxLength(100);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasOne(x => x.LanTra).WithMany(x => x.CacKhoanThu).HasForeignKey(x => x.MaLanTra).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CauHinhTienPhat>(entity =>
        {
            entity.ToTable("CauHinhTienPhat"); entity.HasKey(x => x.Id);
            entity.Property(x => x.MucPhatMoiNgay).HasColumnType("decimal(18,2)");
            entity.Property(x => x.CapNhatLuc).HasColumnType("datetime2");
        });
    }
}
