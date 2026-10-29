# Bản đồ module, vai trò và API

Tài liệu này là nguồn tham chiếu duy nhất khi thêm chức năng mới, đặc biệt là phần **Web độc giả (user)**.
Cập nhật file này khi thêm route hoặc đổi quyền.

## 1. Vai trò

Giá trị lưu trong `TaiKhoan.VaiTro`, khai báo tại `SourceCode/QuanLyThuVien.Domain/Enums/AccountRoles.cs`.

| Hằng số | Giá trị DB | Ý nghĩa | Giao diện tương ứng |
|---|---|---|---|
| `AccountRoles.Admin` | `ADMIN` | Quản trị viên | Desktop |
| `AccountRoles.Librarian` | `THU_THU` | Thủ thư / quản lý nghiệp vụ | Desktop |
| `AccountRoles.Reader` | `DOC_GIA` | Độc giả | Web |
| `AccountRoles.Staff` | `ADMIN,THU_THU` | Tập vai trò nhân viên | Desktop |
| `AccountRoles.All` | cả 3 | Tập tất cả vai trò | — |

**Quy ước:** không gõ lại chuỗi `"ADMIN"`, `"THU_THU"`, `"DOC_GIA"` trong code. Dùng `[Authorize(Roles = AccountRoles.Staff)]`.
Giá trị phải khớp check constraint `CK_TaiKhoan_VaiTro` trong `LibraryDbContext.cs`.

## 2. Trạng thái (chuỗi tiếng Việt lưu trong DB)

Chưa gom thành hằng số; khi sửa phải giữ đúng nguyên văn vì có ràng buộc DB và `RegularExpression` trong DTO.

| Bảng | Giá trị hợp lệ |
|---|---|
| `TaiKhoan.TrangThai` | `Hoạt động`, `Bị khóa` |
| `DocGia.TrangThai` | `Hoạt động`, `Khóa thẻ` |
| `NhanVien.TrangThai` | `Hoạt động`, `Ngừng hoạt động` |

## 3. Trách nhiệm từng project

| Project | Trách nhiệm | Trạng thái |
|---|---|---|
| `Domain` | Entity, enum (`AccountRoles`), exception nghiệp vụ | Xong |
| `Application` | DTO, interface, service nghiệp vụ, validate | Xong cho các module quản trị |
| `Infrastructure` | EF Core `LibraryDbContext`, repository, SQL schema | Xong cho các module quản trị |
| `Api` | REST API dùng chung cho Desktop và Web | Xong 56 endpoint |
| `Desktop` | WinForms cho Admin + Thủ thư, có cả màn hình độc giả | Xong phần quản trị |
| `Web` | ASP.NET Core MVC cho độc giả | **Khung rỗng — phần đang làm** |
| `Tests` | xUnit, hiện chưa có test case | Trống |

## 4. Ma trận API

`ANON` = gọi được không cần đăng nhập. Cột **Quyền** là quyền còn lại sau khi `[AllowAnonymous]` ghi đè `[Authorize]` ở cấp class.

### 4.1 Xác thực — `api/auth`

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| POST | `/api/auth/login` | ANON | Body `LoginRequestDto`; rate limit 10 lần/phút/IP |
| GET | `/api/auth/me` | Đã đăng nhập | Trả `AuthUserDto` |
| POST | `/api/auth/logout` | Đã đăng nhập | Thu hồi session |
| POST | `/api/auth/change-password` | Đã đăng nhập | Mật khẩu mới ≥ 12 ký tự, đủ 4 nhóm |

Token là opaque bearer, gửi qua header `Authorization: Bearer <accessToken>`.

### 4.2 Danh mục — dùng được cho cả Web độc giả

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| GET | `/api/sach` | ANON | Query: `search`, `maTheLoai`, `includeDeleted`, `onlyDeleted` |
| GET | `/api/sach/{id}` | ANON | Chi tiết sách |
| GET | `/api/sach/lookups` | ANON | Thể loại + tác giả cho combobox |
| POST/PUT/DELETE | `/api/sach...` | Staff | Quản trị kho sách |
| GET | `/api/theloai` | ANON | Danh sách thể loại |
| GET | `/api/theloai/{id}` | ANON | |
| GET | `/api/tacgia` | ANON | Danh sách tác giả |
| GET | `/api/tacgia/{id}` | ANON | |

`SachDto` trả về `tenSach`, `isbn`, `namXB`, `tenTheLoai`, `tenTacGia[]`, `soLuongCon`, `isDeleted`.

### 4.3 Độc giả — `api/docgia`

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| GET | `/api/docgia/me` | Reader | Hồ sơ của chính mình (dùng `MaDocGia` trong token) |
| GET | `/api/docgia` | Staff | Tìm kiếm, lọc |
| GET | `/api/docgia/{id}` | Staff | |
| POST | `/api/docgia` | Staff | |
| PUT | `/api/docgia/{id}` | Staff | |
| PATCH | `/api/docgia/{id}/trang-thai` | Staff | `Hoạt động` / `Khóa thẻ` |

