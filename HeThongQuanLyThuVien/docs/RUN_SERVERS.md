# Hướng dẫn chạy API + Web (macOS và Windows)

Tài liệu này tập trung vào việc **chạy lại hệ thống hằng ngày**. Phần cài đặt database từ đầu, tạo tài khoản Admin và cấu hình chi tiết nằm trong [`SETUP_GUIDE.md`](SETUP_GUIDE.md).

## 0. Kiến trúc và cổng

```text
SQL Server (Docker, cổng 14333)
        ↓
API  (http://localhost:5058)   ← Desktop gọi vào
        ↓
Web  (http://localhost:5109)   ← độc giả mở trình duyệt
```

| Thành phần | Cổng | Chạy bằng | Tắt máy có tự lên lại? |
|---|---:|---|---|
| SQL Server | `14333` | Docker container `qltv-sqlserver` | Có, nếu Docker Desktop được mở |
| API | `5058` | `dotnet` | Không, phải chạy lại |
| Web | `5109` | `dotnet` | Không, phải chạy lại |

Thứ tự bắt buộc: **SQL Server → API → Web**.

Web không truy cập database trực tiếp. Mọi dữ liệu đi qua API, nên API phải chạy trước.

---

## 1. Chuẩn bị một lần trên mỗi máy

Bỏ qua mục này nếu máy đã chạy được hệ thống trước đó.

### 1.1. Yêu cầu

- .NET 10 SDK
- Docker Desktop
- Đã clone repo và đứng tại thư mục `HeThongQuanLyThuVien`

Kiểm tra:

```bash
dotnet --version
docker version
```

### 1.2. File `.env` và SQL Server

```bash
cd HeThongQuanLyThuVien
cp .env.example .env
```

Mở `.env`, đặt `MSSQL_SA_PASSWORD` thành mật khẩu mạnh. File `.env` đã được gitignore, không commit.

Khởi động database:

```bash
docker compose up -d sqlserver
docker compose ps
```

### 1.3. Connection string cho API

Connection string lưu bằng User Secrets, **không** nằm trong `appsettings.json` và không nằm trong git.

macOS:

```bash
dotnet user-secrets set "ConnectionStrings:LibraryDb" \
  "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MAT_KHAU_SA>;Encrypt=True;TrustServerCertificate=True" \
  --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj
```

Windows PowerShell:

```powershell
dotnet user-secrets set "ConnectionStrings:LibraryDb" "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MAT_KHAU_SA>;Encrypt=True;TrustServerCertificate=True" --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj
```

Kiểm tra:

```bash
dotnet user-secrets list --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj
```

### 1.4. Database và schema

Database demo trống cần được tạo schema + dữ liệu mẫu. Làm theo một trong hai cách trong [`SETUP_GUIDE.md`](SETUP_GUIDE.md):

- Chạy 8 file schema theo thứ tự, hoặc
- Chạy file gộp `Database/AllTablesAndDemoData.sql` ở SQLCMD Mode.

Không chạy script gộp lên database đang có dữ liệu cần giữ.

---

## 2. Chạy trên macOS

### Bước 1 — Mở Docker Desktop

```bash
open -a Docker
```

Chờ Docker sẵn sàng:

```bash
until docker info >/dev/null 2>&1; do
  echo "Đang chờ Docker..."
  sleep 2
done
```

### Bước 2 — Bật SQL Server

```bash
cd /Users/<ban>/Code/NNLTCSHARP/HeThongQuanLyThuVien
docker compose up -d sqlserver
docker compose ps
```

### Bước 3 — Chạy API ở terminal thứ nhất

```bash
cd /Users/<ban>/Code/NNLTCSHARP/HeThongQuanLyThuVien

dotnet run --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj \
  --launch-profile http
```

Nếu đang đứng trong thư mục `SourceCode`, dùng đường dẫn ngắn hơn:

```bash
dotnet run --project QuanLyThuVien.Api/QuanLyThuVien.Api.csproj --launch-profile http
```

Chờ log:

```text
Now listening on: http://localhost:5058
```

### Bước 4 — Chạy Web ở terminal thứ hai

```bash
cd /Users/<ban>/Code/NNLTCSHARP/HeThongQuanLyThuVien

dotnet run --project SourceCode/QuanLyThuVien.Web/QuanLyThuVien.Web.csproj \
  --launch-profile http
```

Mở trình duyệt:

```text
http://localhost:5109
```

### Bước 5 — Kiểm tra

```bash
curl http://localhost:5058/api/dev/ping
curl -I http://localhost:5109
```

Kết quả mong đợi:

```text
{"message":"API dang hoat dong"}
HTTP/1.1 200 OK
```

---

## 3. Chạy trên Windows

Áp dụng cho Windows 10/11, dùng PowerShell.

### Bước 1 — Mở Docker Desktop

Mở **Docker Desktop** từ Start Menu và chờ engine running:

```powershell
docker version
```

### Bước 2 — Bật SQL Server

```powershell
cd C:\duong\dan\NNLTCSHARP\HeThongQuanLyThuVien
docker compose up -d sqlserver
docker compose ps
```

