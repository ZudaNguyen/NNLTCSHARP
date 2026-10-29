:ON ERROR EXIT
/*
  HeThongQuanLyThuVien - complete schema and minimal demo data
  Target: local/demo SQL Server only. This script creates QuanLyThuVienDB if absent.
  It NEVER drops a database/table and NEVER deletes existing data.
  Guard: refuses to run if any application table already exists.
  For an existing database, use the documented module scripts after backup and use
  database/seeds/QuanLyThuVien.DemoSeeder.csproj for guarded demo data instead.
  No login accounts/passwords are inserted; bootstrap Admin through API User Secrets.
*/
IF DB_ID(N'QuanLyThuVienDB') IS NULL
BEGIN
    CREATE DATABASE [QuanLyThuVienDB];
END;
GO
USE [QuanLyThuVienDB];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF EXISTS
(
    SELECT 1 FROM sys.tables
    WHERE schema_id = SCHEMA_ID(N'dbo')
      AND name IN (N'TheLoai', N'TacGia', N'Sach', N'Sach_TacGia', N'DocGia', N'NhanVien', N'NhanVien_TrangThaiLichSu', N'TaiKhoan', N'TaiKhoan_Session', N'PhieuMuon', N'ChiTiet_PhieuMuon', N'ChiTietTraSach', N'ThuTienPhat', N'CauHinhTienPhat')
)
BEGIN
    ;THROW 52000, 'Application tables already exist. This full setup script is for an empty demo database only; see docs/SETUP_GUIDE.md.', 1;
END;
GO

-- ===== Schema module: BooksSchema.sql =====

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.TheLoai', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TheLoai
    (
        MaTheLoai int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TheLoai PRIMARY KEY,
        TenTheLoai nvarchar(150) NOT NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_TheLoai_IsDeleted DEFAULT (0)
    );
END;

IF OBJECT_ID(N'dbo.TacGia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TacGia
    (
        MaTacGia int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TacGia PRIMARY KEY,
        TenTacGia nvarchar(200) NOT NULL,
        QuocTich nvarchar(100) NULL,
        GioiThieu nvarchar(2000) NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_TacGia_IsDeleted DEFAULT (0)
    );
END;

IF OBJECT_ID(N'dbo.Sach', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sach
    (
        MaSach int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sach PRIMARY KEY
    );
END;
ELSE IF COL_LENGTH(N'dbo.Sach', N'MaSach') IS NULL AND COL_LENGTH(N'dbo.Sach', N'Id') IS NOT NULL
BEGIN
    EXEC sys.sp_rename N'dbo.Sach.Id', N'MaSach', N'COLUMN';
END;

IF COL_LENGTH(N'dbo.Sach', N'MaSach') IS NULL
    THROW 51000, 'dbo.Sach must have an identity key named MaSach or Id.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin')
    INSERT dbo.TheLoai (TenTheLoai) VALUES (N'Công nghệ thông tin');
IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Văn học kinh điển')
    INSERT dbo.TheLoai (TenTheLoai) VALUES (N'Văn học kinh điển');
IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Kinh tế - Quản trị')
    INSERT dbo.TheLoai (TenTheLoai) VALUES (N'Kinh tế - Quản trị');
IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ năng sống')
    INSERT dbo.TheLoai (TenTheLoai) VALUES (N'Kỹ năng sống');

IF NOT EXISTS (SELECT 1 FROM dbo.TacGia WHERE TenTacGia = N'Robert C. Martin')
    INSERT dbo.TacGia (TenTacGia) VALUES (N'Robert C. Martin');
IF NOT EXISTS (SELECT 1 FROM dbo.TacGia WHERE TenTacGia = N'Nguyễn Nhật Ánh')
    INSERT dbo.TacGia (TenTacGia) VALUES (N'Nguyễn Nhật Ánh');
IF NOT EXISTS (SELECT 1 FROM dbo.TacGia WHERE TenTacGia = N'Dale Carnegie')
    INSERT dbo.TacGia (TenTacGia) VALUES (N'Dale Carnegie');
IF NOT EXISTS (SELECT 1 FROM dbo.TacGia WHERE TenTacGia = N'Đỗ Như Tài')
    INSERT dbo.TacGia (TenTacGia) VALUES (N'Đỗ Như Tài');
IF NOT EXISTS (SELECT 1 FROM dbo.TacGia WHERE TenTacGia = N'Paulo Coelho')
    INSERT dbo.TacGia (TenTacGia) VALUES (N'Paulo Coelho');

IF COL_LENGTH(N'dbo.Sach', N'ISBN') IS NULL ALTER TABLE dbo.Sach ADD ISBN nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.Sach', N'TenSach') IS NULL ALTER TABLE dbo.Sach ADD TenSach nvarchar(300) NULL;
IF COL_LENGTH(N'dbo.Sach', N'MaTheLoai') IS NULL ALTER TABLE dbo.Sach ADD MaTheLoai int NULL;
IF COL_LENGTH(N'dbo.Sach', N'NamXB') IS NULL ALTER TABLE dbo.Sach ADD NamXB int NULL;
IF COL_LENGTH(N'dbo.Sach', N'NhaXB') IS NULL ALTER TABLE dbo.Sach ADD NhaXB nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Sach', N'SoLuong') IS NULL ALTER TABLE dbo.Sach ADD SoLuong int NULL;
IF COL_LENGTH(N'dbo.Sach', N'SoLuongCon') IS NULL ALTER TABLE dbo.Sach ADD SoLuongCon int NULL;
IF COL_LENGTH(N'dbo.Sach', N'MoTa') IS NULL ALTER TABLE dbo.Sach ADD MoTa nvarchar(4000) NULL;
IF COL_LENGTH(N'dbo.Sach', N'HinhAnh') IS NULL ALTER TABLE dbo.Sach ADD HinhAnh nvarchar(1000) NULL;
IF COL_LENGTH(N'dbo.Sach', N'IsDeleted') IS NULL ALTER TABLE dbo.Sach ADD IsDeleted bit NULL;
GO

DECLARE @DefaultCategoryId int = (SELECT MIN(MaTheLoai) FROM dbo.TheLoai WHERE IsDeleted = 0);
UPDATE dbo.Sach SET ISBN = CONCAT(N'LEGACY-', MaSach) WHERE ISBN IS NULL OR LTRIM(RTRIM(ISBN)) = N'';
UPDATE dbo.Sach SET TenSach = CONCAT(N'(Chưa nhập tên sách ', MaSach, N')') WHERE TenSach IS NULL OR LTRIM(RTRIM(TenSach)) = N'';
UPDATE dbo.Sach SET MaTheLoai = @DefaultCategoryId WHERE MaTheLoai IS NULL;
UPDATE dbo.Sach SET NamXB = 1900 WHERE NamXB IS NULL;
UPDATE dbo.Sach SET SoLuong = 0 WHERE SoLuong IS NULL;
UPDATE dbo.Sach SET SoLuongCon = SoLuong WHERE SoLuongCon IS NULL;
UPDATE dbo.Sach SET IsDeleted = 0 WHERE IsDeleted IS NULL;

ALTER TABLE dbo.Sach ALTER COLUMN ISBN nvarchar(20) NOT NULL;
ALTER TABLE dbo.Sach ALTER COLUMN TenSach nvarchar(300) NOT NULL;
ALTER TABLE dbo.Sach ALTER COLUMN MaTheLoai int NOT NULL;
ALTER TABLE dbo.Sach ALTER COLUMN NamXB int NOT NULL;
ALTER TABLE dbo.Sach ALTER COLUMN SoLuong int NOT NULL;
ALTER TABLE dbo.Sach ALTER COLUMN SoLuongCon int NOT NULL;
ALTER TABLE dbo.Sach ALTER COLUMN IsDeleted bit NOT NULL;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.Sach')
      AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.Sach'), N'IsDeleted', 'ColumnId')
)
    ALTER TABLE dbo.Sach ADD CONSTRAINT DF_Sach_IsDeleted DEFAULT (0) FOR IsDeleted;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Sach') AND name = N'UX_Sach_ISBN')
