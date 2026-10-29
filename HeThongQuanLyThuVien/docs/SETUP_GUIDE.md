# Hướng dẫn cài đặt và chạy

Áp dụng cho **macOS**, **Windows 10/11** và **Linux**. Cần .NET 10 SDK và Docker Desktop (hoặc Docker Engine + Compose).

> Lệnh schema và seeder **thay đổi dữ liệu**. Chỉ chạy trên database demo đã được phép, và sao lưu trước nếu database đang có dữ liệu cần giữ.

## 1. Chuẩn bị

```bash
cd HeThongQuanLyThuVien
dotnet --info
docker version
```

Mở Docker Desktop và chờ engine sẵn sàng. `.env.example` chỉ chứa placeholder:

```bash
cp .env.example .env     # Windows PowerShell: Copy-Item .env.example .env
# Sửa .env, đặt MSSQL_SA_PASSWORD thành mật khẩu mạnh, riêng cho máy bạn.
docker compose up -d sqlserver
docker compose ps
docker compose logs --tail 80 sqlserver
```

Compose ánh xạ `127.0.0.1:14333` (host) → `1433` (container), dữ liệu nằm trong volume `qltv_sql_data`.

**Không** chạy `docker compose down -v`, `docker volume rm` hay xóa volume để "sửa lỗi". Volume đã khởi tạo sẽ giữ mật khẩu SA cũ; đổi `MSSQL_SA_PASSWORD` trong `.env` không làm đổi mật khẩu của volume đó.

### Kiểm tra container đã lên

```bash
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<MAT_KHAU_SA>' -C -Q "SELECT @@VERSION"
```

Kết quả mong đợi có dòng `Microsoft SQL Server 2022 ... on Linux (Ubuntu 22.04)`.

> Trên macOS **không có** bản SQL Server native và không có SSMS. SQL Server chỉ chạy trong Docker (image Linux); trên Mac Intel container chạy native amd64, trên Apple Silicon chạy qua Rosetta. Công cụ quản lý: extension **mssql** của VS Code hoặc **DBeaver**, kết nối `127.0.0.1,14333`, SQL Login `sa`.

## 2. Database và schema

Các file schema nằm trong `SourceCode/QuanLyThuVien.Infrastructure/Data/Sql/` và **không phải EF migrations**. Chúng tạo/cập nhật bảng nên không được chạy mù trên database đang có dữ liệu.

### 2.1 Database demo trống — đường nhanh nhất

Chạy lần lượt 8 file theo đúng thứ tự phụ thuộc. Script không tạo database, nên tạo trước:

```bash
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<MAT_KHAU_SA>' -C \
  -Q "IF DB_ID('QuanLyThuVienDB') IS NULL CREATE DATABASE QuanLyThuVienDB;"

for f in BooksSchema.sql CatalogModuleSchema.sql ReaderModuleSchema.sql \
         EmployeeModuleSchema.sql AuthenticationModuleSchema.sql BorrowingModuleSchema.sql \
         BorrowRequestModuleSchema.sql AccountLockRequestModuleSchema.sql; do
  docker cp "SourceCode/QuanLyThuVien.Infrastructure/Data/Sql/$f" qltv-sqlserver:/tmp/"$f"
  docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P '<MAT_KHAU_SA>' -C -d QuanLyThuVienDB -b -i /tmp/"$f"
done
```

Kiểm tra kết quả — phải có **16 bảng**:

```bash
docker exec qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<MAT_KHAU_SA>' -C -d QuanLyThuVienDB \
  -Q "SELECT COUNT(*) FROM sys.tables;"
```

Trên Windows có thể thay bằng SSMS: mở từng file, chọn đúng database `QuanLyThuVienDB`, bật **Query → SQLCMD Mode**, chạy theo thứ tự trên.

### 2.2 Một file gộp cho database demo trống

[`Database/AllTablesAndDemoData.sql`](../Database/AllTablesAndDemoData.sql) gộp schema + dữ liệu mẫu. File có thể tạo database, không xóa bảng, và **từ chối chạy** nếu đã có bảng nghiệp vụ. Bắt buộc bật **SQLCMD Mode** vì script dùng `:ON ERROR EXIT`. Không dùng cho database đang có dữ liệu.

### 2.3 Database đang có dữ liệu

1. Xác nhận đúng server/database: `SELECT @@SERVERNAME, DB_NAME();`
2. Tạo full backup và kiểm tra `RESTORE VERIFYONLY`. Volume Docker **không phải** bản backup.
3. Đối chiếu bảng/cột/FK hiện có với `LibraryDbContext.cs` và từng script.
4. Khôi phục backup vào một database test riêng, chạy thử schema ở đó trước.
5. Dừng ngay nếu có lỗi hoặc dữ liệu legacy không khớp; không chạy script sửa/xóa tùy tiện.

## 3. Cấu hình API và Admin khởi tạo

Connection string lấy từ User Secrets hoặc biến môi trường, **không** từ `appsettings.json`:

```bash
dotnet user-secrets set "ConnectionStrings:LibraryDb" \
  "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MAT_KHAU_SA>;Encrypt=True;TrustServerCertificate=True" \
  --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj
```

