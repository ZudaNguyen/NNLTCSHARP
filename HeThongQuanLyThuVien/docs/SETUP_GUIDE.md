# Hướng dẫn cài đặt và chạy

Hướng dẫn này dành cho Windows 10/11, PowerShell, Docker Desktop, .NET 10 SDK và SSMS. Lệnh schema có thể thay đổi dữ liệu; hãy sao lưu trước và chỉ chạy trên database bạn đã chọn.

## 1. Chuẩn bị

Mở PowerShell tại thư mục gốc repo:

```powershell
Set-Location .\HeThongQuanLyThuVien
dotnet --info
docker version
```

Khởi động Docker Desktop. `.env.example` chỉ chứa placeholder. Tạo file cấu hình cục bộ và thay bằng một mật khẩu SA riêng, mạnh:

```powershell
Copy-Item .env.example .env
notepad .env
docker compose up -d sqlserver
docker compose ps
docker compose logs --tail 80 sqlserver
```

Compose ánh xame cổng host `127.0.0.1:14333` tới SQL Server `1433`; database lưu trong volume `qltv_sql_data`. Trong SSMS, kết nối `127.0.0.1,14333` bằng SQL Login `sa` và mật khẩu trong `.env`. Không dùng `localhost\SQLEXPRESS` cho container này. Không chạy `docker compose down -v`, `docker volume rm` hoặc thao tác xóa volume.

## 2. Database và schema

Mặc định dự án trông đợi database `QuanLyThuVienDB`, nhưng các file schema nằm trong `SourceCode/QuanLyThuVien.Infrastructure/Data/Sql` và không phải EF migrations. Chúng tạo/cập nhật bảng; `BooksSchema.sql` có cập nhật các dòng sách legacy và thay đổi ràng buộc. Vì vậy, không chạy mù toàn bộ scripts trên database cần giữ.

Trước khi áp dụng:

1. Xác nhận trong SSMS đang kết nối đúng server/database; xem `SELECT @@SERVERNAME, DB_NAME();`.
2. Tạo và kiểm tra full backup theo quy trình vận hành của bạn. Không dựa vào volume Docker như bản backup.
3. Đối chiếu bảng/cột/FK hiện có với từng script và `LibraryDbContext`; ghi nhận schema khác nhau trước khi áp dụng.
4. Chỉ trên môi trường demo/kiểm thử đã duyệt, tạo database `QuanLyThuVienDB` nếu chưa tồn tại; mở từng file trong SSMS, xác nhận database context rồi thực thi theo phụ thuộc: `BooksSchema.sql`, `CatalogModuleSchema.sql`, `ReaderModuleSchema.sql`, `EmployeeModuleSchema.sql`, `AuthenticationModuleSchema.sql`, `BorrowingModuleSchema.sql`. Dừng nếu có lỗi hoặc dữ liệu legacy không khớp; không chạy lại tùy tiện.
5. Kiểm tra bảng và foreign key sau khi chạy. Những file SQL không tạo database; việc tạo database mới cần người vận hành thực hiện rõ ràng trong môi trường đã xác nhận.

Schema scripts không được chạy trong lượt bàn giao này. Nếu database đã có dữ liệu, nên khôi phục backup vào database test riêng và kiểm thử schema tại đó trước.

Nếu cần một file tổng hợp cho database demo trống, dùng [`database/AllTablesAndDemoData.sql`](../database/AllTablesAndDemoData.sql). Trong SSMS bật **Query → SQLCMD Mode** trước khi chạy (script dùng `:ON ERROR EXIT` để dừng khi preflight/schema có lỗi). File có thể tạo `QuanLyThuVienDB` nếu chưa có, không xóa bảng/database, và từ chối chạy nếu phát hiện bảng nghiệp vụ đã tồn tại. Nó thêm bộ mẫu nhỏ gồm một phiếu mượn quá hạn; SQL ghi trực tiếp nên chỉ dùng ở database demo. Với database hiện có, không chạy file tổng hợp; dùng C# DemoSeeder theo hướng dẫn bên dưới sau khi thỏa điều kiện backup.

## 3. Cấu hình API và Admin khởi tạo

Connection string được lấy từ User Secrets hoặc biến môi trường, không từ `appsettings.json`. Cấu hình User Secrets cho project API:

```powershell
$api = ".\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj"
dotnet user-secrets set "ConnectionStrings:LibraryDb" "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MAT_KHAU_SA>;Encrypt=True;TrustServerCertificate=True" --project $api
```

