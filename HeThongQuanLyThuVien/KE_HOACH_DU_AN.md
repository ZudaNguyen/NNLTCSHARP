# Kế hoạch dự án Hệ thống quản lý thư viện

## 1. Kết luận từ tài liệu đề bài

- Đề tài: **Xây dựng hệ thống quản lý thư viện** (đề tài số 22).
- Sản phẩm bắt buộc gồm hai ứng dụng:
  - Web dành cho độc giả.
  - Desktop Windows Forms dành cho nhân viên/quản trị.
- Hai ứng dụng phải dùng chung dữ liệu nghiệp vụ trên Microsoft SQL Server.
- Desktop bắt buộc có đăng nhập, phân quyền, xem/thêm/sửa/xóa/tìm kiếm và quản lý nghiệp vụ.
- Web bắt buộc có trang chủ, menu, danh mục, tìm kiếm, trang chi tiết và nghiệp vụ chính; nên responsive.
- Báo cáo phải có phân tích, thiết kế giao diện, lược đồ CSDL, kết quả, kết luận và tài liệu tham khảo.

## 2. Phạm vi MVP

### Web độc giả

1. Trang chủ và sách mới/nổi bật.
2. Danh sách sách theo thể loại.
3. Tìm kiếm theo tên sách, ISBN, tác giả.
4. Xem chi tiết sách, tác giả và số lượng còn.
5. Đăng nhập độc giả.
6. Xem lịch sử mượn, hạn trả và tiền phạt.
7. Gửi yêu cầu mượn/đặt sách (nên có để Web có nghiệp vụ chính rõ ràng).

### Desktop quản trị

1. Đăng nhập và phân quyền Quản trị viên/Nhân viên.
2. Dashboard: tổng sách, sách đang mượn, quá hạn, độc giả hoạt động.
3. Quản lý thể loại, tác giả và sách.
4. Quản lý độc giả và nhân viên.
5. Lập phiếu mượn, xác nhận trả sách, tính tiền phạt.
6. Tìm kiếm, lọc và kiểm tra dữ liệu đầu vào.
7. Báo cáo sách đang mượn/quá hạn; xuất báo cáo là chức năng mở rộng.

## 3. Kiến trúc đề xuất

```text
QuanLyThuVien.sln
|-- SourceCode
|   |-- QuanLyThuVien.Domain          (entity, enum, quy tắc cốt lõi)
|   |-- QuanLyThuVien.Application     (DTO, interface, use case, validation)
|   |-- QuanLyThuVien.Infrastructure  (EF Core, SQL Server, repository)
|   |-- QuanLyThuVien.Api             (Web API, xác thực, DI)
|   |-- QuanLyThuVien.Web             (ASP.NET Core MVC, responsive)
|   `-- QuanLyThuVien.Desktop         (Windows Forms)
`-- Tests
    |-- QuanLyThuVien.UnitTests
    `-- QuanLyThuVien.IntegrationTests