### 4.4 Mượn trả — `api/muontra`

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| GET | `/api/muontra/me` | Reader | Danh sách phiếu mượn của mình |
| GET | `/api/muontra/me/{id}` | Reader | Chi tiết một phiếu (kiểm tra sở hữu) |
| GET | `/api/muontra/me/{id}/tien-phat` | Reader | Tiền phạt của phiếu đó |
| GET | `/api/muontra` | Staff | Tìm kiếm phiếu |
| GET | `/api/muontra/docgia/{readerId}` | Staff | Lịch sử mượn của một độc giả |
| GET | `/api/muontra/sach-con-hang` | Staff | Sách còn hàng để lập phiếu |
| GET | `/api/muontra/qua-han` | Staff | Danh sách quá hạn |
| GET | `/api/muontra/{id}` | Staff | |
| POST | `/api/muontra` | Staff | Lập phiếu mượn |
| PATCH | `/api/muontra/{id}/chi-tiet/{detailId}/tra` | Staff | Nhận trả |
| GET/PATCH | `/api/muontra/{id}/tien-phat` | Staff | Xem / điều chỉnh phạt |
| POST | `/api/muontra/{id}/tien-phat/thu` | Staff | Thu tiền phạt |
| GET | `/api/muontra/cau-hinh/tien-phat` | Staff | Đọc cấu hình phạt |
| PUT | `/api/muontra/cau-hinh/tien-phat` | Admin | Sửa cấu hình phạt |

### 4.5 Quản trị — chỉ Desktop

| Nhóm | Route gốc | Quyền |
|---|---|---|
| Nhân viên | `/api/nhanvien` | Admin |
| Tài khoản | `/api/taikhoan` | Admin |
| Dashboard | `/api/dashboard` | Staff |
| Báo cáo | `/api/baocao` | Staff |

### 4.6 Dev only (chỉ `Development`)

| Method | Route | Quyền |
|---|---|---|
| GET | `/api/dev/ping` | ANON |
| GET | `/api/dev/db-check` | Đã đăng nhập |

## 5. Phần Web độc giả (user) — hiện có gì, còn thiếu gì

Đã có sẵn và **đã kiểm chứng chạy thật**:

- Đăng nhập bằng tài khoản `DOC_GIA`, nhận token.
- `GET /api/docgia/me` trả hồ sơ độc giả.
- `GET /api/muontra/me` và `/me/{id}` trả lịch sử mượn.
- `GET /api/muontra/me/{id}/tien-phat` trả tiền phạt.
- `GET /api/sach`, `/api/theloai`, `/api/tacgia` gọi anonymous được → trang chủ, danh mục, tìm kiếm, chi tiết không cần đăng nhập.
- Phân quyền đã đúng: độc giả gọi API quản trị trả `403`, admin gọi `/api/muontra/me` trả `403`.

Còn thiếu (thuộc phần việc user):

1. **Gửi yêu cầu mượn / đặt sách** — chưa có endpoint, chưa có bảng `YeuCauMuon`. Kế hoạch dự án ghi đây là "nên có".
2. **Tầng gọi API trong project Web** — `QuanLyThuVien.Web` hiện chỉ có `HomeController` và một view mặc định, chưa có `Services/LibraryApiClient` hay xử lý token/cookie.
3. **Trang và layout** — chưa có layout độc giả, chưa có trang danh mục/chi tiết/lịch sử.

### Gợi ý vị trí đặt code mới

```text
SourceCode/QuanLyThuVien.Web/
├── Controllers/
│   ├── HomeController.cs        (đã có)
│   ├── SachController.cs        (mới) danh mục + chi tiết + tìm kiếm
│   └── TaiKhoanController.cs    (mới) đăng nhập/đăng xuất/lịch sử mượn
├── Services/
│   ├── LibraryApiClient.cs      (mới) gọi API, gắn Bearer token
│   └── ReaderSession.cs         (mới) giữ token trong cookie auth
├── ViewModels/                  (đã có thư mục)
└── Views/
    ├── Shared/_Layout.cshtml    (đã có, cần chỉnh cho độc giả)
    ├── Sach/Index.cshtml, Details.cshtml
    └── TaiKhoan/Login.cshtml, LichSuMuon.cshtml
```

Nếu cần thêm nghiệp vụ mới, thứ tự thêm để không phá phần đã xong:

1. `Domain` — entity (ví dụ `YeuCauMuon`).
2. `Infrastructure/Persistence/LibraryDbContext.cs` — cấu hình bảng + một file `.sql` mới trong `Data/Sql`.
3. `Application/DTOs` + `Application/Interfaces` + `Application/Services`.
4. `Api/Controllers` — thêm route, dùng `AccountRoles.Reader`.
5. `Web` — trang và client.
6. Cập nhật lại chính file này (mục 4 và 5).

## 6. Quy ước chung

| Hạng mục | Giá trị |
|---|---|
| Cổng API | `http://localhost:5058` |
| Cổng Web | `http://localhost:5109` |
| SQL Server | `127.0.0.1,14333`, database `QuanLyThuVienDB` |
| Solution | `HeThongQuanLyThuVien/QuanLyThuVien.slnx` |
| Ngôn ngữ code | Tên lớp/thuộc tính tiếng Việt không dấu cho entity/DTO; tiếng Anh cho hạ tầng |
| Namespace gốc | `QuanLyThuVien.<Tầng>` |
| Route API | danh từ tiếng Việt không dấu, số ít: `/api/sach`, `/api/docgia` |
