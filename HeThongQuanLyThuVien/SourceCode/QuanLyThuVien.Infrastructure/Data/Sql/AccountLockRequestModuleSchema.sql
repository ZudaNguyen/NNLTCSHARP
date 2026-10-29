-- Module 8: yêu cầu khóa tài khoản độc giả do thủ thư gửi, Admin duyệt.
-- Additive, idempotent. Chạy SAU AuthenticationModuleSchema.sql (cần TaiKhoan, NhanVien).
-- Review and back up the target database before applying.
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
