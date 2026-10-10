# Quản Lý Thư Viện

Hệ thống quản lý thư viện gồm Web người dùng, Web API và ứng dụng quản trị Windows Forms, sử dụng chung dữ liệu SQL Server.

## Cấu trúc thư mục

- `SourceCode`: toàn bộ mã nguồn chính của hệ thống.
- `Tests`: các project kiểm thử tự động.
- `Database`: script, migration và dữ liệu mẫu SQL Server.
- `Documents`: báo cáo, sơ đồ, wireframe và ảnh minh họa.

## Các project

- `QuanLyThuVien.Domain`: entity, enum và quy tắc miền độc lập.
- `QuanLyThuVien.Application`: DTO, interface, validation và use case.
- `QuanLyThuVien.Infrastructure`: Entity Framework Core, SQL Server và triển khai repository/service.
- `QuanLyThuVien.Api`: API dùng chung cho Web và Desktop.
- `QuanLyThuVien.Web`: ASP.NET Core MVC dành cho độc giả.
- `QuanLyThuVien.Desktop`: Windows Forms dành cho nhân viên/quản trị.
- `QuanLyThuVien.UnitTests`: kiểm thử quy tắc nghiệp vụ.
- `QuanLyThuVien.IntegrationTests`: kiểm thử API và hạ tầng.

## Lệnh cơ bản

```powershell
dotnet restore QuanLyThuVien.sln
dotnet build QuanLyThuVien.sln
dotnet run --project SourceCode/QuanLyThuVien.Api
dotnet run --project SourceCode/QuanLyThuVien.Web --launch-profile http
dotnet run --project SourceCode/QuanLyThuVien.Desktop
dotnet test QuanLyThuVien.sln
```

Xem hướng dẫn dành cho thành viên mới trong `HUONG_DAN_CAI_DAT_VA_CHAY.md` và phạm vi dự án trong `KE_HOACH_DU_AN.md`.
