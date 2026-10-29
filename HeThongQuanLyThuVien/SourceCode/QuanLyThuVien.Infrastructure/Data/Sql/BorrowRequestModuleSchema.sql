-- Module 7: yêu cầu mượn sách do độc giả gửi từ Web.
-- Additive, idempotent. Chạy SAU BorrowingModuleSchema.sql (cần bảng PhieuMuon).
-- Review and back up the target database before applying.
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
    -- Mỗi độc giả chỉ có một yêu cầu đang chờ duyệt cho mỗi đầu sách.
    CREATE UNIQUE INDEX UX_YeuCauMuon_DangCho
        ON dbo.YeuCauMuon(MaDocGia, MaSach)
        WHERE TrangThai = N'Chờ duyệt';
END;

COMMIT TRANSACTION;
