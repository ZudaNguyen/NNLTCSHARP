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
| `YeuCauMuon.TrangThai` | `Chờ duyệt`, `Đã duyệt`, `Từ chối`, `Đã hủy` |
| `YeuCauKhoaTaiKhoan.TrangThai` | `Chờ duyệt`, `Đã duyệt`, `Từ chối`, `Đã hủy` |

## 3. Trách nhiệm từng project

| Project | Trách nhiệm | Trạng thái |
|---|---|---|
| `Domain` | Entity, enum (`AccountRoles`), exception nghiệp vụ | Xong |
| `Application` | DTO, interface, service nghiệp vụ, validate | Xong cho các module quản trị |
| `Infrastructure` | EF Core `LibraryDbContext`, repository, SQL schema | Xong cho các module quản trị |
| `Api` | REST API dùng chung cho Desktop và Web | Xong 77 endpoint |
| `Desktop` | WinForms cho Admin + Thủ thư, có cả màn hình độc giả | Xong phần quản trị |
| `Web` | ASP.NET Core MVC cho độc giả | Xong: tra cứu, mượn/đặt sách, đăng ký, lịch sử, hồ sơ |
| `Tests` | xUnit cho nghiệp vụ `YeuCauMuonService`, `YeuCauKhoaTaiKhoanService` và `AccountService` | 30 test |

## 4. Ma trận API

`ANON` = gọi được không cần đăng nhập. Cột **Quyền** là quyền còn lại sau khi `[AllowAnonymous]` ghi đè `[Authorize]` ở cấp class.

### 4.1 Xác thực — `api/auth`

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| POST | `/api/auth/login` | ANON | Body `LoginRequestDto`; rate limit 10 lần/phút/IP |
| POST | `/api/auth/register` | ANON | Body `RegisterReaderRequestDto`; tạo `DocGia` + `TaiKhoan` `Hoạt động` trong một transaction, trả `LoginResponseDto`; rate limit 5 lần/phút/IP |
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

### 4.5 Yêu cầu mượn — `api/yeucau`

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| POST | `/api/yeucau` | Reader | Gửi yêu cầu mượn; body `TaoYeuCauMuonDto` |
| GET | `/api/yeucau/me` | Reader | Danh sách yêu cầu của mình; query `trangThai` |
| GET | `/api/yeucau/me/{id}` | Reader | Chi tiết một yêu cầu (kiểm tra sở hữu) |
| POST | `/api/yeucau/me/{id}/huy` | Reader | Hủy khi còn `Chờ duyệt` |
| GET | `/api/yeucau` | Staff | Tìm kiếm; query `search`, `trangThai` |
| GET | `/api/yeucau/{id}` | Staff | |
| POST | `/api/yeucau/{id}/duyet` | Staff | Duyệt → tạo phiếu mượn 14 ngày |
| POST | `/api/yeucau/{id}/tu-choi` | Staff | Body `TuChoiYeuCauMuonDto` (`lyDo` tùy chọn) |

Quy tắc nghiệp vụ:

- Mỗi độc giả chỉ có **một** yêu cầu `Chờ duyệt` cho mỗi đầu sách (filtered unique index `UX_YeuCauMuon_DangCho`).
- Không gửi được nếu độc giả đang mượn chính cuốn đó (còn sách chưa trả).
- Vẫn cho đặt khi sách đã hết bản; thủ thư duyệt sẽ báo lỗi tồn kho nếu không còn bản.
- Duyệt lại yêu cầu `Đã duyệt` là idempotent: trả về phiếu cũ, không sinh phiếu thứ hai (dùng `IdempotencyKey` của yêu cầu).
- Từ chối mặc định lý do `Thủ thư từ chối yêu cầu.` nếu để trống.

### 4.6 Yêu cầu khóa tài khoản — `api/yeucau-khoa`

Chỉ áp dụng cho tài khoản vai trò `DOC_GIA`. Thủ thư gửi/hủy yêu cầu của mình; **chỉ Admin** duyệt hoặc từ chối.

| Method | Route | Quyền | Ghi chú |
|---|---|---|---|
| GET | `/api/yeucau-khoa/tai-khoan` | Staff | Danh sách tài khoản độc giả để chọn; query `search` |
| GET | `/api/yeucau-khoa` | Staff | Admin xem tất cả; thủ thư chỉ thấy yêu cầu của mình; query `trangThai`, `search` |
| GET | `/api/yeucau-khoa/{id}` | Staff | Kiểm tra phạm vi sở hữu với thủ thư |
| POST | `/api/yeucau-khoa` | Thủ thư | Body `TaoYeuCauKhoaTaiKhoanDto`: `maTaiKhoan`, `lyDo` (10–500 ký tự), `thoiHanKhoaNgay` (1–365, tùy chọn) |
| POST | `/api/yeucau-khoa/{id}/huy` | Thủ thư | Chỉ hủy yêu cầu `Chờ duyệt` do chính mình gửi |
| POST | `/api/yeucau-khoa/{id}/duyet` | Admin | Khóa tài khoản + thẻ độc giả và thu hồi mọi phiên đăng nhập |
| POST | `/api/yeucau-khoa/{id}/tu-choi` | Admin | Body `TuChoiYeuCauKhoaTaiKhoanDto` (`lyDo` tùy chọn, tối đa 500 ký tự) |

