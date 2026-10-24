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
