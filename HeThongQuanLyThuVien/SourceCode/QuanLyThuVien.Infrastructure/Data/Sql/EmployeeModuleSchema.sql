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
