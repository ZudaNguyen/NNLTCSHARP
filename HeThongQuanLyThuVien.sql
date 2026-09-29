CREATE TABLE [TheLoai] (
  [MaTheLoai] int PRIMARY KEY IDENTITY(1, 1),
  [TenTheLoai] varchar(100) NOT NULL,
  [MoTa] varchar(500)
)
GO

CREATE TABLE [TacGia] (
  [MaTacGia] int PRIMARY KEY IDENTITY(1, 1),
  [TenTacGia] varchar(150) NOT NULL,
  [QuocTich] varchar(100),
  [GioiThieu] varchar(500)
)
GO

CREATE TABLE [Sach] (
  [MaSach] int PRIMARY KEY IDENTITY(1, 1),
  [TenSach] varchar(200) NOT NULL,
  [ISBN] varchar(20),
  [NamXuatBan] int,
  [NhaXuatBan] varchar(150),
  [SoLuong] int NOT NULL DEFAULT (0),
  [SoLuongCon] int NOT NULL DEFAULT (0),
  [MaTheLoai] int
)
GO

CREATE TABLE [Sach_TacGia] (
  [MaSach] int,
  [MaTacGia] int,
  PRIMARY KEY ([MaSach], [MaTacGia])
)
GO

CREATE TABLE [DocGia] (
  [MaDocGia] int PRIMARY KEY IDENTITY(1, 1),
  [HoTen] varchar(150) NOT NULL,
  [NgaySinh] date,
  [GioiTinh] varchar(20),
  [DiaChi] varchar(250),
  [SoDienThoai] varchar(15),
  [Email] varchar(150),
  [NgayDangKy] date,
  [TrangThai] varchar(50)
)
GO

CREATE TABLE [NhanVien] (
  [MaNhanVien] int PRIMARY KEY IDENTITY(1, 1),
  [HoTen] varchar(150) NOT NULL,
  [SoDienThoai] varchar(15),
  [Email] varchar(150),
  [TaiKhoan] varchar(100) UNIQUE,
  [MatKhau] varchar(255),
  [ChucVu] varchar(100)
)
GO

CREATE TABLE [PhieuMuon] (
  [MaPhieuMuon] int PRIMARY KEY IDENTITY(1, 1),
  [MaDocGia] int NOT NULL,
  [MaNhanVien] int NOT NULL,
  [NgayMuon] date,
  [HanTra] date,
  [TrangThai] varchar(50)
)
GO

CREATE TABLE [ChiTiet_PhieuMuon] (
  [MaPhieuMuon] int,
  [MaSach] int,
  [SoLuong] int DEFAULT (1),
  [NgayTra] date,
  [TienPhat] decimal(18,2) DEFAULT (0),
  [TinhTrangSach] varchar(200),
  PRIMARY KEY ([MaPhieuMuon], [MaSach])
)
GO

ALTER TABLE [Sach] ADD FOREIGN KEY ([MaTheLoai]) REFERENCES [TheLoai] ([MaTheLoai])
GO

ALTER TABLE [Sach_TacGia] ADD FOREIGN KEY ([MaSach]) REFERENCES [Sach] ([MaSach])
GO

ALTER TABLE [Sach_TacGia] ADD FOREIGN KEY ([MaTacGia]) REFERENCES [TacGia] ([MaTacGia])
GO

ALTER TABLE [PhieuMuon] ADD FOREIGN KEY ([MaDocGia]) REFERENCES [DocGia] ([MaDocGia])
GO

ALTER TABLE [PhieuMuon] ADD FOREIGN KEY ([MaNhanVien]) REFERENCES [NhanVien] ([MaNhanVien])
GO

ALTER TABLE [ChiTiet_PhieuMuon] ADD FOREIGN KEY ([MaPhieuMuon]) REFERENCES [PhieuMuon] ([MaPhieuMuon])
GO

ALTER TABLE [ChiTiet_PhieuMuon] ADD FOREIGN KEY ([MaSach]) REFERENCES [Sach] ([MaSach])
GO
