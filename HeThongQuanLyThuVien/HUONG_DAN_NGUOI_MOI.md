# Hướng dẫn cho người mới: mở và xem thử giao diện Web + Desktop

Tài liệu này dành cho người **chưa biết gì** về dự án. Làm lần lượt từ trên xuống, copy từng khối lệnh dán vào PowerShell. Làm xong bạn sẽ có:

- Một **trang web** cho độc giả (mở bằng trình duyệt).
- Một **ứng dụng Desktop** cho Admin và Thủ thư (cửa sổ Windows).
- Một **cơ sở dữ liệu** có sẵn khoảng 36 cuốn sách mẫu để xem thử.

> Chỉ chạy được trên **Windows 10/11** (ứng dụng Desktop là WinForms). Lần đầu mất khoảng 20 đến 30 phút, chủ yếu để tải phần mềm.

---

## Bước 0. Cách mở PowerShell

1. Nhấn phím **Windows**, gõ `PowerShell`, nhấn **Enter**. Một cửa sổ màu xanh/đen hiện ra.
2. Mỗi khối lệnh trong tài liệu này: **copy cả khối → bấm chuột phải vào cửa sổ PowerShell để dán → nhấn Enter**.
3. Chờ lệnh chạy xong (dòng nhập lệnh mới xuất hiện lại) rồi mới làm bước kế tiếp.

---

## Bước 1. Cài 3 phần mềm (chỉ làm một lần)

| Phần mềm | Dùng để làm gì | Tải ở đâu |
|---|---|---|
| **Git** | Tải mã nguồn về máy | https://git-scm.com/download/win |
| **.NET 10 SDK** | Chạy API, Web, Desktop | https://dotnet.microsoft.com/download/dotnet/10.0 |
| **Docker Desktop** | Chạy SQL Server (cơ sở dữ liệu) | https://www.docker.com/products/docker-desktop/ |

Cài xong, **mở Docker Desktop** và chờ đến khi nó báo **Engine running** (cá voi màu xanh). Sau đó **đóng PowerShell cũ, mở lại** và kiểm tra:

```powershell
git --version
dotnet --version
docker --version
```

Cả ba lệnh phải in ra số phiên bản. Lệnh `dotnet --version` phải bắt đầu bằng `10.`. Nếu báo "không nhận ra lệnh" thì khởi động lại máy rồi thử lại.

---

## Bước 2. Tải mã nguồn

```powershell
cd $HOME\Documents
git clone https://github.com/ZudaNguyen/NNLTCSHARP.git
cd NNLTCSHARP
git switch tuan-ui-fixes
cd HeThongQuanLyThuVien
```

- `tuan-ui-fixes` là nhánh hiện có đầy đủ nhất (Web, API, Desktop, kèm vài sửa lỗi giao diện). Nếu nhóm đã có nhánh mới hơn, hỏi nhóm tên nhánh rồi thay vào lệnh `git switch`.
- Từ đây **mọi lệnh đều chạy trong thư mục `HeThongQuanLyThuVien`**. Nếu lỡ đóng cửa sổ PowerShell, mở lại và chạy `cd $HOME\Documents\NNLTCSHARP\HeThongQuanLyThuVien`.

---

## Bước 3. Bật SQL Server (cơ sở dữ liệu)

Tạo file cấu hình mật khẩu cho SQL Server:

```powershell
Copy-Item .env.example .env
notepad .env
```

Trong Notepad, sửa dòng `MSSQL_SA_PASSWORD=...` thành mật khẩu **do bạn tự đặt**, rồi bấm **Ctrl+S** và đóng Notepad. Ví dụ:

```text
MSSQL_SA_PASSWORD=MatKhau-SQL-Cua-Toi-2026!
```

Quy tắc mật khẩu: **ít nhất 8 ký tự, có chữ HOA, chữ thường, số và ký tự đặc biệt** (nếu không, SQL Server sẽ tự tắt ngay). **Hãy nhớ mật khẩu này**, các bước sau còn dùng. Đừng chia sẻ file `.env` cho ai.

Bật SQL Server:

```powershell
docker compose up -d sqlserver
docker compose ps
```

Lần đầu Docker sẽ tải khoảng 2 GB nên mất vài phút. Dòng cuối phải hiện `qltv-sqlserver ... Up`.

