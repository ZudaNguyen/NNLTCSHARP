CREATE TABLE [TheLoai] (
  [MaTheLoai] int PRIMARY KEY IDENTITY(1, 1),
  [TenTheLoai] nvarchar(100) NOT NULL, -- Nên dùng nvarchar cho tiếng Việt
  [MoTa] nvarchar(500),
  [IsDeleted] bit DEFAULT (0) -- Thêm: Cờ xóa mềm
)
GO

CREATE TABLE [TacGia] (
  [MaTacGia] int PRIMARY KEY IDENTITY(1, 1),
  [TenTacGia] nvarchar(150) NOT NULL,
  [QuocTich] nvarchar(100),
  [GioiThieu] nvarchar(500),
  [IsDeleted] bit DEFAULT (0) -- Thêm: Cờ xóa mềm
)
GO

CREATE TABLE [Sach] (
  [MaSach] int PRIMARY KEY IDENTITY(1, 1),
  [TenSach] nvarchar(200) NOT NULL,
  [ISBN] varchar(20),
  [NamXuatBan] int,
  [NhaXuatBan] nvarchar(150),
  [SoLuong] int NOT NULL DEFAULT (0),
  [SoLuongCon] int NOT NULL DEFAULT (0),
  [MaTheLoai] int,
  [HinhAnh] varchar(500), -- Thêm: Lưu đường dẫn ảnh phục vụ Web
  [MoTaChiTiet] nvarchar(max), -- Thêm: Mô tả dài cho Web
  [IsDeleted] bit DEFAULT (0) -- Thêm: Cờ xóa mềm
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
  [HoTen] nvarchar(150) NOT NULL,
  [NgaySinh] date,
  [GioiTinh] nvarchar(20),
  [DiaChi] nvarchar(250),
  [SoDienThoai] varchar(15),
  [Email] varchar(150),
  [NgayDangKy] date,
  [TrangThai] nvarchar(50)
)
GO

CREATE TABLE [NhanVien] (
  [MaNhanVien] int PRIMARY KEY IDENTITY(1, 1),
  [HoTen] nvarchar(150) NOT NULL,
  [SoDienThoai] varchar(15),
  [Email] varchar(150),
  [TaiKhoan] varchar(100) UNIQUE,
  [MatKhau] varchar(255),
  [ChucVu] nvarchar(100),
  [IsDeleted] bit DEFAULT (0) -- Thêm: Cờ xóa mềm
)
GO

CREATE TABLE [PhieuMuon] (
  [MaPhieuMuon] int PRIMARY KEY IDENTITY(1, 1),
  [MaDocGia] int NOT NULL,
  [MaNhanVien] int NOT NULL,
  [NgayMuon] date,
  [HanTra] date,
  [TrangThai] nvarchar(50),
  [NgayTao] datetime DEFAULT GETDATE() -- Thêm: Dấu vết thời gian tạo phiếu
)
GO

CREATE TABLE [ChiTiet_PhieuMuon] (
  [MaPhieuMuon] int,
  [MaSach] int,
  [SoLuong] int DEFAULT (1),
  [NgayTra] date,
  [TienPhat] decimal(18,2) DEFAULT (0),
  [TinhTrangSach] nvarchar(200),
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