### Bước 3 — Chạy API ở cửa sổ PowerShell thứ nhất

```powershell
cd C:\duong\dan\NNLTCSHARP\HeThongQuanLyThuVien

dotnet run --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

Nếu đang đứng trong thư mục `SourceCode`:

```powershell
dotnet run --project QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

### Bước 4 — Chạy Web ở cửa sổ PowerShell thứ hai

```powershell
cd C:\duong\dan\NNLTCSHARP\HeThongQuanLyThuVien

dotnet run --project SourceCode\QuanLyThuVien.Web\QuanLyThuVien.Web.csproj --launch-profile http
```

Mở:

```text
http://localhost:5109
```

### Bước 5 — Kiểm tra

```powershell
Invoke-RestMethod http://localhost:5058/api/dev/ping
Invoke-WebRequest http://localhost:5109 -UseBasicParsing | Select-Object StatusCode
```

Hoặc dùng `curl.exe` có sẵn trong Windows:

```powershell
curl.exe http://localhost:5058/api/dev/ping
curl.exe -I http://localhost:5109
```

### Desktop Admin/Manager

Desktop chỉ chạy trên Windows và cần API đang hoạt động:

```powershell
dotnet run --project SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

Trên macOS chỉ build được, không mở được giao diện:

```bash
dotnet build QuanLyThuVien.slnx
```

---

## 4. Dừng server

### macOS

Nhấn `Ctrl+C` ở terminal API và terminal Web.

Dừng SQL Server:

```bash
cd /Users/<ban>/Code/NNLTCSHARP/HeThongQuanLyThuVien
docker compose stop sqlserver
```

Nếu cổng bị tiến trình cũ giữ:

```bash
lsof -ti:5058 | xargs kill
lsof -ti:5109 | xargs kill
```

### Windows PowerShell

Nhấn `Ctrl+C` ở hai cửa sổ PowerShell.

Nếu cổng bị chiếm:

```powershell
Get-NetTCPConnection -LocalPort 5058 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
Get-NetTCPConnection -LocalPort 5109 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

Dừng SQL Server:

```powershell
docker compose stop sqlserver
```

---

## 5. Chạy nền (tùy chọn)

Chỉ dùng khi đã quen. Sau khi build:

```bash
cd HeThongQuanLyThuVien
dotnet build QuanLyThuVien.slnx
```

### macOS

Chạy API nền:

```bash
( cd SourceCode/QuanLyThuVien.Api && \
  nohup dotnet run --no-build --launch-profile http > /tmp/qltv-api.log 2>&1 & )
```

Chạy Web nền:

```bash
( cd SourceCode/QuanLyThuVien.Web && \
  nohup dotnet run --no-build --launch-profile http > /tmp/qltv-web.log 2>&1 & )
```

`--launch-profile http` tự đặt `ASPNETCORE_ENVIRONMENT=Development` và đúng cổng. `cd` vào thư mục project là bắt buộc: nếu chạy file `.dll` từ thư mục khác, app sẽ không tìm thấy `appsettings.json`.

Xem log:

```bash
tail -f /tmp/qltv-api.log
tail -f /tmp/qltv-web.log
```

Dừng:

```bash
lsof -ti:5058 | xargs kill
lsof -ti:5109 | xargs kill
```

### Windows PowerShell

```powershell
cd C:\duong\dan\NNLTCSHARP\HeThongQuanLyThuVien
dotnet build QuanLyThuVien.slnx

Start-Process dotnet -ArgumentList "run","--no-build","--launch-profile","http" `
  -WorkingDirectory "SourceCode\QuanLyThuVien.Api" `
  -RedirectStandardOutput "$PWD\api.log" `
  -RedirectStandardError "$PWD\api.err.log"

Start-Process dotnet -ArgumentList "run","--no-build","--launch-profile","http" `
  -WorkingDirectory "SourceCode\QuanLyThuVien.Web" `
  -RedirectStandardOutput "$PWD\web.log" `
  -RedirectStandardError "$PWD\web.err.log"
```

Xem log:

```powershell
Get-Content api.log -Wait
Get-Content web.log -Wait
```

Dừng:

```powershell
Get-NetTCPConnection -LocalPort 5058 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
Get-NetTCPConnection -LocalPort 5109 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

---

## 6. Lỗi thường gặp

### `address already in use`

Một instance API hoặc Web cũ vẫn đang chạy và giữ cổng.

macOS:

```bash
lsof -nP -iTCP:5058 -sTCP:LISTEN
lsof -nP -iTCP:5109 -sTCP:LISTEN

lsof -ti:5058 | xargs kill
lsof -ti:5109 | xargs kill
```

Windows:

```powershell
Get-NetTCPConnection -LocalPort 5058 | Select-Object OwningProcess
Get-NetTCPConnection -LocalPort 5109 | Select-Object OwningProcess

Get-NetTCPConnection -LocalPort 5058 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
Get-NetTCPConnection -LocalPort 5109 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

Chỉ chạy **một** instance API và **một** instance Web.

### API trả `503`

Nguyên nhân thường gặp:

- Chưa cấu hình `ConnectionStrings:LibraryDb`
- SQL Server chưa chạy
- Sai mật khẩu trong connection string

Kiểm tra container:

```bash
docker compose ps
docker compose logs --tail 50 sqlserver
```

### Database thiếu cột hoặc bảng sau khi pull code mới

Code có thể dùng cột/bảng mới trong khi database local chưa được cập nhật. Hai script additive thường gặp:

| Script | Bổ sung |
|---|---|
| `BorrowingModuleSchema.sql` | Cột `PhieuMuon.SoLanGiaHan` (gia hạn mượn) |
| `AccountLockRequestModuleSchema.sql` | Bảng `YeuCauKhoaTaiKhoan` (duyệt khóa tài khoản) |

Chỉ chạy trên database demo hoặc sau khi đã sao lưu. macOS:

```bash
cd /Users/<ban>/Code/NNLTCSHARP/HeThongQuanLyThuVien
set -a; . ./.env; set +a

docker cp SourceCode/QuanLyThuVien.Infrastructure/Data/Sql/BorrowingModuleSchema.sql \
  qltv-sqlserver:/tmp/BorrowingModuleSchema.sql
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d QuanLyThuVienDB -b \
  -i /tmp/BorrowingModuleSchema.sql

docker cp SourceCode/QuanLyThuVien.Infrastructure/Data/Sql/AccountLockRequestModuleSchema.sql \
  qltv-sqlserver:/tmp/AccountLockRequestModuleSchema.sql
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d QuanLyThuVienDB -b \
  -i /tmp/AccountLockRequestModuleSchema.sql
```

Windows PowerShell:

```powershell
cd C:\duong\dan\NNLTCSHARP\HeThongQuanLyThuVien

docker cp SourceCode\QuanLyThuVien.Infrastructure\Data\Sql\BorrowingModuleSchema.sql qltv-sqlserver:/tmp/BorrowingModuleSchema.sql
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<MAT_KHAU_SA>" -C -d QuanLyThuVienDB -b -i /tmp/BorrowingModuleSchema.sql

docker cp SourceCode\QuanLyThuVien.Infrastructure\Data\Sql\AccountLockRequestModuleSchema.sql qltv-sqlserver:/tmp/AccountLockRequestModuleSchema.sql
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<MAT_KHAU_SA>" -C -d QuanLyThuVienDB -b -i /tmp/AccountLockRequestModuleSchema.sql
```

Sau khi thêm schema, khởi động lại API.

### Web báo lỗi đăng nhập

- Web chỉ cho tài khoản `DOC_GIA` đăng nhập.
- Admin/Thủ thư dùng Desktop.
- Kiểm tra API đang chạy ở `5058` trước khi đăng nhập.

### Sửa `.cshtml` nhưng giao diện không đổi

Web không bật Razor runtime compilation. Phải dừng Web, build lại và chạy lại:

```bash
# Ctrl+C ở terminal Web
dotnet run --project SourceCode/QuanLyThuVien.Web/QuanLyThuVien.Web.csproj --launch-profile http
```

Riêng CSS/JS trong `wwwroot` được phục vụ trực tiếp, chỉ cần tải lại trang với `Cmd+Shift+R` (macOS) hoặc `Ctrl+F5` (Windows).

### `dotnet: command not found`

Chưa cài .NET 10 SDK hoặc chưa mở lại terminal sau khi cài. Cài từ <https://dotnet.microsoft.com/download> rồi mở terminal mới.

---

## 7. Bảng lệnh nhanh

| Việc cần làm | macOS | Windows PowerShell |
|---|---|---|
| Bật SQL Server | `docker compose up -d sqlserver` | `docker compose up -d sqlserver` |
| Chạy API | `dotnet run --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj --launch-profile http` | `dotnet run --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http` |
| Chạy Web | `dotnet run --project SourceCode/QuanLyThuVien.Web/QuanLyThuVien.Web.csproj --launch-profile http` | `dotnet run --project SourceCode\QuanLyThuVien.Web\QuanLyThuVien.Web.csproj --launch-profile http` |
| Chạy Desktop | Không chạy được | `dotnet run --project SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj` |
| Build toàn bộ | `dotnet build QuanLyThuVien.slnx` | `dotnet build QuanLyThuVien.slnx` |
| Chạy test | `dotnet test QuanLyThuVien.slnx` | `dotnet test QuanLyThuVien.slnx` |
| Kiểm tra API | `curl http://localhost:5058/api/dev/ping` | `Invoke-RestMethod http://localhost:5058/api/dev/ping` |
| Dừng cổng 5058 | `lsof -ti:5058 \| xargs kill` | `Get-NetTCPConnection -LocalPort 5058 \| ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }` |
| Dừng cổng 5109 | `lsof -ti:5109 \| xargs kill` | `Get-NetTCPConnection -LocalPort 5109 \| ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }` |

Mọi lệnh trong bảng giả định bạn đang đứng tại thư mục `HeThongQuanLyThuVien` (nơi có `QuanLyThuVien.slnx` và `docker-compose.yml`).
