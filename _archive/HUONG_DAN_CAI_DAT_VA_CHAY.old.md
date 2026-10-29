# Hướng dẫn cài đặt, chạy và sử dụng Hệ thống quản lý thư viện

Tài liệu này dành cho thành viên phát triển, người cài máy chủ và người sử dụng phần mềm Desktop. Thực hiện lần lượt từ trên xuống.

## 1. Mô hình của hệ thống

Hệ thống có ba thành phần sử dụng chung dữ liệu:

```text
Web độc giả ───────────┐
                      ├──► Web API ───► SQL Server
Desktop Admin ─────────┤
Desktop Thủ thư ───────┘
```

- Độc giả sử dụng website.
- Admin và Thủ thư/Manager sử dụng cùng một ứng dụng Windows Forms.
- Không có hai bộ cài Desktop riêng.
- Tài khoản đăng nhập quyết định giao diện và quyền được sử dụng.
- Web và Desktop không truy cập SQL Server trực tiếp; mọi thao tác nghiệp vụ đi qua Web API.

## 2. Trạng thái hiện tại của dự án

### Đã có

- Solution và các project nền tảng.
- Giao diện Web dành cho độc giả.
- Hai trang Web mẫu Admin và Manager dùng làm thiết kế tham khảo.
- Project WinForms có thể build và chạy.

### Chưa hoàn thiện

- Màn hình đăng nhập và phân quyền Desktop.
- Dashboard Admin và Thủ thư bằng WinForms.
- Entity Framework Core và kết nối SQL Server.
- API đăng nhập, sách, độc giả, mượn/trả và báo cáo.
- Tài khoản Admin/Thủ thư mặc định.
- Bộ cài phát hành cho máy người dùng.

> Hai URL `/Home/Admin` và `/Home/Manager` hiện chỉ là mẫu giao diện Web. Chúng không thay thế phần Desktop bắt buộc của đồ án. Sau khi chuyển thiết kế sang WinForms, hai trang này có thể được xóa khỏi Web.

## 3. Phần mềm cần cài trên máy phát triển

### Bắt buộc

1. **.NET 10 SDK**
   - Kiểm tra: `dotnet --version`
   - Kết quả cần bắt đầu bằng `10.`
2. **Git** để tải và đồng bộ mã nguồn.
3. Một trong hai môi trường:
   - **Visual Studio Community**: chọn workload `.NET desktop development` và `ASP.NET and web development`.
   - **Visual Studio Code**: cài extension `C# Dev Kit`.

### Database

1. Microsoft SQL Server.
2. SQL Server Management Studio hoặc Azure Data Studio.

> WinForms chỉ thiết kế và chạy đầy đủ trên Windows. Thành viên dùng macOS có thể làm Web, API, Domain, Application và kiểm thử.

## 4. Cấu trúc mã nguồn

```text
HeThongQuanLyThuVien/
|-- SourceCode/
|   |-- QuanLyThuVien.Domain/          Entity, enum, quy tắc nghiệp vụ lõi
|   |-- QuanLyThuVien.Application/     DTO, interface, validation, use case
|   |-- QuanLyThuVien.Infrastructure/  EF Core, SQL Server, repository
|   |-- QuanLyThuVien.Api/             API dùng chung cho Web và Desktop
|   |-- QuanLyThuVien.Web/             Website dành cho độc giả
|   `-- QuanLyThuVien.Desktop/         WinForms dành cho Admin và Thủ thư
|-- Tests/                             Unit test và integration test
|-- Database/                          Migration, script và dữ liệu mẫu
|-- Documents/                         Báo cáo, ERD, use case, wireframe
|-- QuanLyThuVien.sln                  File mở toàn bộ solution
`-- HeThongQuanLyThuVien.sql            Script database ban đầu
```

Quy tắc quan trọng:

- Không viết truy vấn database trực tiếp trong Form hoặc Controller.
- Quy tắc nghiệp vụ dùng chung đặt trong `Application`/`Domain`.
- Truy cập SQL Server đặt trong `Infrastructure`.
- Web và Desktop gọi chung API để tránh hai nơi xử lý nghiệp vụ khác nhau.

## 5. Tải, mở và build dự án

### Visual Studio

1. Mở `QuanLyThuVien.sln`.
2. Chờ Visual Studio restore package.
3. Nếu được hỏi độ tin cậy, chọn tin cậy vì đây là mã nguồn của nhóm.
4. Nhấn **Build > Build Solution**.

### Visual Studio Code

1. Chọn **File > Open Folder**.
2. Mở đúng thư mục `HeThongQuanLyThuVien`.
3. Mở Terminal và chạy:

```powershell
dotnet restore QuanLyThuVien.sln -m:1
dotnet build QuanLyThuVien.sln --no-restore -m:1
```

Khi dòng cuối là `Build succeeded`, môi trường đã sẵn sàng.

## 6. Cài đặt SQL Server trên máy chủ

