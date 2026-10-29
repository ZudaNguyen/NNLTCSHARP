# Chuyển môi trường làm việc sang PC Windows

Tài liệu này dùng khi chuyển từ macOS sang PC Windows để **vừa chạy Desktop Admin/Manager, vừa chạy Web độc giả** trên cùng một máy.

Mục tiêu sau khi làm xong:

- SQL Server chạy trong Docker ở `127.0.0.1,14333`.
- API chạy ở `http://localhost:5058`.
- Web độc giả chạy ở `http://localhost:5109`.
- Desktop WinForms mở được và đăng nhập bằng tài khoản Admin/Manager.

> Không có cách "chuyển phiên chat" sang máy khác. Cách đúng là mang **mã nguồn + database + tài liệu trạng thái** sang PC, rồi mở một phiên OpenCode mới tại đúng thư mục repo trên PC. Toàn bộ ngữ cảnh cần thiết nằm trong repo và trong tài liệu này.

---

## 1. Trạng thái hiện tại của nhánh

- Nhánh: `NgDang_user`
- Remote: `https://github.com/ZudaNguyen/NNLTCSHARP.git`
- Đã xong: API, Desktop Admin/Manager, Web độc giả, gửi/duyệt yêu cầu mượn, đăng ký độc giả, gợi ý tìm kiếm, giới hạn số lượng mượn theo số bản còn lại.
- Chưa làm: quy trình Manager gửi yêu cầu khóa tài khoản User lên Admin duyệt (bảng `YeuCauKhoaTaiKhoan`, API, màn hình Desktop, test, docs).

Chi tiết nghiệp vụ đã chốt:

- 3 vai trò: `ADMIN` (Admin), `THU_THU` (Manager/Thủ thư), `DOC_GIA` (User/Độc giả).
- Manager được khóa thẻ/tạm ngưng mượn ngay.
- Manager **không** được khóa tài khoản đăng nhập; phải gửi yêu cầu kèm lý do để Admin duyệt.
- `TaiKhoan.TrangThai = "Bị khóa"` chặn đăng nhập và thu hồi phiên.
- `DocGia.TrangThai = "Khóa thẻ"` chỉ chặn mượn/đặt sách, không chặn đăng nhập.

---

## 2. Cài trên PC Windows

Cần cài:

1. **Git for Windows** — https://git-scm.com/download/win
2. **.NET 10 SDK** (x64) — https://dotnet.microsoft.com/download
3. **Docker Desktop** (bật WSL 2 backend) — https://www.docker.com/products/docker-desktop/
4. **OpenCode** — tải bản CLI Windows x64 tại https://opencode.ai/v2/docs/ (mục CLI → Windows x64), giải nén và thêm thư mục chứa `opencode.exe` vào `PATH`. Có thể dùng OpenCode Desktop bản Windows nếu thích giao diện.
5. (Tùy chọn) **SQL Server Management Studio** hoặc **Azure Data Studio** để xem dữ liệu.
6. (Tùy chọn) **Visual Studio 2022/2026** có workload ".NET desktop development" nếu muốn bấm F5 cho Desktop.

Kiểm tra trong PowerShell:

```powershell
git --version
dotnet --version      # phải là 10.x
docker version
opencode --version
```

Nếu `dotnet --version` ra 8.x hoặc 9.x, cài thêm .NET 10 SDK và mở PowerShell mới.

---

## 3. Lấy mã nguồn

```powershell
cd $HOME\Code
git clone -b NgDang_user https://github.com/ZudaNguyen/NNLTCSHARP.git
cd NNLTCSHARP\HeThongQuanLyThuVien
git status
```

Kết quả mong đợi: `On branch NgDang_user`, working tree sạch.

Build thử toàn solution:

```powershell
dotnet build QuanLyThuVien.slnx
```

Kết quả mong đợi: `0 Error(s)`.

---

## 4. Database

Chọn **một** trong hai cách.

### Cách A — Tạo database mới từ script (sạch, nhanh)

```powershell
copy .env.example .env
notepad .env
```

Trong `.env`, đặt mật khẩu mạnh, ví dụ:

```text
MSSQL_SA_PASSWORD=Qltv_Dev_2026!Strong
```

Sau đó:

```powershell
docker compose up -d sqlserver
docker compose ps
```

Chờ container `qltv-sqlserver` ở trạng thái `running`/`healthy` (khoảng 20–40 giây), rồi nạp schema + dữ liệu demo:

```powershell
docker cp .\Database\AllTablesAndDemoData.sql qltv-sqlserver:/tmp/AllTablesAndDemoData.sql
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Qltv_Dev_2026!Strong" -C -b `
  -i /tmp/AllTablesAndDemoData.sql
