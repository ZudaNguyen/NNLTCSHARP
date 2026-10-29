# _archive

Chứa các thành phần **không còn dùng** nhưng giữ lại để tham khảo lịch sử. Không project nào ở đây nằm trong solution và không được build.

| Đường dẫn | Nội dung | Vì sao bỏ |
|---|---|---|
| `legacy-desktop/` | Bản WinForms cũ (28 file) dùng `Data/DatabaseConnection.cs` hard-code `Server=NGVHNNA\SQLEXPRESS`, truy cập SQL trực tiếp, không qua API | Đã thay bằng `SourceCode/QuanLyThuVien.Desktop` dùng API + phân quyền |
| `legacy-schema.sql` | Script tạo bảng ban đầu | Lệch so với schema hiện tại (tên cột khác, `NhanVien.MatKhau` lưu mật khẩu thô) |
| `HUONG_DAN_CAI_DAT_VA_CHAY.old.md` | Hướng dẫn cài đặt cũ | Sai cổng API (`5074`), sai tên connection string (`DefaultConnection`); đã thay bằng `docs/SETUP_GUIDE.md` |

Có thể xóa toàn bộ thư mục này trước khi nộp bài nếu không cần đối chiếu lịch sử. Lịch sử vẫn còn trong git.
