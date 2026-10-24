SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.TheLoai', N'U') IS NULL
    THROW 51100, 'dbo.TheLoai must exist before applying the catalog module schema.', 1;

IF COL_LENGTH(N'dbo.TheLoai', N'MoTa') IS NULL
    ALTER TABLE dbo.TheLoai ADD MoTa nvarchar(2000) NULL;

COMMIT TRANSACTION;