BEGIN
    IF EXISTS (SELECT ISBN FROM dbo.Sach GROUP BY ISBN HAVING COUNT(*) > 1)
        THROW 51001, 'Duplicate ISBN values exist; resolve them before creating UX_Sach_ISBN.', 1;
    CREATE UNIQUE INDEX UX_Sach_ISBN ON dbo.Sach(ISBN);
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sach_TheLoai')
    ALTER TABLE dbo.Sach ADD CONSTRAINT FK_Sach_TheLoai FOREIGN KEY (MaTheLoai) REFERENCES dbo.TheLoai(MaTheLoai);

IF OBJECT_ID(N'dbo.Sach_TacGia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sach_TacGia
    (
        MaSach int NOT NULL,
        MaTacGia int NOT NULL,
        CONSTRAINT PK_Sach_TacGia PRIMARY KEY (MaSach, MaTacGia),
        CONSTRAINT FK_Sach_TacGia_Sach FOREIGN KEY (MaSach) REFERENCES dbo.Sach(MaSach),
        CONSTRAINT FK_Sach_TacGia_TacGia FOREIGN KEY (MaTacGia) REFERENCES dbo.TacGia(MaTacGia)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TheLoai') AND name = N'UX_TheLoai_TenTheLoai')
    CREATE UNIQUE INDEX UX_TheLoai_TenTheLoai ON dbo.TheLoai(TenTheLoai);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TacGia') AND name = N'UX_TacGia_TenTacGia')
    CREATE UNIQUE INDEX UX_TacGia_TenTacGia ON dbo.TacGia(TenTacGia);

COMMIT TRANSACTION;


-- ===== Schema module: CatalogModuleSchema.sql =====

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.TheLoai', N'U') IS NULL
    THROW 51100, 'dbo.TheLoai must exist before applying the catalog module schema.', 1;

IF COL_LENGTH(N'dbo.TheLoai', N'MoTa') IS NULL
    ALTER TABLE dbo.TheLoai ADD MoTa nvarchar(2000) NULL;

COMMIT TRANSACTION;


-- ===== Schema module: ReaderModuleSchema.sql =====

-- Additive, idempotent schema for Module 3. Does not alter existing tables/data.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.DocGia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocGia
    (
        MaDocGia int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DocGia PRIMARY KEY,
        HoTen nvarchar(200) NOT NULL,
        NgaySinh date NOT NULL,
        GioiTinh nvarchar(20) NOT NULL,
        SoDienThoai nvarchar(11) NOT NULL,
        Email nvarchar(254) NULL,
        DiaChi nvarchar(500) NULL,
        NgayDangKy date NOT NULL CONSTRAINT DF_DocGia_NgayDangKy DEFAULT (CONVERT(date, GETDATE())),
        TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_DocGia_TrangThai DEFAULT (N'Hoạt động'),
        CONSTRAINT CK_DocGia_GioiTinh CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')),
        CONSTRAINT CK_DocGia_SoDienThoai CHECK
            (LEN(SoDienThoai) IN (10, 11) AND LEFT(SoDienThoai, 1) = N'0' AND SoDienThoai NOT LIKE N'%[^0-9]%'),
        CONSTRAINT CK_DocGia_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Khóa thẻ'))
    );

    CREATE UNIQUE INDEX UX_DocGia_Email
        ON dbo.DocGia(Email)
        WHERE Email IS NOT NULL;
    CREATE INDEX IX_DocGia_HoTen ON dbo.DocGia(HoTen);
    CREATE INDEX IX_DocGia_SoDienThoai ON dbo.DocGia(SoDienThoai);
    CREATE INDEX IX_DocGia_TrangThai ON dbo.DocGia(TrangThai);
END;

COMMIT TRANSACTION;
GO


-- ===== Schema module: EmployeeModuleSchema.sql =====

-- Module 5: additive schema for employee profiles. No login credentials or account roles.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVien (
        MaNhanVien int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
        HoTen nvarchar(200) NOT NULL,
        SoDienThoai nvarchar(11) NULL,
        Email nvarchar(254) NULL,
        ChucVu nvarchar(100) NOT NULL,
        TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_NhanVien_TrangThai DEFAULT N'Hoạt động',
        NgayTao datetime2 NOT NULL CONSTRAINT DF_NhanVien_NgayTao DEFAULT SYSDATETIME(),
        CONSTRAINT CK_NhanVien_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Ngừng hoạt động'))
    );

    CREATE UNIQUE INDEX UX_NhanVien_Email ON dbo.NhanVien(Email) WHERE Email IS NOT NULL;
    CREATE UNIQUE INDEX UX_NhanVien_SoDienThoai ON dbo.NhanVien(SoDienThoai) WHERE SoDienThoai IS NOT NULL;
    CREATE INDEX IX_NhanVien_HoTen ON dbo.NhanVien(HoTen);
    CREATE INDEX IX_NhanVien_TrangThai ON dbo.NhanVien(TrangThai);
END;

IF OBJECT_ID(N'dbo.NhanVien_TrangThaiLichSu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVien_TrangThaiLichSu (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien_TrangThaiLichSu PRIMARY KEY,
        MaNhanVien int NOT NULL,
        TrangThaiCu nvarchar(20) NOT NULL,
        TrangThaiMoi nvarchar(20) NOT NULL,
        ThayDoiLuc datetime2 NOT NULL CONSTRAINT DF_NhanVien_TrangThaiLichSu_ThayDoiLuc DEFAULT SYSDATETIME(),
        CONSTRAINT FK_NhanVien_TrangThaiLichSu_NhanVien FOREIGN KEY (MaNhanVien)
            REFERENCES dbo.NhanVien(MaNhanVien),
        CONSTRAINT CK_NhanVien_TrangThaiLichSu_Cu CHECK (TrangThaiCu IN (N'Hoạt động', N'Ngừng hoạt động')),
        CONSTRAINT CK_NhanVien_TrangThaiLichSu_Moi CHECK (TrangThaiMoi IN (N'Hoạt động', N'Ngừng hoạt động'))
    );
    CREATE INDEX IX_NhanVien_TrangThaiLichSu_MaNhanVien_ThayDoiLuc
        ON dbo.NhanVien_TrangThaiLichSu(MaNhanVien, ThayDoiLuc DESC);
END;

COMMIT TRANSACTION;


-- ===== Schema module: AuthenticationModuleSchema.sql =====

-- Module 6: additive account and revocable opaque-session storage.
-- Passwords use ASP.NET Core Identity hashes. Session bearer values are never stored here,
-- only their SHA-256 hashes.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoan (
        MaTaiKhoan int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaiKhoan PRIMARY KEY,
        TenDangNhap nvarchar(64) NOT NULL,
        TenDangNhapChuanHoa nvarchar(64) NOT NULL,
        MatKhauHash nvarchar(512) NOT NULL,
        TenHienThi nvarchar(200) NOT NULL,
        VaiTro nvarchar(20) NOT NULL,
        TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT N'Hoạt động',
        MaNhanVien int NULL,
        MaDocGia int NULL,
        SoLanDangNhapThatBai int NOT NULL CONSTRAINT DF_TaiKhoan_SoLanDangNhapThatBai DEFAULT 0,
        KhoaDenUtc datetime2 NULL,
        NgayTaoUtc datetime2 NOT NULL CONSTRAINT DF_TaiKhoan_NgayTaoUtc DEFAULT SYSUTCDATETIME(),
        CapNhatLucUtc datetime2 NULL,
        CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.NhanVien(MaNhanVien),
        CONSTRAINT FK_TaiKhoan_DocGia FOREIGN KEY (MaDocGia) REFERENCES dbo.DocGia(MaDocGia),
        CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN (N'ADMIN', N'THU_THU', N'DOC_GIA')),
        CONSTRAINT CK_TaiKhoan_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Bị khóa')),
        CONSTRAINT CK_TaiKhoan_LienKet CHECK (
            (VaiTro = N'ADMIN' AND MaDocGia IS NULL)
            OR (VaiTro = N'THU_THU' AND MaNhanVien IS NOT NULL AND MaDocGia IS NULL)
            OR (VaiTro = N'DOC_GIA' AND MaDocGia IS NOT NULL AND MaNhanVien IS NULL)
        ),
        CONSTRAINT CK_TaiKhoan_SoLanDangNhapThatBai CHECK (SoLanDangNhapThatBai >= 0)
    );
    CREATE UNIQUE INDEX UX_TaiKhoan_TenDangNhapChuanHoa ON dbo.TaiKhoan(TenDangNhapChuanHoa);
    CREATE UNIQUE INDEX UX_TaiKhoan_MaNhanVien ON dbo.TaiKhoan(MaNhanVien) WHERE MaNhanVien IS NOT NULL;
    CREATE UNIQUE INDEX UX_TaiKhoan_MaDocGia ON dbo.TaiKhoan(MaDocGia) WHERE MaDocGia IS NOT NULL;
    CREATE INDEX IX_TaiKhoan_VaiTro_TrangThai ON dbo.TaiKhoan(VaiTro, TrangThai);
END;

IF OBJECT_ID(N'dbo.TaiKhoan_Session', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoan_Session (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_TaiKhoan_Session PRIMARY KEY,
        MaTaiKhoan int NOT NULL,
        TokenHash binary(32) NOT NULL,
        TaoLucUtc datetime2 NOT NULL,
        HetHanLucUtc datetime2 NOT NULL,
        ThuHoiLucUtc datetime2 NULL,
        CONSTRAINT FK_TaiKhoan_Session_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES dbo.TaiKhoan(MaTaiKhoan)
    );
    CREATE UNIQUE INDEX UX_TaiKhoan_Session_TokenHash ON dbo.TaiKhoan_Session(TokenHash);
    CREATE INDEX IX_TaiKhoan_Session_Account_Expiry ON dbo.TaiKhoan_Session(MaTaiKhoan, HetHanLucUtc);
END;

COMMIT TRANSACTION;


-- ===== Schema module: BorrowingModuleSchema.sql =====

-- Module 4: additive schema only. Review and back up the target database before applying.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.PhieuMuon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhieuMuon (
        MaPhieu int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhieuMuon PRIMARY KEY,
        MaDocGia int NOT NULL,
        NguoiLap nvarchar(100) NULL,
        NgayMuon date NOT NULL,
        HanTra date NOT NULL,
        IdempotencyKey uniqueidentifier NOT NULL,
        CONSTRAINT FK_PhieuMuon_DocGia FOREIGN KEY (MaDocGia) REFERENCES dbo.DocGia(MaDocGia),
        CONSTRAINT CK_PhieuMuon_Ngay CHECK (HanTra >= NgayMuon)
    );
    CREATE UNIQUE INDEX UX_PhieuMuon_IdempotencyKey ON dbo.PhieuMuon(IdempotencyKey);
END;

IF OBJECT_ID(N'dbo.ChiTiet_PhieuMuon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTiet_PhieuMuon (
        MaChiTiet int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChiTiet_PhieuMuon PRIMARY KEY,
        MaPhieu int NOT NULL,
        MaSach int NOT NULL,
        SoLuongMuon int NOT NULL,
        SoLuongDaXuLy int NOT NULL CONSTRAINT DF_ChiTiet_PhieuMuon_DaXuLy DEFAULT(0),
        CONSTRAINT FK_ChiTiet_PhieuMuon_Phieu FOREIGN KEY (MaPhieu) REFERENCES dbo.PhieuMuon(MaPhieu),
        CONSTRAINT FK_ChiTiet_PhieuMuon_Sach FOREIGN KEY (MaSach) REFERENCES dbo.Sach(MaSach),
        CONSTRAINT CK_ChiTiet_PhieuMuon_SoLuong CHECK (SoLuongMuon > 0 AND SoLuongDaXuLy >= 0 AND SoLuongDaXuLy <= SoLuongMuon),
        CONSTRAINT UQ_ChiTiet_PhieuMuon_PhieuSach UNIQUE (MaPhieu, MaSach)
    );
END;

IF OBJECT_ID(N'dbo.ChiTietTraSach', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietTraSach (
        MaLanTra int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChiTietTraSach PRIMARY KEY,
        MaChiTiet int NOT NULL,
        SoLuongXuLy int NOT NULL,
        NgayTra date NOT NULL,
        TinhTrangSach nvarchar(40) NOT NULL,
        SoNgayTre int NOT NULL,
        MucPhatMoiNgayApDung decimal(18,2) NULL,
        SoTienPhatPhaiThu decimal(18,2) NULL,
        IdempotencyKey uniqueidentifier NOT NULL,
        CONSTRAINT FK_ChiTietTraSach_ChiTiet FOREIGN KEY (MaChiTiet) REFERENCES dbo.ChiTiet_PhieuMuon(MaChiTiet),
        CONSTRAINT CK_ChiTietTraSach_SoLuong CHECK (SoLuongXuLy > 0 AND SoNgayTre >= 0),
        CONSTRAINT CK_ChiTietTraSach_Tien CHECK ((MucPhatMoiNgayApDung IS NULL OR MucPhatMoiNgayApDung >= 0) AND (SoTienPhatPhaiThu IS NULL OR SoTienPhatPhaiThu >= 0))
    );
    CREATE UNIQUE INDEX UX_ChiTietTraSach_IdempotencyKey ON dbo.ChiTietTraSach(IdempotencyKey);
END;

IF OBJECT_ID(N'dbo.ThuTienPhat', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ThuTienPhat (
        MaKhoanThu int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThuTienPhat PRIMARY KEY,
        MaLanTra int NOT NULL,
        SoTien decimal(18,2) NOT NULL,
        NgayThu datetime2 NOT NULL,
        NguoiThu nvarchar(100) NULL,
        IdempotencyKey uniqueidentifier NOT NULL,
        CONSTRAINT FK_ThuTienPhat_LanTra FOREIGN KEY (MaLanTra) REFERENCES dbo.ChiTietTraSach(MaLanTra),
        CONSTRAINT CK_ThuTienPhat_SoTien CHECK (SoTien > 0)
    );
    CREATE UNIQUE INDEX UX_ThuTienPhat_IdempotencyKey ON dbo.ThuTienPhat(IdempotencyKey);
END;

IF OBJECT_ID(N'dbo.CauHinhTienPhat', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CauHinhTienPhat (
        Id tinyint NOT NULL CONSTRAINT PK_CauHinhTienPhat PRIMARY KEY,
        MucPhatMoiNgay decimal(18,2) NOT NULL,
        CapNhatLuc datetime2 NOT NULL,
        CONSTRAINT CK_CauHinhTienPhat_Id CHECK (Id = 1),
        CONSTRAINT CK_CauHinhTienPhat_Muc CHECK (MucPhatMoiNgay > 0)
    );
END;

COMMIT TRANSACTION;


-- ===== Schema module: BorrowRequestModuleSchema.sql =====
-- Module 7: yêu cầu mượn sách do độc giả gửi từ Web. Chạy sau module Borrowing.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.YeuCauMuon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.YeuCauMuon (
        MaYeuCau int IDENTITY(1,1) NOT NULL CONSTRAINT PK_YeuCauMuon PRIMARY KEY,
        MaDocGia int NOT NULL,
        MaSach int NOT NULL,
        SoLuong int NOT NULL,
        NgayTao datetime2 NOT NULL CONSTRAINT DF_YeuCauMuon_NgayTao DEFAULT(SYSDATETIME()),
        TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_YeuCauMuon_TrangThai DEFAULT(N'Chờ duyệt'),
        GhiChu nvarchar(500) NULL,
        LyDoTuChoi nvarchar(500) NULL,
        NguoiDuyet nvarchar(100) NULL,
        NgayDuyet datetime2 NULL,
        MaPhieu int NULL,
        IdempotencyKey uniqueidentifier NOT NULL,
        CONSTRAINT FK_YeuCauMuon_DocGia FOREIGN KEY (MaDocGia) REFERENCES dbo.DocGia(MaDocGia),
        CONSTRAINT FK_YeuCauMuon_Sach FOREIGN KEY (MaSach) REFERENCES dbo.Sach(MaSach),
        CONSTRAINT FK_YeuCauMuon_PhieuMuon FOREIGN KEY (MaPhieu) REFERENCES dbo.PhieuMuon(MaPhieu),
        CONSTRAINT CK_YeuCauMuon_SoLuong CHECK (SoLuong > 0),
        CONSTRAINT CK_YeuCauMuon_TrangThai CHECK (TrangThai IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối', N'Đã hủy')),
        CONSTRAINT CK_YeuCauMuon_MaPhieu CHECK (TrangThai <> N'Đã duyệt' OR MaPhieu IS NOT NULL)
    );
    CREATE UNIQUE INDEX UX_YeuCauMuon_IdempotencyKey ON dbo.YeuCauMuon(IdempotencyKey);
    CREATE INDEX IX_YeuCauMuon_DocGia_TrangThai ON dbo.YeuCauMuon(MaDocGia, TrangThai);
    CREATE INDEX IX_YeuCauMuon_TrangThai_NgayTao ON dbo.YeuCauMuon(TrangThai, NgayTao);
    CREATE UNIQUE INDEX UX_YeuCauMuon_DangCho
        ON dbo.YeuCauMuon(MaDocGia, MaSach)
        WHERE TrangThai = N'Chờ duyệt';
END;

COMMIT TRANSACTION;


-- ===== Schema module: AccountLockRequestModuleSchema.sql =====
-- Module 8: yêu cầu khóa tài khoản độc giả do thủ thư gửi, Admin duyệt.
-- Chạy sau module Authentication (cần TaiKhoan, NhanVien).
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.YeuCauKhoaTaiKhoan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.YeuCauKhoaTaiKhoan (
        MaYeuCau int IDENTITY(1,1) NOT NULL CONSTRAINT PK_YeuCauKhoaTaiKhoan PRIMARY KEY,
        MaTaiKhoan int NOT NULL,
        MaNhanVien int NOT NULL,
        LyDo nvarchar(500) NOT NULL,
        ThoiHanKhoaNgay int NULL,
        TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_YeuCauKhoaTaiKhoan_TrangThai DEFAULT(N'Chờ duyệt'),
        NguoiDuyet nvarchar(100) NULL,
        NgayTao datetime2 NOT NULL CONSTRAINT DF_YeuCauKhoaTaiKhoan_NgayTao DEFAULT(SYSDATETIME()),
        NgayDuyet datetime2 NULL,
        LyDoTuChoi nvarchar(500) NULL,
        IdempotencyKey uniqueidentifier NOT NULL,
        CONSTRAINT FK_YeuCauKhoaTaiKhoan_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES dbo.TaiKhoan(MaTaiKhoan),
        CONSTRAINT FK_YeuCauKhoaTaiKhoan_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.NhanVien(MaNhanVien),
        CONSTRAINT CK_YeuCauKhoaTaiKhoan_TrangThai CHECK (TrangThai IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối', N'Đã hủy')),
        CONSTRAINT CK_YeuCauKhoaTaiKhoan_ThoiHan CHECK (ThoiHanKhoaNgay IS NULL OR ThoiHanKhoaNgay BETWEEN 1 AND 365)
    );
    CREATE UNIQUE INDEX UX_YeuCauKhoaTaiKhoan_IdempotencyKey ON dbo.YeuCauKhoaTaiKhoan(IdempotencyKey);
    CREATE INDEX IX_YeuCauKhoaTaiKhoan_NhanVien_TrangThai ON dbo.YeuCauKhoaTaiKhoan(MaNhanVien, TrangThai);
    CREATE INDEX IX_YeuCauKhoaTaiKhoan_TrangThai_NgayTao ON dbo.YeuCauKhoaTaiKhoan(TrangThai, NgayTao);
    -- Mỗi tài khoản chỉ có một yêu cầu khóa đang chờ duyệt.
    CREATE UNIQUE INDEX UX_YeuCauKhoaTaiKhoan_DangCho
        ON dbo.YeuCauKhoaTaiKhoan(MaTaiKhoan)
        WHERE TrangThai = N'Chờ duyệt';
END;

COMMIT TRANSACTION;


-- ===== Minimal sample dataset =====
-- All DEMO rows below are synthetic. ISBNs are checksum-valid examples, not publication claims.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'DEMO - Khoa học máy tính')
    INSERT dbo.TheLoai (TenTheLoai, MoTa) VALUES (N'DEMO - Khoa học máy tính', N'Dữ liệu giả lập để thử nghiệm ứng dụng.');
IF NOT EXISTS (SELECT 1 FROM dbo.TacGia WHERE TenTacGia = N'DEMO - Tác giả mẫu 01')
    INSERT dbo.TacGia (TenTacGia, QuocTich, GioiThieu) VALUES (N'DEMO - Tác giả mẫu 01', N'Việt Nam', N'Nhân vật giả lập.');

IF NOT EXISTS (SELECT 1 FROM dbo.Sach WHERE ISBN = N'9786040000019')
BEGIN
    INSERT dbo.Sach (ISBN, TenSach, MaTheLoai, NamXB, NhaXB, SoLuong, SoLuongCon, MoTa, IsDeleted)
    SELECT N'9786040000019', N'DEMO - Nhập môn cơ sở dữ liệu', t.MaTheLoai, 2024,
           N'Nhà xuất bản Demo', 5, 5,
           N'Sách giả lập cho kiểm thử; không đại diện ấn phẩm đã phát hành.', 0
    FROM dbo.TheLoai AS t WHERE t.TenTheLoai = N'DEMO - Khoa học máy tính';
END;

INSERT dbo.Sach_TacGia (MaSach, MaTacGia)
SELECT s.MaSach, a.MaTacGia
FROM dbo.Sach AS s
CROSS JOIN dbo.TacGia AS a
WHERE s.ISBN = N'9786040000019'
  AND a.TenTacGia = N'DEMO - Tác giả mẫu 01'
  AND NOT EXISTS (SELECT 1 FROM dbo.Sach_TacGia AS sa WHERE sa.MaSach = s.MaSach AND sa.MaTacGia = a.MaTacGia);

IF NOT EXISTS (SELECT 1 FROM dbo.DocGia WHERE Email = N'demo.reader@example.invalid')
    INSERT dbo.DocGia (HoTen, NgaySinh, GioiTinh, SoDienThoai, Email, DiaChi, NgayDangKy, TrangThai)
    VALUES (N'DEMO - Độc giả mẫu 01', '2000-01-01', N'Khác', N'0900000001', N'demo.reader@example.invalid', N'Địa chỉ giả lập', CONVERT(date, GETDATE()), N'Hoạt động');

IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE Email = N'demo.staff@example.invalid')
    INSERT dbo.NhanVien (HoTen, SoDienThoai, Email, ChucVu, TrangThai)
    VALUES (N'DEMO - Nhân viên mẫu 01', NULL, N'demo.staff@example.invalid', N'Thủ thư', N'Hoạt động');

-- Demo-only fallback rate. Existing fine configuration is preserved.
IF NOT EXISTS (SELECT 1 FROM dbo.CauHinhTienPhat WHERE Id = 1)
    INSERT dbo.CauHinhTienPhat (Id, MucPhatMoiNgay, CapNhatLuc) VALUES (1, 1000.00, SYSDATETIME());

-- One active overdue loan. Inventory is decremented exactly once by this guarded insert.
IF NOT EXISTS (SELECT 1 FROM dbo.PhieuMuon WHERE IdempotencyKey = '71000000-0000-4000-8000-000000000001')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Sach WHERE ISBN = N'9786040000019' AND IsDeleted = 0 AND SoLuongCon > 0)
        THROW 52001, 'Demo book is missing, hidden, or has no available copies.', 1;

    INSERT dbo.PhieuMuon (MaDocGia, NguoiLap, NgayMuon, HanTra, IdempotencyKey)
    SELECT d.MaDocGia, N'DEMO - Nhân viên mẫu 01', DATEADD(DAY, -15, CONVERT(date, GETDATE())),
           DATEADD(DAY, -5, CONVERT(date, GETDATE())), '71000000-0000-4000-8000-000000000001'
    FROM dbo.DocGia AS d WHERE d.Email = N'demo.reader@example.invalid';

    DECLARE @MaPhieu int = CONVERT(int, SCOPE_IDENTITY());
    DECLARE @MaSach int = (SELECT MaSach FROM dbo.Sach WHERE ISBN = N'9786040000019');
    INSERT dbo.ChiTiet_PhieuMuon (MaPhieu, MaSach, SoLuongMuon, SoLuongDaXuLy)
    VALUES (@MaPhieu, @MaSach, 1, 0);
    UPDATE dbo.Sach SET SoLuongCon = SoLuongCon - 1 WHERE MaSach = @MaSach;
END;

-- ===== Extended DEMO catalog for reader-facing testing =====
-- Every row is synthetic and guarded by ISBN/name so the script can be re-run.
-- Each category below has 5 visible titles with copy counts 1, 2, 3, 4 and 5.
-- Also covers: one copy left, out of stock, multiple authors, hidden book.
IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật phần mềm')
    INSERT dbo.TheLoai (TenTheLoai, MoTa) VALUES (N'Kỹ thuật phần mềm', N'Thể loại giả lập để thử nghiệm.');
IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Thiếu nhi')
    INSERT dbo.TheLoai (TenTheLoai, MoTa) VALUES (N'Thiếu nhi', N'Thể loại giả lập để thử nghiệm.');
IF NOT EXISTS (SELECT 1 FROM dbo.TheLoai WHERE TenTheLoai = N'Lịch sử - Địa lý')
    INSERT dbo.TheLoai (TenTheLoai, MoTa) VALUES (N'Lịch sử - Địa lý', N'Thể loại giả lập để thử nghiệm.');

DECLARE @DemoTacGiaMoi TABLE
(
    TenTacGia nvarchar(200) NOT NULL PRIMARY KEY,
    QuocTich nvarchar(100) NULL,
    GioiThieu nvarchar(2000) NULL
);
INSERT @DemoTacGiaMoi (TenTacGia, QuocTich, GioiThieu) VALUES
    (N'DEMO - Nguyễn Văn Lập', N'Việt Nam', N'Nhân vật giả lập.'),
    (N'DEMO - Đặng Quốc Bảo', N'Việt Nam', N'Nhân vật giả lập.'),
    (N'DEMO - Trần Thị Minh', N'Việt Nam', N'Nhân vật giả lập.'),
    (N'DEMO - Lê Hoàng Nam', N'Việt Nam', N'Nhân vật giả lập.'),
    (N'DEMO - Vũ Minh Châu', N'Việt Nam', N'Nhân vật giả lập.'),
    (N'DEMO - Phạm Thu Hà', N'Việt Nam', N'Nhân vật giả lập.'),
    (N'DEMO - Hoàng Anh Tuấn', N'Việt Nam', N'Nhân vật giả lập.');

INSERT dbo.TacGia (TenTacGia, QuocTich, GioiThieu)
SELECT d.TenTacGia, d.QuocTich, d.GioiThieu
FROM @DemoTacGiaMoi AS d
WHERE NOT EXISTS (SELECT 1 FROM dbo.TacGia AS t WHERE t.TenTacGia = d.TenTacGia);

DECLARE @DemoSachMoi TABLE
(
    ISBN nvarchar(20) NOT NULL PRIMARY KEY,
    TenSach nvarchar(300) NOT NULL,
    TenTheLoai nvarchar(150) NOT NULL,
    NamXB int NOT NULL,
    NhaXB nvarchar(200) NOT NULL,
    SoLuong int NOT NULL,
    SoLuongCon int NOT NULL,
    MoTa nvarchar(4000) NULL,
    IsDeleted bit NOT NULL
);
INSERT @DemoSachMoi (ISBN, TenSach, TenTheLoai, NamXB, NhaXB, SoLuong, SoLuongCon, MoTa, IsDeleted) VALUES
    (N'9786042000017', N'DEMO - Lập trình C# từ cơ bản đến nâng cao', N'Công nghệ thông tin', 2023, N'Nhà xuất bản Demo', 5, 5, N'Sách giả lập: còn nhiều bản.', 0),
    (N'9786042000024', N'DEMO - Cấu trúc dữ liệu và giải thuật', N'Công nghệ thông tin', 2022, N'Nhà xuất bản Demo', 3, 3, N'Sách giả lập: hai tác giả.', 0),
    (N'9786042000031', N'DEMO - Kiến trúc phần mềm thực chiến', N'Kỹ thuật phần mềm', 2024, N'Nhà xuất bản Demo', 2, 1, N'Sách giả lập: chỉ còn 1 bản.', 0),
    (N'9786042000048', N'DEMO - Kiểm thử tự động với .NET', N'Kỹ thuật phần mềm', 2024, N'Nhà xuất bản Demo', 4, 0, N'Sách giả lập: đã hết bản, vẫn cho đặt trước.', 0),
    (N'9786042000055', N'DEMO - Truyện ngắn chọn lọc', N'Văn học kinh điển', 2020, N'Nhà xuất bản Demo', 3, 2, N'Sách giả lập: thể loại văn học.', 0),
    (N'9786042000062', N'DEMO - Thơ tình thế kỷ XX', N'Văn học kinh điển', 2019, N'Nhà xuất bản Demo', 2, 2, N'Sách giả lập: thể loại văn học.', 0),
    (N'9786042000079', N'DEMO - Quản trị tài chính cá nhân', N'Kinh tế - Quản trị', 2023, N'Nhà xuất bản Demo', 5, 4, N'Sách giả lập: thể loại kinh tế.', 0),
    (N'9786042000086', N'DEMO - Khởi nghiệp tinh gọn', N'Kinh tế - Quản trị', 2021, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: chỉ có 1 bản.', 0),
    (N'9786042000093', N'DEMO - Kỹ năng giao tiếp hiệu quả', N'Kỹ năng sống', 2022, N'Nhà xuất bản Demo', 5, 5, N'Sách giả lập: thể loại kỹ năng sống.', 0),
    (N'9786042000109', N'DEMO - Quản lý thời gian', N'Kỹ năng sống', 2020, N'Nhà xuất bản Demo', 3, 3, N'Sách giả lập: thể loại kỹ năng sống.', 0),
    (N'9786042000116', N'DEMO - Cuộc phiêu lưu của chú mèo nhỏ', N'Thiếu nhi', 2023, N'Nhà xuất bản Demo', 4, 4, N'Sách giả lập: thể loại thiếu nhi.', 0),
    (N'9786042000123', N'DEMO - Lịch sử Việt Nam bằng tranh', N'Lịch sử - Địa lý', 2022, N'Nhà xuất bản Demo', 3, 3, N'Sách giả lập: thể loại lịch sử - địa lý.', 0),
    (N'9786042000130', N'DEMO - Sách đã ẩn (kiểm thử lọc)', N'Công nghệ thông tin', 2018, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập đã ẩn để kiểm thử lọc danh sách.', 1),
    (N'9786042000147', N'DEMO - Lập trình Python cho người mới', N'Công nghệ thông tin', 2024, N'Nhà xuất bản Demo', 2, 2, N'Sách giả lập: nhập môn lập trình Python.', 0),
    (N'9786042000154', N'DEMO - Cơ sở dữ liệu nâng cao', N'Công nghệ thông tin', 2023, N'Nhà xuất bản Demo', 4, 4, N'Sách giả lập: tối ưu truy vấn và thiết kế dữ liệu.', 0),
    (N'9786042000161', N'DEMO - Trí tuệ nhân tạo nhập môn', N'Công nghệ thông tin', 2024, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: khái niệm cơ bản về AI.', 0),
    (N'9786042000178', N'DEMO - Mùa hè không tên', N'Văn học kinh điển', 2021, N'Nhà xuất bản Demo', 4, 4, N'Sách giả lập: tiểu thuyết thanh xuân.', 0),
    (N'9786042000185', N'DEMO - Tuyển tập truyện ngắn Việt Nam', N'Văn học kinh điển', 2020, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: tuyển tập truyện ngắn.', 0),
    (N'9786042000192', N'DEMO - Thơ cho tuổi trẻ', N'Văn học kinh điển', 2022, N'Nhà xuất bản Demo', 5, 5, N'Sách giả lập: thơ dành cho bạn trẻ.', 0),
    (N'9786042000208', N'DEMO - Kinh tế học vi mô căn bản', N'Kinh tế - Quản trị', 2023, N'Nhà xuất bản Demo', 3, 3, N'Sách giả lập: kiến thức kinh tế vi mô.', 0),
    (N'9786042000215', N'DEMO - Quản trị dự án hiện đại', N'Kinh tế - Quản trị', 2024, N'Nhà xuất bản Demo', 2, 2, N'Sách giả lập: quản trị dự án theo Agile.', 0),
    (N'9786042000222', N'DEMO - Tài chính doanh nghiệp', N'Kinh tế - Quản trị', 2022, N'Nhà xuất bản Demo', 4, 4, N'Sách giả lập: quản trị tài chính doanh nghiệp.', 0),
    (N'9786042000239', N'DEMO - Sống tập trung', N'Kỹ năng sống', 2023, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: rèn luyện sự tập trung.', 0),
    (N'9786042000246', N'DEMO - Giao tiếp trong công việc', N'Kỹ năng sống', 2024, N'Nhà xuất bản Demo', 2, 2, N'Sách giả lập: kỹ năng giao tiếp nơi làm việc.', 0),
    (N'9786042000253', N'DEMO - Rèn luyện thói quen tốt', N'Kỹ năng sống', 2021, N'Nhà xuất bản Demo', 4, 4, N'Sách giả lập: xây dựng thói quen tích cực.', 0),
    (N'9786042000260', N'DEMO - DevOps thực hành', N'Kỹ thuật phần mềm', 2024, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: quy trình DevOps thực tế.', 0),
    (N'9786042000277', N'DEMO - Thiết kế hệ thống phân tán', N'Kỹ thuật phần mềm', 2023, N'Nhà xuất bản Demo', 5, 5, N'Sách giả lập: thiết kế hệ thống phân tán.', 0),
    (N'9786042000284', N'DEMO - Bộ sưu tập truyện cổ tích', N'Thiếu nhi', 2022, N'Nhà xuất bản Demo', 5, 5, N'Sách giả lập: truyện cổ tích cho thiếu nhi.', 0),
    (N'9786042000291', N'DEMO - Khám phá vũ trụ cho trẻ em', N'Thiếu nhi', 2024, N'Nhà xuất bản Demo', 2, 2, N'Sách giả lập: kiến thức vũ trụ cho trẻ em.', 0),
    (N'9786042000307', N'DEMO - Chú gấu con học chữ', N'Thiếu nhi', 2021, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: truyện tranh tập đọc.', 0),
    (N'9786042000314', N'DEMO - Những câu đố vui cho bé', N'Thiếu nhi', 2023, N'Nhà xuất bản Demo', 3, 3, N'Sách giả lập: câu đố vui và trò chơi.', 0),
    (N'9786042000321', N'DEMO - Lịch sử thế giới cận đại', N'Lịch sử - Địa lý', 2022, N'Nhà xuất bản Demo', 4, 4, N'Sách giả lập: lịch sử thế giới cận đại.', 0),
    (N'9786042000338', N'DEMO - Địa lý Việt Nam qua bản đồ', N'Lịch sử - Địa lý', 2024, N'Nhà xuất bản Demo', 5, 5, N'Sách giả lập: địa lý Việt Nam trực quan.', 0),
    (N'9786042000345', N'DEMO - Danh nhân lịch sử Việt Nam', N'Lịch sử - Địa lý', 2020, N'Nhà xuất bản Demo', 1, 1, N'Sách giả lập: chân dung danh nhân lịch sử.', 0),
    (N'9786042000352', N'DEMO - Các nền văn minh cổ đại', N'Lịch sử - Địa lý', 2023, N'Nhà xuất bản Demo', 2, 2, N'Sách giả lập: khảo cứu các nền văn minh cổ đại.', 0);

INSERT dbo.Sach (ISBN, TenSach, MaTheLoai, NamXB, NhaXB, SoLuong, SoLuongCon, MoTa, IsDeleted)
SELECT d.ISBN, d.TenSach, t.MaTheLoai, d.NamXB, d.NhaXB, d.SoLuong, d.SoLuongCon, d.MoTa, d.IsDeleted
FROM @DemoSachMoi AS d
INNER JOIN dbo.TheLoai AS t ON t.TenTheLoai = d.TenTheLoai
WHERE NOT EXISTS (SELECT 1 FROM dbo.Sach AS s WHERE s.ISBN = d.ISBN);

IF EXISTS
(
    SELECT 1 FROM @DemoSachMoi AS d
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Sach AS s WHERE s.ISBN = d.ISBN)
)
    THROW 52002, 'Some DEMO catalog rows were not inserted; check categories and ISBN data.', 1;

DECLARE @DemoSachTacGia TABLE
(
    ISBN nvarchar(20) NOT NULL,
    TenTacGia nvarchar(200) NOT NULL,
    PRIMARY KEY (ISBN, TenTacGia)
);
INSERT @DemoSachTacGia (ISBN, TenTacGia) VALUES
    (N'9786042000017', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000024', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000024', N'DEMO - Đặng Quốc Bảo'),
    (N'9786042000031', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000048', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000048', N'DEMO - Đặng Quốc Bảo'),
    (N'9786042000055', N'DEMO - Trần Thị Minh'),
    (N'9786042000062', N'DEMO - Trần Thị Minh'),
    (N'9786042000079', N'DEMO - Lê Hoàng Nam'),
    (N'9786042000086', N'DEMO - Lê Hoàng Nam'),
    (N'9786042000093', N'DEMO - Vũ Minh Châu'),
    (N'9786042000109', N'DEMO - Vũ Minh Châu'),
    (N'9786042000116', N'DEMO - Phạm Thu Hà'),
    (N'9786042000123', N'DEMO - Hoàng Anh Tuấn'),
    (N'9786042000123', N'DEMO - Trần Thị Minh'),
    (N'9786042000130', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000147', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000154', N'DEMO - Đặng Quốc Bảo'),
    (N'9786042000161', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000161', N'DEMO - Đặng Quốc Bảo'),
    (N'9786042000178', N'DEMO - Trần Thị Minh'),
    (N'9786042000185', N'DEMO - Trần Thị Minh'),
    (N'9786042000185', N'DEMO - Phạm Thu Hà'),
    (N'9786042000192', N'DEMO - Phạm Thu Hà'),
    (N'9786042000208', N'DEMO - Lê Hoàng Nam'),
    (N'9786042000215', N'DEMO - Lê Hoàng Nam'),
    (N'9786042000215', N'DEMO - Hoàng Anh Tuấn'),
    (N'9786042000222', N'DEMO - Vũ Minh Châu'),
    (N'9786042000239', N'DEMO - Vũ Minh Châu'),
    (N'9786042000246', N'DEMO - Phạm Thu Hà'),
    (N'9786042000253', N'DEMO - Hoàng Anh Tuấn'),
    (N'9786042000260', N'DEMO - Nguyễn Văn Lập'),
    (N'9786042000260', N'DEMO - Hoàng Anh Tuấn'),
    (N'9786042000277', N'DEMO - Đặng Quốc Bảo'),
    (N'9786042000284', N'DEMO - Phạm Thu Hà'),
    (N'9786042000291', N'DEMO - Hoàng Anh Tuấn'),
    (N'9786042000307', N'DEMO - Vũ Minh Châu'),
    (N'9786042000314', N'DEMO - Trần Thị Minh'),
    (N'9786042000321', N'DEMO - Lê Hoàng Nam'),
    (N'9786042000338', N'DEMO - Hoàng Anh Tuấn'),
    (N'9786042000345', N'DEMO - Trần Thị Minh'),
    (N'9786042000352', N'DEMO - Đặng Quốc Bảo');

INSERT dbo.Sach_TacGia (MaSach, MaTacGia)
SELECT s.MaSach, a.MaTacGia
FROM @DemoSachTacGia AS d
INNER JOIN dbo.Sach AS s ON s.ISBN = d.ISBN
INNER JOIN dbo.TacGia AS a ON a.TenTacGia = d.TenTacGia
WHERE NOT EXISTS (SELECT 1 FROM dbo.Sach_TacGia AS sa WHERE sa.MaSach = s.MaSach AND sa.MaTacGia = a.MaTacGia);

COMMIT TRANSACTION;
GO

-- Verify sample rows. No credentials are created by this script.
SELECT MaTheLoai, TenTheLoai FROM dbo.TheLoai WHERE TenTheLoai LIKE N'DEMO -%';
SELECT MaTacGia, TenTacGia FROM dbo.TacGia WHERE TenTacGia LIKE N'DEMO -%';
SELECT MaSach, ISBN, TenSach, SoLuong, SoLuongCon, IsDeleted FROM dbo.Sach WHERE ISBN = N'9786040000019';
SELECT MaDocGia, HoTen, Email FROM dbo.DocGia WHERE Email = N'demo.reader@example.invalid';
SELECT MaPhieu, MaDocGia, NgayMuon, HanTra FROM dbo.PhieuMuon WHERE IdempotencyKey = '71000000-0000-4000-8000-000000000001';
SELECT s.MaSach, s.ISBN, s.TenSach, t.TenTheLoai, s.SoLuong, s.SoLuongCon, s.IsDeleted
FROM dbo.Sach AS s INNER JOIN dbo.TheLoai AS t ON t.MaTheLoai = s.MaTheLoai
WHERE s.ISBN LIKE N'9786042000%' ORDER BY s.ISBN;
SELECT s.ISBN, s.TenSach, a.TenTacGia
FROM dbo.Sach AS s
INNER JOIN dbo.Sach_TacGia AS sa ON sa.MaSach = s.MaSach
INNER JOIN dbo.TacGia AS a ON a.MaTacGia = sa.MaTacGia
WHERE s.ISBN LIKE N'9786042000%' ORDER BY s.ISBN, a.TenTacGia;