Phần này thực hiện trên máy được chọn làm máy chủ.

1. Cài SQL Server và SQL Server Management Studio.
2. Mở SQL Server Management Studio và kết nối tới SQL Server.
3. Tạo database tên `QuanLyThuVien`.
4. Chọn database `QuanLyThuVien`.
5. Mở file `HeThongQuanLyThuVien.sql` và chạy script.
6. Kiểm tra các bảng đã được tạo: `TheLoai`, `TacGia`, `Sach`, `Sach_TacGia`, `DocGia`, `NhanVien`, `PhieuMuon`, `ChiTiet_PhieuMuon`.

Script hiện tại là bản thiết kế ban đầu. Trước khi phát hành cần bổ sung migration, constraint, dữ liệu mẫu và tài khoản đăng nhập đã băm mật khẩu.

## 7. Cấu hình và chạy Web API

API hiện chạy mặc định tại:

```text
http://localhost:5074
```

Chạy API:

```powershell
dotnet run --project SourceCode/QuanLyThuVien.Api --launch-profile http
```

Khi hoàn thiện EF Core, connection string phải được cấu hình trong `SourceCode/QuanLyThuVien.Api/appsettings.Development.json` hoặc User Secrets, ví dụ:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_MAY_CHU;Database=QuanLyThuVien;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Không commit mật khẩu SQL Server hoặc connection string thật lên Git.

## 8. Chạy website độc giả

Giữ API đang chạy và mở Terminal khác:

```powershell
dotnet run --project SourceCode/QuanLyThuVien.Web --launch-profile http
```

Website mặc định:

| Nội dung | Địa chỉ |
|---|---|
| Trang độc giả | `http://localhost:5109/` |
| Mẫu thiết kế Admin | `http://localhost:5109/Home/Admin` |
| Mẫu thiết kế Thủ thư | `http://localhost:5109/Home/Manager` |

Hai trang Admin và Manager trong bảng chỉ là mẫu để chuyển sang WinForms. Người dùng chính thức không quản trị hệ thống bằng hai URL này.

## 9. Chạy Desktop trong môi trường phát triển

Trên Windows, giữ API đang chạy rồi mở Terminal khác:

```powershell
dotnet run --project SourceCode/QuanLyThuVien.Desktop
```

Để kéo-thả giao diện, mở `QuanLyThuVien.sln` bằng Visual Studio, nhấp phải Form và chọn **View Designer**.

Khi phần gọi API được triển khai, Desktop cần file cấu hình tương tự:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5074"
  }
}
```

Không đặt connection string SQL Server trong Desktop.

## 10. Đăng nhập và phân quyền Desktop

Ứng dụng Desktop chỉ có một màn hình đăng nhập. API kiểm tra tài khoản và trả về vai trò.

```text
Đăng nhập
   |-- Vai trò Admin  ──► Dashboard Admin
   `-- Vai trò ThuThu ──► Dashboard Thủ thư
```

### Quyền Admin

- Quản lý tài khoản nhân viên.
- Phân quyền Admin/Thủ thư.
- Quản lý sách, tác giả và thể loại.
- Quản lý độc giả.
- Xem toàn bộ báo cáo.
- Thay đổi cấu hình nghiệp vụ.

### Quyền Thủ thư/Manager

- Quản lý kho sách.
- Quản lý độc giả.
- Xử lý yêu cầu đặt/mượn sách.
- Lập phiếu mượn và xác nhận trả sách.
- Theo dõi quá hạn và tiền phạt.
- Xem báo cáo nghiệp vụ được cho phép.
- Không được phân quyền hoặc sửa cấu hình quan trọng.

Ẩn menu trên giao diện là chưa đủ. API bắt buộc phải kiểm tra quyền cho từng endpoint.

### Tài khoản demo

Tài khoản demo chưa được tạo ở giai đoạn hiện tại. Khi triển khai xác thực, nhóm cần seed ít nhất:

| Vai trò | Tên đăng nhập đề xuất | Mật khẩu |
|---|---|---|
| Admin | `admin` | Thiết lập khi seed, không ghi mật khẩu thật vào Git |
| Thủ thư | `thuthu01` | Thiết lập khi seed, không ghi mật khẩu thật vào Git |

Mật khẩu phải được băm, không lưu dạng văn bản thuần trong database.

## 11. Cài nhiều máy Desktop trong mạng LAN

Ví dụ:

```text
Máy chủ SQL Server + API: 192.168.1.10
Máy Desktop Admin:         192.168.1.20
Máy Desktop Thủ thư 1:     192.168.1.21
Máy Desktop Thủ thư 2:     192.168.1.22
```

Trên máy chủ, chạy API lắng nghe mạng LAN:

```powershell
dotnet run --project SourceCode/QuanLyThuVien.Api --urls http://0.0.0.0:5074
```