---

## Bước 4. Tạo bảng và dữ liệu mẫu

Thay `<MẬT KHẨU SQL CỦA BẠN>` bằng mật khẩu bạn đặt ở bước 3 (giữ nguyên hai dấu nháy kép):

```powershell
$env:SA = "<MẬT KHẨU SQL CỦA BẠN>"
docker cp .\Database\AllTablesAndDemoData.sql qltv-sqlserver:/tmp/all.sql
docker exec -e "SQLCMDPASSWORD=$env:SA" qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -b -f 65001 -i /tmp/all.sql
```

Chờ khoảng 10 đến 30 giây. Màn hình sẽ in ra một danh sách sách mẫu (chữ `DEMO - ...`), đó là **thành công**.

Kiểm tra lại (phải ra số `16`):

```powershell
docker exec -e "SQLCMDPASSWORD=$env:SA" qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -d QuanLyThuVienDB -Q "SELECT COUNT(*) AS so_bang FROM sys.tables"
```

> Nếu thấy `Application tables already exist` nghĩa là database đã được tạo từ trước. Đó là bình thường, bỏ qua và làm tiếp. Script này **không bao giờ xóa dữ liệu**.

Dữ liệu mẫu có sẵn: 36 sách, 8 thể loại, 13 tác giả, 1 độc giả mẫu, 1 nhân viên mẫu (Mã NV 1), 1 phiếu mượn đang quá hạn. **Chưa có tài khoản đăng nhập nào**, bước sau sẽ tạo Admin.

---

## Bước 5. Cấu hình cho API (mật khẩu DB và tài khoản Admin)

Các lệnh này lưu bí mật vào máy bạn (không đưa lên Git). Thay hai chỗ có `< >`:

- `<MẬT KHẨU SQL>`: mật khẩu ở bước 3.
- `<MẬT KHẨU ADMIN>`: mật khẩu bạn muốn dùng cho tài khoản **admin**. Phải **từ 12 ký tự trở lên, có chữ hoa, chữ thường, số và ký tự đặc biệt**. Ví dụ: `Admin-Thu-Vien-2026!`. **Đừng dùng dấu `$` hoặc dấu huyền (`) trong mật khẩu.**

```powershell
$api = ".\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj"
dotnet user-secrets set "ConnectionStrings:LibraryDb" 'Server=127.0.0.1,14333;Database=QuanLyThuVienDB;User Id=sa;Password=<MẬT KHẨU SQL>;Encrypt=True;TrustServerCertificate=True' --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:UserName" "admin" --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:DisplayName" "Quan tri vien" --project $api
dotnet user-secrets set "Authentication:BootstrapAdmin:Password" '<MẬT KHẨU ADMIN>' --project $api
```

(Chú ý: dùng dấu nháy **đơn** `'...'` ở dòng có mật khẩu như trên.) Mỗi lệnh phải in `Successfully saved ...`.

**Ghi lại**: tài khoản Admin là `admin` và mật khẩu bạn vừa đặt.

---

## Bước 6. Chạy 3 chương trình (mở 3 cửa sổ PowerShell)

Thứ tự bắt buộc: **API → Web → Desktop**. Mỗi chương trình dùng **một cửa sổ PowerShell riêng**; mỗi cửa sổ mới đều phải `cd` vào thư mục `HeThongQuanLyThuVien` trước.

### Cửa sổ 1: API

```powershell
cd $HOME\Documents\NNLTCSHARP\HeThongQuanLyThuVien
dotnet run --project .\SourceCode\QuanLyThuVien.Api\QuanLyThuVien.Api.csproj --launch-profile http
```

Lần đầu sẽ tải thư viện và biên dịch nên hơi lâu. Đợi tới khi thấy dòng **`Now listening on: http://localhost:5058`**. Kiểm tra bằng cách mở trình duyệt vào http://localhost:5058/api/dev/ping, phải thấy `{"message":"API dang hoat dong"}`. **Đừng đóng cửa sổ này.**

### Cửa sổ 2: Web

```powershell
cd $HOME\Documents\NNLTCSHARP\HeThongQuanLyThuVien
dotnet run --project .\SourceCode\QuanLyThuVien.Web\QuanLyThuVien.Web.csproj --launch-profile http
```

Khi thấy `Now listening on: http://localhost:5109`, mở trình duyệt vào **http://localhost:5109**. Bạn sẽ thấy trang chủ "Thư Viện" với các sách mẫu. **Đừng đóng cửa sổ này.**

### Cửa sổ 3: Desktop

```powershell
cd $HOME\Documents\NNLTCSHARP\HeThongQuanLyThuVien
dotnet run --project .\SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

Một cửa sổ **Đăng nhập** hiện ra. Đăng nhập bằng `admin` và mật khẩu Admin bạn đặt ở bước 5.

---

## Bước 7. Tạo thêm tài khoản để xem giao diện Thủ thư và Độc giả

Hệ thống có 3 vai trò, mỗi vai trò thấy một giao diện khác nhau:

| Vai trò | Dùng ở đâu | Tạo thế nào |
|---|---|---|
| **Admin** | Desktop | Đã có từ bước 5 |
| **Thủ thư** (nhân viên) | Desktop | Admin tạo trong Desktop (bên dưới) |
| **Độc giả** | Web | Tự đăng ký trên Web (bên dưới) |

### Tạo tài khoản Thủ thư

1. Mở Desktop, đăng nhập `admin`.
2. Bấm mục **Tài khoản** ở thanh bên trái.
3. Điền: **Tên đăng nhập** `thuthu.test`, **Tên hiển thị** `Thu thu test`, **Vai trò** `THU_THU`, **Mã nhân viên** `1`, **Mật khẩu mới** (12 ký tự trở lên, có hoa, thường, số, ký tự đặc biệt).
4. Bấm **Tạo tài khoản**.
5. Bấm **Đăng xuất** (góc dưới thanh bên trái) và đăng nhập lại bằng `thuthu.test`.

> Tài khoản Thủ thư bắt buộc gắn với một nhân viên đang hoạt động. Dữ liệu mẫu có sẵn nhân viên **Mã 1**, nên cứ nhập `1`.

### Tạo tài khoản Độc giả

1. Mở http://localhost:5109/dang-ky.
2. Điền đầy đủ. Mật khẩu từ 12 ký tự, có chữ hoa, thường, số, ký tự đặc biệt. Số điện thoại bắt đầu bằng `0` và có 10 hoặc 11 số. Email phải đúng dạng `ten@example.com`.
3. Bấm **Tạo tài khoản**, sau đó đăng nhập ở http://localhost:5109/dang-nhap.

---

## Bước 8. Nên xem những gì

**Web (độc giả)** tại http://localhost:5109

- Trang chủ, **Tìm kiếm sách** (`/sach`): gõ từ khóa, bấm thể loại.
- **Chi tiết sách** (bấm "Xem chi tiết"): xem số lượng còn.
- Sau khi đăng nhập độc giả: **Yêu cầu mượn** (`/yeu-cau`), **Lịch sử mượn trả** (`/lich-su-muon`), **Thông tin cá nhân** (`/tai-khoan`).
- Thử thu nhỏ cửa sổ trình duyệt thật hẹp (như điện thoại): giao diện tự co lại và hiện nút menu ☰.

**Desktop, đăng nhập Admin**: 3 mục **Duyệt khóa tài khoản**, **Nhân viên**, **Tài khoản & Phân quyền**.

**Desktop, đăng nhập Thủ thư**: **Tổng quan** (số liệu), **Kho sách**, **Tác giả & Thể loại**, **Độc giả**, **Mượn / Trả sách**, **Duyệt yêu cầu mượn**, **Yêu cầu khóa tài khoản**, **Báo cáo & Thống kê**.

---

## Hằng ngày: bật và tắt

**Bật lại:** mở Docker Desktop, chờ Engine running, rồi chạy lần lượt Bước 6 (API → Web → Desktop). SQL Server tự bật cùng Docker, và dữ liệu vẫn còn.

**Tắt:** trong cửa sổ API và cửa sổ Web bấm **Ctrl+C**, đóng Desktop, rồi (không bắt buộc) tắt SQL Server:

```powershell
docker compose stop
```

---

## Gặp lỗi? Tra bảng này

| Triệu chứng | Nguyên nhân và cách xử lý |
|---|---|
| `cannot connect to the Docker daemon` hoặc `error during connect` | Docker Desktop chưa chạy. Mở Docker Desktop, chờ **Engine running**, làm lại. |
| SQL Server vừa bật đã tắt, `docker compose ps` không thấy `Up` | Mật khẩu SQL ở `.env` quá yếu. Đặt lại mật khẩu mạnh hơn rồi `docker compose up -d sqlserver`. Xem nguyên nhân bằng `docker compose logs sqlserver`. |
| Desktop báo "Hãy kiểm tra API đang chạy tại localhost:5058" hoặc Web không hiện sách | API chưa chạy. Mở cửa sổ 1 và chạy lại API (Bước 6). |
| Cửa sổ API báo lỗi `Login failed for user 'sa'` | Mật khẩu SQL trong Bước 5 khác mật khẩu ở `.env`. Chạy lại lệnh `user-secrets set "ConnectionStrings:LibraryDb"` với đúng mật khẩu. |
| Báo lỗi 500, trong cửa sổ API có `Invalid object name 'YeuCauMuon'` hoặc `'YeuCauKhoaTaiKhoan'`, hoặc `Invalid column name 'SoLanGiaHan'` | Database tạo từ bản cũ, thiếu bảng hoặc cột mới. Chạy lệnh **nâng cấp** ngay bên dưới bảng này. |
| Chạy script ở Bước 4 báo `Application tables already exist` | Bình thường, database đã có sẵn. Bỏ qua. |
| `Address already in use` (cổng 5058 hoặc 5109 đang bị dùng) | Có chương trình cũ chưa tắt. Chạy `netstat -ano \| findstr :5058`, lấy số PID ở cột cuối, rồi `taskkill /PID <số> /F`. |
| Đăng nhập `admin` báo sai mật khẩu | Tài khoản Admin chỉ được tạo **một lần**, ở lần chạy API đầu tiên, theo mật khẩu lúc đó. Sửa mật khẩu trong `user-secrets` sau này **không** đổi mật khẩu đã tạo. Muốn làm lại từ đầu, xem mục "Làm lại từ đầu". |
| `dotnet` hoặc `docker` "không nhận ra lệnh" | Cài chưa xong hoặc chưa khởi động lại. Đóng PowerShell, mở lại, hoặc khởi động lại máy. |
| Không tải được thư viện khi chạy lần đầu | Cần có mạng Internet ở lần chạy đầu tiên. |

### Lệnh nâng cấp database cũ (khi gặp lỗi `Invalid object name` / `Invalid column name`)

Các script này **chỉ thêm bảng/cột còn thiếu, không xóa dữ liệu**. Vẫn nên sao lưu trước nếu database có dữ liệu quan trọng.

```powershell
$env:SA = "<MẬT KHẨU SQL CỦA BẠN>"
foreach ($f in 'BorrowingModuleSchema.sql','BorrowRequestModuleSchema.sql','AccountLockRequestModuleSchema.sql') {
  docker cp ".\SourceCode\QuanLyThuVien.Infrastructure\Data\Sql\$f" "qltv-sqlserver:/tmp/$f"
  docker exec -e "SQLCMDPASSWORD=$env:SA" qltv-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -d QuanLyThuVienDB -b -f 65001 -i "/tmp/$f"
}
```

Xong, tắt rồi chạy lại API.

### Làm lại từ đầu (XÓA SẠCH dữ liệu)

Chỉ dùng khi muốn bỏ hết dữ liệu và tạo lại như mới. **Lệnh này xóa toàn bộ database trong Docker.**

```powershell
docker compose down -v
docker compose up -d sqlserver
```

Sau đó làm lại từ **Bước 4**.

---

## Ghi chú cho người sửa code

- Không đưa file `.env` và mật khẩu lên Git (đã được bỏ qua sẵn).
- Tài liệu chi tiết hơn: [README.md](README.md), [docs/SETUP_GUIDE.md](docs/SETUP_GUIDE.md), [docs/RUN_SERVERS.md](docs/RUN_SERVERS.md).
- Mật khẩu trong tài liệu này chỉ là **ví dụ**. Hãy tự đặt mật khẩu riêng, đừng dùng lại mật khẩu ví dụ.
