# Hệ thống quản lý thư viện

Hệ thống gồm một **REST API dùng chung**, một **ứng dụng Desktop WinForms cho quản trị/thủ thư** và một **Web ASP.NET Core MVC cho độc giả**. Cả ba dùng chung database SQL Server.

- Solution duy nhất: [`QuanLyThuVien.slnx`](QuanLyThuVien.slnx)
- Bản đồ vai trò, module và API: [`docs/MODULE_MAP.md`](docs/MODULE_MAP.md)
- Hướng dẫn cài đặt và xử lý lỗi: [`docs/SETUP_GUIDE.md`](docs/SETUP_GUIDE.md)

## 1. Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ / nền tảng | C#, .NET 10 |
| Backend | ASP.NET Core Web API |
| Desktop | WinForms (`net10.0-windows`) |
| Web | ASP.NET Core MVC |
| ORM / CSDL | EF Core 10, SQL Server 2022 Developer |
| Môi trường CSDL | Docker Compose |
| Test | xUnit (chưa có test case) |

## 2. Cấu trúc

```text
HeThongQuanLyThuVien/
├── QuanLyThuVien.slnx              # solution duy nhất
├── docker-compose.yml              # SQL Server 2022, host port 14333
├── Directory.Build.props           # cho phép build WinForms trên macOS/Linux
├── Database/
│   ├── AllTablesAndDemoData.sql    # schema + dữ liệu mẫu cho DB demo trống
│   └── seeds/                      # seeder C# opt-in
├── docs/                           # SETUP_GUIDE, MODULE_MAP, báo cáo
├── SourceCode/
│   ├── QuanLyThuVien.Domain        # entity, AccountRoles
│   ├── QuanLyThuVien.Application   # DTO, interface, service nghiệp vụ
│   ├── QuanLyThuVien.Infrastructure# DbContext, repository, SQL schema
│   ├── QuanLyThuVien.Api           # 56 endpoint, opaque bearer auth
│   ├── QuanLyThuVien.Desktop       # WinForms cho Admin + Thủ thư
│   └── QuanLyThuVien.Web           # MVC cho độc giả, gọi REST API
└── Tests/                          # UnitTests, IntegrationTests
```

Thư mục `_archive/` ở gốc repo chứa bản Desktop cũ và script SQL ban đầu, chỉ để tham khảo, **không** nằm trong solution.

## 3. Trạng thái từng phần

| Phần | Phụ trách | Trạng thái |
|---|---|---|
| API: sách, thể loại, tác giả, độc giả, nhân viên, tài khoản, mượn trả, phạt, dashboard, báo cáo | nhóm | Xong, đã kiểm chứng runtime |
| Desktop: đăng nhập, phân quyền, CRUD, mượn trả, báo cáo | nhóm | Xong, build được; chỉ chạy trên Windows |
| Web độc giả: trang chủ, danh mục, tìm kiếm (kèm gợi ý), chi tiết, đăng nhập, lịch sử mượn, phạt | **user** | Xong, đã kiểm chứng runtime với API + SQL Server thật |
| Gửi yêu cầu mượn/đặt sách trực tuyến (`YeuCauMuon`) | **user** | Xong: độc giả gửi/hủy, thủ thư duyệt/từ chối |
| Tự đăng ký độc giả trên Web (`/dang-ky`) | **user** | Xong: tạo thẻ + tài khoản `Hoạt động` ngay, tự đăng nhập |

Bộ dữ liệu demo trong `Database/AllTablesAndDemoData.sql` có **35 đầu sách hiển thị**,
chia đều **5 đầu sách cho mỗi thể loại** (7 thể loại); số bản của mỗi đầu sách nằm trong
khoảng **1–5**. Script còn giữ các ca kiểm thử còn 1 bản, hết bản, nhiều tác giả và sách đã ẩn.

## 4. Chạy nhanh

Yêu cầu: .NET 10 SDK, Docker Desktop (hoặc Docker Engine + compose).

```bash
cd HeThongQuanLyThuVien
cp .env.example .env          # đặt MSSQL_SA_PASSWORD mạnh
docker compose up -d sqlserver
docker compose ps

dotnet build QuanLyThuVien.slnx

dotnet user-secrets set "ConnectionStrings:LibraryDb" \
  "Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MAT_KHAU_SA>;Encrypt=True;TrustServerCertificate=True" \
  --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj

dotnet run --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj --launch-profile http
```

- API: `http://localhost:5058`
- Web: `http://localhost:5109`
- SQL Server: `127.0.0.1,14333`, database `QuanLyThuVienDB`

Web cần API đang chạy (mặc định đọc `ApiBaseUrl` = `http://localhost:5058/` trong `appsettings.json`). Trong terminal khác:

```bash
dotnet run --project SourceCode/QuanLyThuVien.Web/QuanLyThuVien.Web.csproj --launch-profile http
```

Trang độc giả: `/`, `/sach`, `/sach/{id}`, `/yeu-cau`, `/dang-nhap`, `/dang-ky`, `/lich-su-muon`, `/lich-su-muon/{id}`, `/tai-khoan`. Chỉ tài khoản vai trò `DOC_GIA` đăng nhập được trên Web; Admin/Thủ thư dùng Desktop.

Trên macOS không chạy được Desktop (WinForms). Để test nghiệp vụ Admin/Thủ thư song song với Web, dùng bộ công cụ trong [`scripts/dev`](scripts/dev/) — xem hướng dẫn [`docs/TESTING_ON_MACOS.md`](docs/TESTING_ON_MACOS.md):

```bash
cp scripts/dev/.env.dev.example scripts/dev/.env.dev   # điền ADMIN_PASS
./scripts/dev/staff.sh whoami
./scripts/dev/staff.sh overdue-loan 1 1 1 5            # lập phiếu quá hạn
./scripts/dev/staff.sh return-all 3                    # trả sách → xem phạt
```

Tạo database và chạy schema theo thứ tự trong [`docs/SETUP_GUIDE.md`](docs/SETUP_GUIDE.md). Desktop chỉ chạy được trên Windows:

```powershell
dotnet run --project SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

`dotnet build QuanLyThuVien.slnx` chạy được cả trên macOS/Linux nhờ `Directory.Build.props`, nhưng chỉ là compile-check cho Desktop.

## 5. Tài khoản demo

Không có mật khẩu demo nào được lưu trong repo. API tự tạo Admin đầu tiên khi database chưa có Admin và đủ ba cấu hình `Authentication:BootstrapAdmin:*` — xem [SETUP_GUIDE](docs/SETUP_GUIDE.md#3-cấu-hình-api-và-admin-khởi-tạo).

Ghi thông tin đăng nhập demo vào `docs/DEMO_ACCOUNTS.local.md` (đã gitignore). Mẫu: [`docs/DEMO_ACCOUNTS.example.md`](docs/DEMO_ACCOUNTS.example.md).

## 6. Dữ liệu mẫu

- Schema: 7 file trong `SourceCode/QuanLyThuVien.Infrastructure/Data/Sql/`, chạy theo thứ tự trong SETUP_GUIDE.
- Gộp schema + dữ liệu mẫu cho database trống: [`Database/AllTablesAndDemoData.sql`](Database/AllTablesAndDemoData.sql), chạy ở **SQLCMD Mode**.
- Seeder C# có kiểm tra backup: [`Database/seeds/README.md`](Database/seeds/README.md).

Không chạy script gộp hay seeder lên database đang có dữ liệu cần giữ.
