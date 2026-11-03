# Chạy Web + Desktop (Admin, Manager)

Chạy trên **Windows 10/11**. Cần cài sẵn **Docker Desktop** (đang mở, hiện *Engine running*) và **.NET 10 SDK**. Mọi lệnh chạy trong PowerShell, tại thư mục `HeThongQuanLyThuVien`:

```powershell
cd <đường dẫn>\NNLTCSHARP\HeThongQuanLyThuVien
```

## 1. Bật SQL Server bằng Docker Compose

```powershell
Copy-Item .env.example .env
notepad .env
```

Trong `.env` đặt `MSSQL_SA_PASSWORD=<mật khẩu SQL của bạn>` (ít nhất 8 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt), lưu lại rồi chạy:

```powershell
docker compose up -d sqlserver
docker compose ps
```

Dòng `qltv-sqlserver` phải ở trạng thái `Up`.

## 2. Nạp bảng và dữ liệu mẫu (chỉ làm lần đầu)

```powershell
$env:SA = "<mật khẩu SQL của bạn>"
docker cp .\Database\AllTablesAndDemoData.sql qltv-sqlserver:/tmp/all.sql
docker exec -e "SQLCMDPASSWORD=$env:SA" qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -b -f 65001 -i /tmp/all.sql
```

## 3. Cấu hình API (chỉ làm lần đầu)

`<mật khẩu admin>`: từ 12 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt, không có dấu `$`.

```powershell
$api = ".\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj"
dotnet user-secrets set "ConnectionStrings:LibraryDb" 'Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<mật khẩu SQL>;Encrypt=True;TrustServerCertificate=True' --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:UserName" "admin" --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:DisplayName" "Quan tri vien" --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:Password" '<mật khẩu admin>' --project $api
```

## 4. Chạy (mỗi lệnh một cửa sổ PowerShell, đúng thứ tự)

**Cửa sổ 1, API** (chờ thấy `Now listening on: http://localhost:5058`):

```powershell
dotnet run --project .\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

**Cửa sổ 2, Web** (mở trình duyệt vào http://localhost:5109):

```powershell
dotnet run --project .\SourceCode\QuanLyThuVien.Web\QuanLyThuVien.Web.csproj --launch-profile http
```

**Cửa sổ 3, Desktop** (Admin và Manager dùng chung một ứng dụng):

```powershell
dotnet run --project .\SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

## 5. Đăng nhập

| Vai trò | Tài khoản | Ghi chú |
|---|---|---|
| **Admin** | `admin` và `<mật khẩu admin>` ở bước 3 | Đăng nhập thẳng ở Desktop |
| **Manager** (Thủ thư) | tự tạo, xem bên dưới | Đăng nhập ở Desktop |

**Tạo Manager (làm một lần):** đăng nhập Desktop bằng `admin` → mục **Tài khoản** → nhập Tên đăng nhập (ví dụ `thuthu.test`), Tên hiển thị, **Vai trò `THU_THU`**, **Mã nhân viên `1`**, Mật khẩu mới (12 ký tự trở lên, có hoa, thường, số, ký tự đặc biệt) → **Tạo tài khoản** → **Đăng xuất** rồi đăng nhập lại bằng tài khoản vừa tạo.

## 6. Tắt

Bấm `Ctrl+C` trong cửa sổ API và Web, đóng Desktop, rồi (không bắt buộc):

```powershell
docker compose stop
```

Lần sau chỉ cần: mở Docker Desktop, chạy lại **bước 4** (SQL Server tự bật cùng Docker).