Thay `<MAT_KHAU_SA>` bằng giá trị cục bộ trong `.env`; không lưu lệnh chứa mật khẩu vào script/log/repository. Có thể thay User Secrets bằng biến `ConnectionStrings__LibraryDb` trong phiên PowerShell riêng.

Khi database chưa có bất kỳ Admin nào, API có thể tạo một tài khoản bootstrap nếu cấu hình đủ:

```powershell
dotnet user-secrets set "Authentication:BootstrapAdmin:UserName" "<TEN_DANG_NHAP_ADMIN>" --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:DisplayName" "<TEN_HIEN_THI>" --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:Password" "<MAT_KHAU_MẠNH>" --project $api
```

Chọn mật khẩu dài, ngẫu nhiên và riêng cho môi trường demo. Bootstrap không đặt lại mật khẩu hoặc thay đổi tài khoản Admin hiện có. Sau khi đăng nhập kiểm chứng, ghi nhận thông tin vào file cục bộ được gitignore theo [mẫu](DEMO_ACCOUNTS.example.md); nếu không kiểm chứng được thì để trạng thái chưa xác minh. Không tự tạo tài khoản Thủ thư/Độc giả bằng SQL.

## 4. Chạy API, Web và Desktop

API terminal:

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project .\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

Profile `http` nghe tại `http://localhost:5058`. Trong Development, kiểm tra ping bằng `Invoke-RestMethod http://localhost:5058/api/dev/ping`. `/api/dev/db-check` yêu cầu Bearer auth. Chỉ dùng endpoint dev trên máy local.

Web hiện là khung ASP.NET Core MVC tại cổng HTTP `5109`; chưa xác nhận các trang nghiệp vụ tích hợp với API. Chạy bằng:

```powershell
dotnet run --project .\SourceCode\QuanLyThuVien.Web\QuanLyThuVien.Web.csproj --launch-profile http
```

Desktop cần API đang chạy và endpoint API theo cấu hình app. Chạy trong terminal khác:

```powershell
dotnet run --project .\SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

## 5. Seed demo

Seeder chỉ chạy trong Development, giới hạn SQL Server ở `127.0.0.1,14333` và DB `QuanLyThuVienDB`, yêu cầu cấu hình phạt cùng full backup không quá 7 ngày và `RESTORE VERIFYONLY` thành công. Chạy:

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project .\database\seeds\QuanLyThuVien.DemoSeeder.csproj
```

Seeder idempotent theo khóa demo, không xóa dữ liệu, không tự khôi phục bản ghi đã ẩn và không tạo thông tin đăng nhập. Xem [chi tiết seed](../database/seeds/README.md). Không bỏ qua kiểm tra backup để seed vào dữ liệu cần giữ.

## 6. Xử lý lỗi thường gặp

| Triệu chứng | Kiểm tra |
|---|---|
| Docker pipe/engine permission denied | Mở Docker Desktop, chờ engine sẵn sàng, rồi chạy `docker version` và `docker compose ps`. |
| Cổng 14333 bị chiếm | Kiểm tra tiến trình/listener Windows; không đổi cổng nếu chưa cập nhật đồng bộ connection string và SSMS. |
| Login SQL thất bại | Đảm bảo `.env` có mật khẩu mạnh hợp lệ và khớp connection string; Compose không tự sửa mật khẩu cho volume đã khởi tạo. Không xóa volume để thử lại. |
| API chạy nhưng nghiệp vụ 503 | Kiểm tra User Secrets project API có `ConnectionStrings:LibraryDb`; kiểm tra SSMS server/database và trạng thái SQL. |
| `db-check` 401 | Endpoint yêu cầu Bearer token đăng nhập hợp lệ; ping không kiểm tra DB. |
| Seeder từ chối chạy | Kiểm tra Development, host/DB giới hạn, `CauHinhTienPhat`, backup gần nhất và quyền `RESTORE VERIFYONLY`; không tắt guard. |
| Schema lỗi / constraint | Dừng thực thi, lưu thông báo lỗi, so sánh schema và khôi phục quy trình từ backup. Không chạy script sửa/xóa tùy tiện. |

## 7. Lệnh build

```powershell
dotnet build .\QuanLyThuVien.slnx
dotnet build .\database\seeds\QuanLyThuVien.DemoSeeder.csproj
```

Build không xác nhận kết nối database, đăng nhập, CRUD runtime hay hiển thị giao diện. Kết quả bàn giao hiện tại nằm trong [báo cáo kiểm thử](FINAL_TEST_REPORT.md).