Trên mọi máy Desktop, cấu hình:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://192.168.1.10:5074"
  }
}
```

Sau đó:

1. Cho phép cổng `5074` qua Windows Firewall trên máy chủ.
2. Kiểm tra các máy nằm cùng mạng LAN.
3. Từ máy Desktop, thử mở `http://192.168.1.10:5074`.
4. Không mở trực tiếp cổng SQL Server cho các máy Desktop nếu không cần thiết.
5. Tất cả máy đăng nhập bằng tài khoản riêng; không dùng chung một tài khoản Thủ thư.

Khi phát hành thật, ưu tiên HTTPS và không dùng HTTP qua mạng công cộng.

## 12. Cách dữ liệu đồng bộ giữa các máy

Các Desktop không kết nối trực tiếp với nhau. Tất cả cùng gọi API và dùng chung SQL Server.

Ví dụ luồng mượn sách:

1. Độc giả gửi yêu cầu trên Web.
2. Web gọi API và API lưu yêu cầu vào SQL Server.
3. Desktop Thủ thư tải danh sách yêu cầu từ API.
4. Thủ thư xử lý yêu cầu.
5. API tạo phiếu mượn và cập nhật số lượng sách trong một transaction.
6. Web độc giả và các Desktop tải lại sẽ thấy trạng thái mới.

Giai đoạn cơ bản có thể dùng nút **Làm mới** hoặc tải lại mỗi 10–30 giây. SignalR là chức năng mở rộng nếu nhóm còn thời gian.

API phải kiểm tra trạng thái/`RowVersion` để tránh hai Thủ thư cùng xử lý một yêu cầu.

## 13. Thứ tự chạy khi demo

1. Khởi động SQL Server.
2. Chạy `QuanLyThuVien.Api`.
3. Chạy `QuanLyThuVien.Web`.
4. Chạy một hoặc nhiều phiên bản `QuanLyThuVien.Desktop`.
5. Đăng nhập một Desktop bằng Admin.
6. Đăng nhập Desktop khác bằng Thủ thư.
7. Trên Web, độc giả tìm và gửi yêu cầu mượn sách.
8. Trên Desktop Thủ thư, tải và xử lý yêu cầu.
9. Trên Web, kiểm tra lịch sử mượn đã cập nhật.
10. Trên Desktop Admin, kiểm tra dashboard/báo cáo đã thay đổi.

Kịch bản trên chỉ thực hiện được đầy đủ sau khi API, database và xác thực hoàn thiện.

## 14. Build, test và phát hành

Build toàn bộ:

```powershell
dotnet build QuanLyThuVien.sln -m:1
```

Chạy kiểm thử:

```powershell
dotnet test QuanLyThuVien.sln -m:1
```

Publish Desktop cho Windows 64-bit:

```powershell
dotnet publish SourceCode/QuanLyThuVien.Desktop -c Release -r win-x64 --self-contained false
```

Publish API:

```powershell
dotnet publish SourceCode/QuanLyThuVien.Api -c Release
```

Publish Web:

```powershell
dotnet publish SourceCode/QuanLyThuVien.Web -c Release
```

Thư mục phát hành nằm trong `bin/Release/.../publish` của từng project. Bộ cài tự động chưa được cấu hình.

## 15. Lỗi thường gặp

### `dotnet` không được nhận diện

Cài .NET 10 SDK rồi đóng và mở lại Terminal.

### Port đang được sử dụng

Dừng phiên bản đang chạy bằng `Ctrl + C` hoặc chọn port khác:

```powershell
dotnet run --project SourceCode/QuanLyThuVien.Web --urls http://localhost:5200
```

### Không restore được NuGet

Kiểm tra Internet, proxy hoặc VPN rồi chạy:

```powershell
dotnet restore QuanLyThuVien.sln -m:1
```

### Build báo file đang được sử dụng

Dừng Web/API/Desktop đang chạy, sau đó build lại.

### Desktop không kết nối được API

1. Kiểm tra API có đang chạy không.
2. Kiểm tra `BaseUrl` và port.
3. Không dùng `localhost` khi API nằm trên máy khác.
4. Kiểm tra Windows Firewall và kết nối LAN.

### Desktop kết nối API nhưng không có dữ liệu

Kiểm tra connection string của API, trạng thái SQL Server, database được chọn và dữ liệu mẫu.

### VS Code không có WinForms Designer

VS Code không có WinForms Designer chính thức. Dùng Visual Studio Community trên Windows để kéo-thả giao diện.

## 16. Quy trình làm việc của nhóm

1. Pull code mới nhất trước khi bắt đầu.
2. Tạo branch riêng cho chức năng.
3. Không commit `bin`, `obj`, `.vs`, mật khẩu hoặc connection string thật.
4. Không chỉnh `Form.Designer.cs` của người khác khi chưa thống nhất.
5. Build và chạy luồng liên quan trước khi push.
6. Ghi rõ phần đã làm trong commit hoặc pull request.
7. Mỗi thành viên phải hiểu luồng Web/Desktop → API → SQL Server, không chỉ phần giao diện mình phụ trách.
