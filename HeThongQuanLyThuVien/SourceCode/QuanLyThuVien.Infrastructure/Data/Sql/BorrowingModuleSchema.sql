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