```

Luồng dữ liệu ưu tiên: `Web/Desktop -> Web API -> Application -> Infrastructure -> SQL Server`.
Cách này giúp hai giao diện dùng chung toàn bộ nghiệp vụ, dễ demo sự đồng bộ dữ liệu và đáp ứng phần điểm kiến trúc/Web API/DI. Nếu thời gian quá hạn chế, Web và Desktop có thể dùng chung `Application` + `Infrastructure`, nhưng không nên viết truy vấn riêng ở từng giao diện.

## 4. Rà soát database hiện tại

Script hiện có 8 bảng: `TheLoai`, `TacGia`, `Sach`, `Sach_TacGia`, `DocGia`, `NhanVien`, `PhieuMuon`, `ChiTiet_PhieuMuon`. Các quan hệ cơ bản hợp lý và gần chuẩn 3NF, nhưng cần hoàn thiện trước khi sinh code:

1. Đổi các cột tiếng Việt từ `varchar` sang `nvarchar`.
2. Thêm `CREATE DATABASE`/`USE`, tên schema `dbo` và tên constraint rõ ràng.
3. Tạo unique index cho `Sach.ISBN`, `DocGia.Email`, `NhanVien.Email` khi có giá trị.
4. Thêm check constraint: số lượng không âm, `SoLuongCon <= SoLuong`, tiền phạt không âm, hạn trả không trước ngày mượn.
5. Không lưu mật khẩu thuần; đổi `MatKhau` thành `MatKhauHash` và chỉ lưu chuỗi băm.
6. Chuẩn hóa trạng thái/chức vụ bằng enum trong C# và check constraint trong SQL.
7. Bổ sung thời điểm tạo/cập nhật để truy vết dữ liệu.
8. Cân nhắc tách mỗi cuốn vật lý thành `CuonSach` nếu cần quản lý tình trạng từng bản sao. Với phạm vi MVP có thể giữ mô hình số lượng.
9. `NgayTra` nằm trong chi tiết là hợp lý khi từng đầu sách được trả riêng; thao tác trả phải cập nhật `SoLuongCon` trong transaction.
10. Bổ sung bảng `YeuCauMuon`/`DatSach` nếu Web cho phép độc giả gửi yêu cầu trực tuyến.

## 5. Quy tắc nghiệp vụ cần thống nhất

- Chỉ độc giả trạng thái hoạt động mới được mượn.
- Không cho mượn quá số lượng còn.
- Ngày hẹn trả không được trước ngày mượn.
- Một phiếu chỉ hoàn tất khi mọi chi tiết đã trả.
- Khi mượn/trả/hủy phải cập nhật tồn kho trong cùng transaction.
- Tiền phạt tính theo số ngày quá hạn và mức phạt cấu hình; không ghi cứng trong giao diện.
- Không xóa cứng dữ liệu đã phát sinh phiếu; chuyển trạng thái ngừng hoạt động hoặc dùng soft delete.
- Mọi đầu vào phải được kiểm tra ở API/Application, không chỉ ở giao diện.

## 6. Thứ tự triển khai

### Giai đoạn 1 - nền tảng

- Chốt use case, wireframe và quy tắc nghiệp vụ.
- Sửa script SQL, tạo dữ liệu mẫu và sơ đồ ERD.
- Tạo solution, các project, dependency direction và cấu hình môi trường.

### Giai đoạn 2 - nghiệp vụ lõi

- Entity Framework Core và migration.
- CRUD thể loại, tác giả, sách, độc giả.
- Đăng nhập/phân quyền.
- Mượn, trả, quá hạn, tiền phạt và transaction tồn kho.

### Giai đoạn 3 - giao diện và tích hợp

- Hoàn thiện Web responsive.
- Hoàn thiện Desktop WinForms.
- Kiểm thử luồng đồng bộ Web - Desktop - SQL Server.

### Giai đoạn 4 - nộp bài

- Unit test cho quy tắc mượn/trả/tính phạt.
- Seed dữ liệu demo, xử lý lỗi và log.
- Chụp ảnh giao diện, hoàn thiện báo cáo, kịch bản demo và phân công vấn đáp.

## 7. Definition of Done cho mỗi chức năng

- Có yêu cầu và quy tắc nghiệp vụ rõ ràng.
- Có validation và thông báo lỗi dễ hiểu.
- Phân quyền đúng.
- Dữ liệu Web/Desktop đồng bộ qua cùng backend/database.
- Có ít nhất kiểm thử cho luồng thành công và lỗi quan trọng.
- Không chứa mật khẩu, connection string hoặc secret thật trong Git.
- Thành viên phụ trách giải thích được mã và kiến trúc liên quan.

## 8. Việc cần chốt trước khi bắt đầu code

- Phiên bản SQL Server và chuỗi kết nối của máy phát triển.
- Dùng .NET 10 hiện có hay cài .NET 8 LTS để đồng nhất máy các thành viên/phòng demo.
- Web có cho độc giả đặt/mượn trực tuyến hay chỉ tra cứu và xem lịch sử.
- Công thức tiền phạt, số sách tối đa và thời hạn mượn.
- Danh sách vai trò và quyền chi tiết.
- Phân công bốn thành viên, quy ước branch/PR và người tích hợp.
