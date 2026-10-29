# Database

Thư mục này chứa script và seeder cho SQL Server.

## Nội dung

| Đường dẫn | Mô tả |
|---|---|
| `AllTablesAndDemoData.sql` | Gộp schema + dữ liệu mẫu cho **database demo trống**, kèm bộ sách DEMO gồm 35 đầu sách hiển thị (7 thể loại × 5 đầu sách, số bản trong mỗi thể loại là 1–5; còn nhiều bản, còn 1 bản, hết bản, nhiều tác giả, sách đã ẩn). Bắt buộc chạy ở **SQLCMD Mode**; từ chối chạy nếu đã có bảng nghiệp vụ. |
| `seeds/` | Seeder C# opt-in, có kiểm tra backup trước khi ghi. Xem [`seeds/README.md`](seeds/README.md). |

## Schema chuẩn

Nguồn schema chính là 7 file trong `SourceCode/QuanLyThuVien.Infrastructure/Data/Sql/`:

`BooksSchema.sql` → `CatalogModuleSchema.sql` → `ReaderModuleSchema.sql` → `EmployeeModuleSchema.sql` → `AuthenticationModuleSchema.sql` → `BorrowingModuleSchema.sql` → `BorrowRequestModuleSchema.sql`

Chạy đúng thứ tự này, sau đó kiểm tra phải có **15 bảng** trong `sys.tables`. Hướng dẫn đầy đủ (kể cả cách chạy bằng Docker trên macOS): [`docs/SETUP_GUIDE.md`](../docs/SETUP_GUIDE.md).

## Script cũ

Script thiết kế ban đầu `HeThongQuanLyThuVien.sql` đã được chuyển sang `_archive/legacy-schema.sql` ở gốc repo. Nó **lệch** so với schema hiện tại (tên cột, cột `NhanVien.MatKhau` còn lưu mật khẩu thô) và chỉ dùng để tham khảo lịch sử — không chạy.
