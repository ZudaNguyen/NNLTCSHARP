# Dữ liệu demo thư viện

Seeder một lần có kiểm soát nằm tại `database/seeds/QuanLyThuVien.DemoSeeder.csproj`. API không tự seed khi khởi động.

## An toàn và cách chạy

- Seeder chỉ chấp nhận `Development`, server `127.0.0.1,14333` và database `QuanLyThuVienDB`.
- Trước mọi lần chạy, seeder tìm full backup gần nhất trong `msdb`, yêu cầu backup không quá 7 ngày và chạy `RESTORE VERIFYONLY`. Nếu thiếu hoặc verify lỗi, seeder dừng trước khi ghi.
- Connection string được đọc từ User Secrets của project API (`ConnectionStrings:LibraryDb`) hoặc biến `ConnectionStrings__LibraryDb`; bí mật không được in.
- Chạy từ thư mục solution:

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project database/seeds/QuanLyThuVien.DemoSeeder.csproj
```

Seeder tái sử dụng `ITheLoaiService`, `ITacGiaService`, `ISachService`, `IDocGiaService`, `INhanVienService` và `IMuonTraService`. Tạo phiếu, xác nhận trả, tính phạt và ghi nhận tiền thu đi qua nghiệp vụ Application hiện có để cập nhật tồn kho/lịch sử đúng cách.

## Tính lặp an toàn

- Bản ghi danh mục được nhận diện bằng tên demo; sách bằng ISBN mẫu có checksum; độc giả/nhân viên bằng tên/email demo.
- Mượn, trả và thu tiền dùng idempotency key ổn định. Chạy lại không tạo thêm phiếu, lần trả hay khoản thu.
- Seeder không cập nhật, ẩn, khôi phục hoặc xóa dữ liệu có sẵn; nếu bản ghi demo đã bị ẩn, seeder dừng thay vì tự bật lại.
- Sách có tiền tố `DEMO -`; ISBN chỉ là giá trị giả lập qua kiểm tra checksum, không khẳng định đã được cấp/phát hành. Người, địa chỉ, email và số điện thoại đều là dữ liệu hư cấu; không tạo tài khoản đăng nhập.
- Các khoản phạt dùng mức phạt hiện có trong `CauHinhTienPhat`; seeder từ chối chạy nếu chưa cấu hình mức phạt hợp lệ và không tự đổi cấu hình.
- Không tạo user/password demo; tài khoản hiện có được giữ nguyên. Các tác vụ đăng nhập cần dùng thông tin do chủ DB cấu hình riêng.

## Nhóm kịch bản

Seeder bổ sung 5 thể loại, 8 tác giả, 16 sách, 12 độc giả, 4 nhân viên và 12 phiếu. Các phiếu mẫu gồm đang mượn, gần hạn, quá hạn, trả đúng hạn, trả trễ có phạt chưa thu/thu một phần, mượn nhiều sách và trả một phần. Không xóa/reset dữ liệu hiện tại.

Sau khi chạy, kiểm tra lại row count trên SSMS với server `127.0.0.1,14333`, database `QuanLyThuVienDB`. Để xem bản ghi demo:

```sql
USE QuanLyThuVienDB;
SELECT ISBN, TenSach, SoLuong, SoLuongCon FROM dbo.Sach WHERE TenSach LIKE N'DEMO -%';
SELECT HoTen, Email, TrangThai FROM dbo.DocGia WHERE HoTen LIKE N'DEMO -%';
SELECT MaPhieu, MaDocGia, NgayMuon, HanTra FROM dbo.PhieuMuon ORDER BY MaPhieu DESC;
```

Seeder không tự rollback/xóa dữ liệu. Nếu cần bỏ bộ demo, trước tiên lọc chính xác các tên/ISBN `DEMO -`, đối chiếu toàn bộ quan hệ mượn-trả-thu tiền và dùng backup/quy trình quản trị phù hợp; không xóa trực tiếp các phiếu đang tham gia lịch sử nghiệp vụ.
