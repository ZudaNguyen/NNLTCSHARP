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