Cách khác: đặt biến `ConnectionStrings__LibraryDb` trong phiên terminal hiện tại. Không lưu lệnh chứa mật khẩu vào script, log hay repo.

Khi database **chưa có Admin nào**, API tạo một tài khoản bootstrap nếu đủ ba cấu hình:

```bash
dotnet user-secrets set "Authentication:BootstrapAdmin:UserName"    "<TEN_DANG_NHAP_ADMIN>" --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj
dotnet user-secrets set "Authentication:BootstrapAdmin:DisplayName" "<TEN_HIEN_THI>"        --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj
dotnet user-secrets set "Authentication:BootstrapAdmin:Password"    "<MAT_KHAU_MANH>"       --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj
```

Mật khẩu phải ≥ 12 ký tự và có chữ hoa, chữ thường, chữ số, ký tự đặc biệt. Bootstrap không đặt lại mật khẩu và không sửa Admin đã tồn tại.

Tài khoản Thủ thư/Độc giả: tạo qua API (`POST /api/docgia` rồi `POST /api/taikhoan` bằng token Admin), **không** chèn SQL trực tiếp. Xem [MODULE_MAP](MODULE_MAP.md).

## 4. Chạy API, Web và Desktop

### API — cổng 5058

```bash
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj --launch-profile http
```

Windows PowerShell: `$env:DOTNET_ENVIRONMENT = "Development"` rồi chạy lệnh `dotnet run` tương ứng.

Kiểm tra:

```bash
curl http://localhost:5058/api/dev/ping
# {"message":"API dang hoat dong"}

TOKEN=$(curl -s -X POST http://localhost:5058/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"tenDangNhap":"<ADMIN_USER>","matKhau":"<ADMIN_PASSWORD>"}')
curl http://localhost:5058/api/dev/db-check -H "Authorization: Bearer $TOKEN"
```

`/api/dev/db-check` yêu cầu Bearer token; `/api/dev/ping` thì không. Hai endpoint này chỉ tồn tại ở môi trường `Development`.

Nếu chưa cấu hình connection string, API vẫn khởi động và các route nghiệp vụ trả `503` — đây là hành vi có chủ đích để dev UI không cần DB.

### Web — cổng 5109

Cần API đang chạy ở cổng 5058 (Web **không** truy cập database trực tiếp, mọi dữ liệu đi qua REST API):

```bash
dotnet run --project SourceCode/QuanLyThuVien.Web/QuanLyThuVien.Web.csproj --launch-profile http
```

Mở `http://localhost:5109`. Web đọc địa chỉ API từ `ApiBaseUrl` trong `appsettings.json` (mặc định `http://localhost:5058/`); đổi cổng API thì sửa khóa này.

Web dành cho độc giả, dùng cookie authentication: sau khi đăng nhập, token của API được giữ trong claim `api_token` của cookie HttpOnly (`QuanLyThuVien.Reader`, 8 giờ, sliding) — không lưu localStorage và không có session server.

| Route | Nội dung |
|---|---|
| `/` | Trang chủ: sách nổi bật, thể loại, sách đang mượn, thông báo hạn trả/quá hạn |
| `/sach` | Danh mục + tìm kiếm (`?search=`) và lọc thể loại (`?maTheLoai=`) |
| `/sach/{id}` | Chi tiết sách, tình trạng còn/hết |
| `/dang-nhap`, `/dang-xuat` | Đăng nhập / đăng xuất (chỉ vai trò `DOC_GIA`) |
| `/lich-su-muon` | Lịch sử mượn trả (`?trangThai=dang-muon` để lọc phiếu chưa trả hết) |
| `/lich-su-muon/{id}` | Chi tiết phiếu mượn + tiền phạt từng lần trả |
| `/tai-khoan` | Hồ sơ độc giả và phiếu mượn gần đây |

Các trang không cần đăng nhập: `/`, `/sach`, `/sach/{id}`, `/dang-nhap`. Các trang còn lại chuyển hướng về `/dang-nhap`.

`/Home/Manager` và `/Home/Admin` vẫn là mockup tĩnh của giao diện Desktop, không dùng dữ liệu thật và không thuộc phần Web độc giả.

### Desktop — chỉ Windows

Cần API đang chạy. Trong terminal khác:

```powershell
dotnet run --project SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

Trên macOS/Linux chỉ build được (compile-check), không mở được giao diện:

```bash
dotnet build QuanLyThuVien.slnx
```

### Test nghiệp vụ Admin/Thủ thư trên macOS

Không cần Desktop: dùng [`scripts/dev/staff.sh`](../scripts/dev/staff.sh) để lập phiếu, trả sách, thu tiền phạt, xem dashboard qua API, rồi F5 Web độc giả để xem kết quả. Hướng dẫn đầy đủ (kèm cách dựng Windows VM cho giao diện Desktop): [`docs/TESTING_ON_MACOS.md`](TESTING_ON_MACOS.md).

```bash
cp scripts/dev/.env.dev.example scripts/dev/.env.dev   # điền ADMIN_PASS
./scripts/dev/staff.sh whoami
AS=manager ./scripts/dev/staff.sh dashboard             # kiểm tra phân quyền thủ thư
```

## 5. Seed dữ liệu demo

Seeder **opt-in**, chạy riêng, không tự chạy cùng API. Nó chỉ chấp nhận `Development`, host `127.0.0.1,14333`, database `QuanLyThuVienDB`, và **yêu cầu full backup không quá 7 ngày đã qua `RESTORE VERIFYONLY`**. Trên database mới tạo chưa có backup, seeder sẽ từ chối — đó là hành vi đúng, không tắt guard.

```bash
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project Database/seeds/QuanLyThuVien.DemoSeeder.csproj
```

Seeder idempotent theo khóa demo, không xóa dữ liệu, không tạo thông tin đăng nhập. Chi tiết: [`Database/seeds/README.md`](../Database/seeds/README.md).

## 6. Xử lý lỗi thường gặp

| Triệu chứng | Kiểm tra |
|---|---|
| `Cannot connect to the Docker daemon` | Mở Docker Desktop, chờ engine xanh, rồi `docker version` |
| `docker compose up` báo thiếu `MSSQL_SA_PASSWORD` | Chưa có `.env`; copy từ `.env.example` và điền mật khẩu |
| Login SQL thất bại sau khi đổi `.env` | Volume đã giữ mật khẩu cũ; dùng mật khẩu lúc tạo volume, **không** xóa volume |
| Cổng 14333 bị chiếm | Tìm tiến trình đang giữ cổng; đổi cổng phải sửa đồng thời `docker-compose.yml` và connection string |
| `error MSB4025` / `NETSDK1100` | Thiếu `Directory.Build.props` hoặc build sai project; build bằng `dotnet build QuanLyThuVien.slnx` |
| API chạy nhưng nghiệp vụ trả `503` | Chưa có `ConnectionStrings:LibraryDb` trong User Secrets |
| `db-check` trả `401` | Cần Bearer token từ `/api/auth/login`; ping không kiểm tra DB |
| Gọi API quản trị bằng tài khoản độc giả | Trả `403` là đúng — xem ma trận quyền trong [MODULE_MAP](MODULE_MAP.md) |
| Seeder từ chối chạy | Thiếu Development, sai host/DB, thiếu `CauHinhTienPhat`, hoặc chưa có backup ≤ 7 ngày verify được |
| Schema lỗi constraint | Dừng lại, lưu thông báo lỗi, so sánh schema, khôi phục từ backup |

## 7. Build và test

```bash
dotnet build QuanLyThuVien.slnx
dotnet test Tests/QuanLyThuVien.UnitTests/QuanLyThuVien.UnitTests.csproj
```

Build thành công **không** xác nhận kết nối database, đăng nhập, CRUD runtime hay hiển thị giao diện. Bộ `Tests/QuanLyThuVien.UnitTests` hiện có **30 test** cho `YeuCauMuonService`, `YeuCauKhoaTaiKhoanService` và `AccountService`.

## 8. Kết quả kiểm chứng gần nhất

Ngày 2026-10-29, trên macOS (Intel i7-8569U), đã chạy thật và đạt:

- `dotnet build QuanLyThuVien.slnx` → 0 error.
- SQL Server 2022 Developer (`16.0.4236.2`) trong Docker, database `QuanLyThuVienDB` với 15 bảng sau khi chạy 7 script schema.
- API khởi động ở `http://localhost:5058`, `GET /api/dev/ping` trả 200.
- Bootstrap Admin thành công, `POST /api/auth/login` trả token, `GET /api/auth/me` và `GET /api/dev/db-check` trả 200.
- Tạo độc giả + tài khoản `DOC_GIA` qua API; `GET /api/docgia/me`, `GET /api/muontra/me` trả 200.
- Phân quyền đúng: độc giả gọi `/api/nhanvien` → 403; admin gọi `/api/muontra/me` → 403.
- Web chạy ở `http://localhost:5109`: `/`, `/sach`, `/sach/1`, `/dang-nhap` trả 200; đăng nhập `DOC_GIA` tạo cookie và mở được `/lich-su-muon`, `/tai-khoan`; đăng nhập Admin bị từ chối; `/sach/999` trả 404; `/lich-su-muon` khi chưa đăng nhập chuyển về `/dang-nhap`.

Chưa kiểm chứng: giao diện WinForms (cần Windows), seeder (cần backup hợp lệ).

Bổ sung ngày 2026-10-29 trên PC Windows 11:

- `dotnet build QuanLyThuVien.slnx` → 0 error; `dotnet test` → 30/30 pass.
- SQL Server 2022 trong Docker, database `QuanLyThuVienDB` có **16 bảng** sau khi chạy 8 script schema.
- Luồng yêu cầu khóa tài khoản đã chạy thật: Manager tạo/hủy, Admin duyệt/từ chối; duyệt xong tài khoản `Bị khóa`, thẻ `Khóa thẻ`, phiên bị thu hồi, token cũ trả `401`.
- Phân quyền: Manager gọi duyệt → `403`; Admin gọi hủy yêu cầu → `403`; tạo trùng yêu cầu đang chờ → `409`.