Quy tắc nghiệp vụ:

- Thủ thư **không** được đổi `TaiKhoan.TrangThai`; chỉ gửi yêu cầu kèm lý do để Admin quyết định.
- Mỗi tài khoản chỉ có **một** yêu cầu `Chờ duyệt` (filtered unique index `UX_YeuCauKhoaTaiKhoan_DangCho`); `IdempotencyKey` cũng unique.
- Không gửi được nếu tài khoản không phải `DOC_GIA`, tài khoản đã `Bị khóa`, hoặc thẻ đã `Khóa thẻ`.
- Duyệt yêu cầu: `TaiKhoan.TrangThai = Bị khóa`, `DocGia.TrangThai = Khóa thẻ`, thu hồi tất cả phiên còn hiệu lực. Duyệt lại yêu cầu `Đã duyệt` là idempotent.
- `ThoiHanKhoaNgay` chỉ là thông tin nghiệp vụ để Admin tham khảo; **không** tự mở khóa khi hết hạn. Mở khóa vẫn thực hiện thủ công ở màn hình Tài khoản/Độc giả.
- Từ chối mặc định lý do `Admin từ chối yêu cầu khóa tài khoản.` nếu để trống.
- Desktop: Manager dùng mục **Yêu cầu khóa tài khoản** (NGHIỆP VỤ), Admin dùng **Duyệt khóa tài khoản** (QUẢN TRỊ). Cùng một control `YeuCauKhoaTaiKhoanControl` với cờ `isAdmin`.

### 4.7 Quản trị — chỉ Desktop

| Nhóm | Route gốc | Quyền |
|---|---|---|
| Nhân viên | `/api/nhanvien` | Admin |
| Tài khoản | `/api/taikhoan` | Admin |
| Dashboard | `/api/dashboard` | Staff |
| Báo cáo | `/api/baocao` | Staff |

### 4.8 Dev only (chỉ `Development`)

| Method | Route | Quyền |
|---|---|---|
| GET | `/api/dev/ping` | ANON |
| GET | `/api/dev/db-check` | Đã đăng nhập |

## 5. Phần Web độc giả (user) — hiện có gì, còn thiếu gì

Đã xong và **đã kiểm chứng runtime** (API cổng 5058 + Web cổng 5109 + SQL Server thật):

- Đăng nhập bằng tài khoản `DOC_GIA`; Web dùng cookie authentication, token API nằm trong claim `api_token` của cookie HttpOnly. Tài khoản không phải `DOC_GIA` bị từ chối ngay trên form đăng nhập.
- `POST /api/auth/register` → `/dang-ky`: độc giả tự đăng ký, hệ thống tạo thẻ và tài khoản `Hoạt động` ngay trong một transaction. API trả luôn token như đăng nhập nên Web tự đăng nhập và chuyển về trang chủ; kiểm tra trùng tên đăng nhập/email và chính sách mật khẩu (≥ 12 ký tự, đủ 4 nhóm).
- `GET /api/docgia/me` → `/tai-khoan` (hồ sơ + 5 phiếu gần nhất).
- `GET /api/muontra/me` và `/me/{id}` → `/lich-su-muon`, `/lich-su-muon/{id}`.
- `GET /api/muontra/me/{id}/tien-phat` → bảng tiền phạt từng lần trả (phải thu / đã thu / còn nợ).
- `GET /api/sach`, `/api/theloai` gọi anonymous được → `/`, `/sach`, `/sach/{id}` không cần đăng nhập.
- `GET /sach/suggest?q=...` trả tối đa 6 gợi ý (tên sách, tác giả, thể loại, số bản còn) cho ô tìm kiếm ở topbar và `/sach`; JS tự debounce, hỗ trợ phím ↑/↓/Enter/Escape.
- `POST /api/yeucau` → nút **Gửi yêu cầu mượn / Đặt trước** ở `/sach/{id}`; `GET /api/yeucau/me` → `/yeu-cau`; `POST /api/yeucau/me/{id}/huy` → nút **Hủy**.
- Trang `/sach/{id}` phân biệt 4 trạng thái: khách (mời đăng nhập), đang mượn cuốn này (kèm phiếu), đã có yêu cầu chờ duyệt (kèm nút hủy), còn lại (form đặt mượn).
- Trang chủ và layout đã thay dữ liệu mockup bằng dữ liệu thật: sách nổi bật, thể loại, sách đang mượn, cảnh báo sắp đến hạn/quá hạn, badge thông báo và badge yêu cầu chờ duyệt.
- Phân quyền đã đúng: độc giả gọi API quản trị trả `403`, admin gọi `/api/muontra/me` trả `403`, admin đăng nhập Web bị chặn.