```

Script này tự tạo `QuanLyThuVienDB`, **từ chối chạy** nếu đã có bảng, và **không** tạo tài khoản/mật khẩu. Tài khoản Admin sẽ được API tạo ở bước 5. Độc giả đăng ký tại `/dang-ky` trên Web.

Kiểm tra:

```powershell
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Qltv_Dev_2026!Strong" -C -d QuanLyThuVienDB `
  -Q "SELECT COUNT(*) AS SoBang FROM sys.tables;"
```

Mong đợi: `15`.

### Cách B — Mang nguyên dữ liệu đang có trên Mac sang (giữ tài khoản, phiếu mượn, phạt)

Trên **Mac**, tại thư mục `HeThongQuanLyThuVien`:

```bash
set -a && . ./.env && set +a
docker exec qltv-sqlserver mkdir -p /var/opt/mssql/backup
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
  -Q "BACKUP DATABASE QuanLyThuVienDB TO DISK='/var/opt/mssql/backup/QuanLyThuVienDB.bak' WITH INIT, COMPRESSION"
docker cp qltv-sqlserver:/var/opt/mssql/backup/QuanLyThuVienDB.bak ./QuanLyThuVienDB.bak
```

Chép file `QuanLyThuVienDB.bak` sang PC (USB, OneDrive, hoặc `scp`). Trên **PC**, sau khi `docker compose up -d sqlserver`:

```powershell
docker cp .\QuanLyThuVienDB.bak qltv-sqlserver:/var/opt/mssql/backup/QuanLyThuVienDB.bak
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Qltv_Dev_2026!Strong" -C `
  -Q "RESTORE FILELISTONLY FROM DISK='/var/opt/mssql/backup/QuanLyThuVienDB.bak'"
```

Xem 2 dòng kết quả để lấy tên logical file (thường là `QuanLyThuVienDB` và `QuanLyThuVienDB_log`), rồi:

```powershell
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "Qltv_Dev_2026!Strong" -C `
  -Q "RESTORE DATABASE QuanLyThuVienDB FROM DISK='/var/opt/mssql/backup/QuanLyThuVienDB.bak' WITH REPLACE, MOVE 'QuanLyThuVienDB' TO '/var/opt/mssql/data/QuanLyThuVienDB.mdf', MOVE 'QuanLyThuVienDB_log' TO '/var/opt/mssql/data/QuanLyThuVienDB_log.ldf'"
```

Nếu tên logical khác, thay đúng theo kết quả `RESTORE FILELISTONLY`. Không chạy script `AllTablesAndDemoData.sql` sau khi restore.

Với cách B, các tài khoản đã tạo trên Mac vẫn dùng được (mật khẩu không đổi).

---

## 5. Cấu hình API

Đặt connection string và tài khoản Admin khởi tạo vào User Secrets (không ghi vào repo):

```powershell
dotnet user-secrets set "ConnectionStrings:LibraryDb" `
  "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=Qltv_Dev_2026!Strong;Encrypt=True;TrustServerCertificate=True" `
  --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj

dotnet user-secrets set "Authentication:BootstrapAdmin:UserName"    "admin.dev" `
  --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj
dotnet user-secrets set "Authentication:BootstrapAdmin:DisplayName" "Quản trị hệ thống" `
  --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj
dotnet user-secrets set "Authentication:BootstrapAdmin:Password"    "Admin_Dev_2026!Strong" `
  --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj
```

- Mật khẩu Admin phải ≥ 12 ký tự, có hoa/thường/số/ký tự đặc biệt.
- Bootstrap chỉ chạy khi database **chưa có Admin**. Nếu dùng Cách B (restore), Admin đã tồn tại nên bước này không đổi mật khẩu cũ.

---

## 6. Chạy 3 tiến trình

Mở **3 cửa sổ PowerShell** tại `HeThongQuanLyThuVien`.

**Cửa sổ 1 — API (5058):**

```powershell
dotnet run --project SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

**Cửa sổ 2 — Web (5109):**

```powershell
dotnet run --project SourceCode\QuanLyThuVien.Web\QuanLyThuVien.Web.csproj --launch-profile http
```

**Cửa sổ 3 — Desktop:**

```powershell
$env:QUANLYTHUVIEN_API_URL = "http://localhost:5058"
dotnet run --project SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

Ghi chú:

- Desktop chỉ chạy trên Windows; trên macOS chỉ build được.
- Web **chỉ cho vai trò `DOC_GIA` đăng nhập**; Admin/Manager dùng Desktop.
- Web đọc `ApiBaseUrl` trong `SourceCode/QuanLyThuVien.Web/appsettings.json` (mặc định `http://localhost:5058/`).
- Desktop đọc biến `QUANLYTHUVIEN_API_URL` (mặc định `http://localhost:5058/`).

---

## 7. Kiểm tra nhanh

```powershell
curl.exe http://localhost:5058/api/dev/ping
```

Mong đợi: `{"message":"API dang hoat dong"}`.

Đăng nhập API bằng Admin:

```powershell
$body = '{"tenDangNhap":"admin.dev","matKhau":"Admin_Dev_2026!Strong"}'
$login = Invoke-RestMethod -Method Post -Uri http://localhost:5058/api/auth/login `
  -ContentType "application/json" -Body $body
$login.accessToken.Substring(0, 20) + "..."
```

Kiểm tra DB:

```powershell
Invoke-RestMethod -Uri http://localhost:5058/api/dev/db-check `
  -Headers @{ Authorization = "Bearer $($login.accessToken)" }
```

Web: mở `http://localhost:5109`, đăng ký một độc giả mới tại `/dang-ky`, đăng nhập, thử gửi yêu cầu mượn.

Desktop: đăng nhập `admin.dev` / `Admin_Dev_2026!Strong`, kiểm tra menu **Tài khoản**, **Nhân viên**, **Mượn trả**.

Tạo tài khoản Manager (nếu dùng Cách A):

```powershell
# Cần Git Bash hoặc WSL để chạy script bash
copy scripts\dev\.env.dev.example scripts\dev\.env.dev
notepad scripts\dev\.env.dev      # điền ADMIN_PASS (và MANAGER_PASS nếu cần)
bash scripts/dev/staff.sh new-staff "Tran Thi Thu Thu" thuthu.test
```

Hoặc tạo qua API: `POST /api/docgia` rồi `POST /api/taikhoan` bằng token Admin (xem `docs/MODULE_MAP.md`).

---

## 8. Mở OpenCode trên PC và tiếp tục công việc

```powershell
cd $HOME\Code\NNLTCSHARP
opencode
```

Trong phiên mới, dùng prompt mở đầu:

```text
Đọc AGENTS.md (nếu có), README.md, docs/MODULE_MAP.md, docs/PC_HANDOFF.md và
docs/SETUP_GUIDE.md. Đây là repo hệ thống quản lý thư viện C# .NET 10 trên nhánh
NgDang_user. Môi trường Windows đã chạy được SQL Server (Docker), API 5058,
Web 5109 và Desktop WinForms. Hãy xác nhận lại trạng thái build/run, sau đó
tiếp tục triển khai quy trình Manager gửi yêu cầu khóa tài khoản User lên Admin
duyệt.
```

Cần đăng nhập lại provider trong OpenCode trên PC (`/connect` hoặc `opencode auth login`) vì thông tin đăng nhập không đi kèm repo.

---

## 9. Lỗi thường gặp

| Triệu chứng | Cách xử lý |
|---|---|
| `Cannot connect to the Docker daemon` | Mở Docker Desktop, chờ engine xanh, chạy `docker version` |
| `docker compose up` báo thiếu `MSSQL_SA_PASSWORD` | Chưa có `.env` hoặc sai tên biến |
| Login SQL thất bại sau khi đổi `.env` | Volume giữ mật khẩu cũ; dùng đúng mật khẩu lúc tạo volume, không xóa volume nếu còn dữ liệu |
| Cổng 14333/5058/5109 bị chiếm | `netstat -ano | findstr :5058`, đổi cổng và cập nhật đồng thời `launchSettings.json`, `ApiBaseUrl`, `QUANLYTHUVIEN_API_URL` |
| API trả `503` | Thiếu `ConnectionStrings:LibraryDb` trong User Secrets |
| `db-check` trả `401` | Thiếu Bearer token |
| Web báo không kết nối API | API chưa chạy hoặc `ApiBaseUrl` sai cổng |
| Desktop không mở | Đang chạy trên macOS/Linux, hoặc thiếu .NET Desktop Runtime |
| Desktop báo lỗi API | Chưa đặt `$env:QUANLYTHUVIEN_API_URL` đúng cổng |
| `AllTablesAndDemoData.sql` báo bảng đã tồn tại | Database không trống; dùng script module hoặc restore `.bak`, đừng chạy lại script gộp |

---

## 10. Việc tiếp theo sau khi môi trường PC chạy ổn

1. Thêm bảng `YeuCauKhoaTaiKhoan` (16 bảng) + cập nhật `LibraryDbContext`.
2. API: tạo/xem/hủy yêu cầu cho Manager; duyệt/từ chối cho Admin.
3. Desktop: form gửi yêu cầu cho Manager; màn hình duyệt cho Admin.
4. Khi duyệt: khóa `TaiKhoan.TrangThai`, thu hồi phiên, và khóa `DocGia.TrangThai` (hoặc bổ sung kiểm tra trạng thái tài khoản khi lập phiếu).
5. Bổ sung lý do/thời hạn cho khóa thẻ.
6. Test phân quyền: Manager gọi API quản trị → `403`; Manager không duyệt được; Admin duyệt xong User không đăng nhập được.
7. Cập nhật `docs/MODULE_MAP.md`, `README.md`, `docs/SETUP_GUIDE.md` (15 → 16 bảng).
8. Commit theo tầng: `db`, `api`, `desktop`, `test`, `docs`; push `origin/NgDang_user`.
