# Hệ thống quản lý thư viện

Ứng dụng quản lý thư viện gồm REST API dùng chung cho WinForms và Web. Solution chính: `QuanLyThuVien.slnx`; nhánh bàn giao: `feat/backend-nhan`.

## 1. Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ / nền tảng | C#, .NET 10 |
| Backend | ASP.NET Core Web API |
| Desktop | WinForms |
| Web | ASP.NET Core MVC (project Web hiện là khung MVC, chưa xác nhận các luồng nghiệp vụ chạy end-to-end) |
| ORM / CSDL | EF Core, SQL Server 2022 Developer |
| Môi trường CSDL | Docker Compose |

## 2. Chức năng

Quản lý sách, thể loại, tác giả, độc giả, nhân viên, tài khoản và phân quyền; lập phiếu mượn, nhận trả, tính/thu phạt; dashboard và báo cáo. API có thể chạy ping khi chưa cấu hình database, nhưng API nghiệp vụ cần `ConnectionStrings:LibraryDb`. Xem báo cáo để biết phần nào đã build, phần nào chưa kiểm chứng runtime.

## 3. Chạy nhanh trên Windows PowerShell

```powershell
Set-Location .\HeThongQuanLyThuVien
Copy-Item .env.example .env
# Mở .env và đặt MSSQL_SA_PASSWORD thành mật khẩu riêng, mạnh.
docker compose up -d sqlserver
dotnet user-secrets set "ConnectionStrings:LibraryDb" "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MAT_KHAU_SA>;Encrypt=True;TrustServerCertificate=True" --project .\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj
# Nếu cần tạo Admin đầu tiên, xem hướng dẫn trong docs/SETUP_GUIDE.md.
# Kiểm tra và áp dụng schema có chủ đích theo docs/SETUP_GUIDE.md; sao lưu trước.
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project .\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

API HTTP chạy tại `http://localhost:5058`. Mở cửa sổ PowerShell khác để chạy Desktop:

```powershell
Set-Location .\HeThongQuanLyThuVien
dotnet run --project .\SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

Compose giữ dữ liệu trong volume `qltv_sql_data`. Không xóa volume để xử lý lỗi thông thường. Xem [Hướng dẫn cài đặt](docs/SETUP_GUIDE.md) trước khi áp dụng SQL vào database đang có.

## 4. Tài khoản demo

Không có mật khẩu demo nào được xác minh trong lượt bàn giao này. API chỉ tạo Admin bootstrap khi database chưa có tài khoản Admin và đủ ba cấu hình bootstrap. Không tự dùng tài khoản/mật khẩu phỏng đoán.

| Quyền | Tài khoản | Mật khẩu |
|---|---|---|
| Admin | Chưa xác minh | Chưa xác minh |
| Thủ thư | Chưa xác minh | Chưa xác minh |
| Độc giả | Chưa xác minh | Chưa xác minh |

Sau khi tạo và đăng nhập xác nhận tài khoản trên môi trường demo, lưu thông tin nội bộ tại `docs/DEMO_ACCOUNTS.local.md` (file được gitignore); không đưa mật khẩu vào README, source, log hoặc Git. Xem [mẫu ghi nhận](docs/DEMO_ACCOUNTS.example.md) và [cách bootstrap an toàn](docs/SETUP_GUIDE.md#tạo-tài-khoản-admin-đầu-tiên).

## 5. Seed dữ liệu demo

Seeder opt-in, chạy riêng và có kiểm tra an toàn; không tự chạy cùng API. Đọc [database/seeds/README.md](database/seeds/README.md) trước. Seeder yêu cầu Developer DB cục bộ, cấu hình tiền phạt hợp lệ và full backup gần đây đã verify; nó không tạo tài khoản đăng nhập.

Muốn một file SQL gộp schema và dữ liệu mẫu cho **database demo trống**, xem [AllTablesAndDemoData.sql](database/AllTablesAndDemoData.sql). Chạy bằng SSMS ở **SQLCMD Mode**; file từ chối chạy khi đã có bảng nghiệp vụ. Không dùng nó để cập nhật database hiện có.

## 6. Tài liệu bàn giao

- [Hướng dẫn cài đặt, schema và xử lý lỗi](docs/SETUP_GUIDE.md)
- [Seeder và dữ liệu demo](database/seeds/README.md)
- [Mẫu tài khoản demo](docs/DEMO_ACCOUNTS.example.md)
- [Báo cáo kiểm thử cuối](docs/FINAL_TEST_REPORT.md)
- [Checklist bàn giao](docs/HANDOVER_CHECKLIST.md)