Còn thiếu (thuộc phần việc user):

1. **Đổi mật khẩu trên Web** — API đã có `/api/auth/change-password` nhưng Web chưa có form.
2. **Gia hạn phiếu mượn trực tuyến** — chưa có endpoint; hiện thủ thư xử lý ở quầy.

### Cấu trúc Web hiện tại

```text
SourceCode/QuanLyThuVien.Web/
├── Controllers/
│   ├── HomeController.cs        trang chủ + mockup Manager/Admin
│   ├── SachController.cs        /sach (tìm kiếm, lọc thể loại, gợi ý) + /sach/{id}
│   ├── YeuCauMuonController.cs  /yeu-cau, gửi yêu cầu, hủy yêu cầu
│   └── TaiKhoanController.cs    đăng nhập/đăng ký/đăng xuất/lịch sử mượn/hồ sơ
├── Services/
│   ├── LibraryApiClient.cs      cửa ngõ duy nhất gọi REST API, gắn Bearer token
│   └── ReaderSession.cs         đọc/ghi claim cookie, đăng nhập/đăng xuất
├── ViewModels/                  Home, BookSearch, BorrowRequest, Account
└── Views/
    ├── Shared/_Layout.cshtml    layout độc giả, menu theo route, badge thông báo
    ├── Shared/_BookCard.cshtml  thẻ sách dùng lại
    ├── Home/Index.cshtml
    ├── Sach/Index.cshtml, Details.cshtml
    ├── YeuCauMuon/Index.cshtml
    └── TaiKhoan/Login.cshtml, LichSuMuon.cshtml, ChiTietMuon.cshtml, HoSo.cshtml
```

Quy ước: Web **không** tham chiếu `Infrastructure`/DbContext và không chứa logic nghiệp vụ; mọi thao tác đi qua `LibraryApiClient`. Địa chỉ API cấu hình bằng `ApiBaseUrl` trong `appsettings.json` (mặc định `http://localhost:5058/`).

Nếu cần thêm nghiệp vụ mới, thứ tự thêm để không phá phần đã xong:

1. `Domain` — entity (ví dụ `YeuCauMuon`).
2. `Infrastructure/Persistence/LibraryDbContext.cs` — cấu hình bảng + một file `.sql` mới trong `Data/Sql`.
3. `Application/DTOs` + `Application/Interfaces` + `Application/Services`.
4. `Api/Controllers` — thêm route, dùng `AccountRoles.Reader`.
5. `Web` — trang và client.
6. Cập nhật lại chính file này (mục 4 và 5).

### 5.1 Route Web độc giả

| Route | Controller/Action | Ghi chú |
|---|---|---|
| `GET /` | `Home/Index` | Trang chủ, tổng hợp thông báo |
| `GET /sach` | `Sach/Index` | Tìm kiếm, lọc thể loại |
| `GET /sach/suggest?q=` | `Sach/Suggest` | Gợi ý tìm kiếm (JSON, tối đa 6, từ 2 ký tự) |
| `GET /sach/{id}` | `Sach/Details` | Chi tiết + form đặt mượn |
| `POST /yeu-cau/dat/{maSach}` | `YeuCauMuon/Create` | Gửi yêu cầu, cần đăng nhập |
| `GET /yeu-cau` | `YeuCauMuon/Index` | Danh sách + tab trạng thái |
| `POST /yeu-cau/{id}/huy` | `YeuCauMuon/Cancel` | Hủy yêu cầu đang chờ |
| `GET /lich-su-muon`, `/lich-su-muon/{id}` | `TaiKhoan/LichSuMuon`, `ChiTietMuon` | Phiếu mượn, tiền phạt |
| `GET /tai-khoan` | `TaiKhoan/HoSo` | Hồ sơ độc giả |
| `GET/POST /dang-nhap` | `TaiKhoan/Login` | Cookie auth |
| `GET/POST /dang-ky` | `TaiKhoan/Register` | Tự đăng ký, tạo tài khoản hoạt động ngay, tự đăng nhập |
| `POST /dang-xuat` | `TaiKhoan/Logout` | Xóa cookie |

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
