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
