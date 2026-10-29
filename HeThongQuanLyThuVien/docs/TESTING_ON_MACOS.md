# Test song song Web độc giả + nghiệp vụ Admin/Thủ thư trên macOS

> Ứng dụng Desktop là WinForms nên **không chạy được trên macOS**. Nhưng Desktop chỉ là
> một client của REST API: mọi thao tác Admin/Thủ thư đều có endpoint. Vì vậy trên Mac
> bạn vẫn kiểm thử được toàn bộ luồng nghiệp vụ, chỉ không nhìn thấy giao diện Desktop.

## 0. Chọn cách test

| Bạn cần kiểm tra | Cách dùng |
|---|---|
| Web hiển thị đúng sau khi thủ thư lập phiếu / trả sách / thu phạt | **Cách 1** (API) |
| Quy tắc nghiệp vụ: quá hạn, tiền phạt, trùng sách, hết bản, phân quyền | **Cách 1** (API) |
| Giao diện WinForms: bố cục, binding, nút bấm | **Cách 2** (Windows VM/PC) |
| Chỉ cần biết Desktop còn build được | **Cách 2** hoặc máy Windows bất kỳ |

Cách 1 dùng được **mỗi ngày** và nhanh hơn mở Desktop. Chỉ khi cần review UI Desktop mới
phải dựng Windows.

---

## 1. Cách 1 — điều khiển API như Admin/Thủ thư (khuyến nghị)

### 1.1 Chuẩn bị một lần

```bash
cd HeThongQuanLyThuVien
cp scripts/dev/.env.dev.example scripts/dev/.env.dev
# mở scripts/dev/.env.dev, điền ADMIN_PASS (và MANAGER_USER/MANAGER_PASS nếu có)
./scripts/dev/staff.sh whoami
```

Yêu cầu: API đang chạy ở `http://localhost:5058` (xem `docs/SETUP_GUIDE.md`).

`scripts/dev/.env.dev` và `scripts/dev/.tokens/` đã nằm trong `.gitignore` — mật khẩu
không bị commit. Script cache token trong 8 giờ, nên không bị chặn bởi rate limit
10 lần đăng nhập/phút.

### 1.2 Quy trình 2 cửa sổ

- **Cửa sổ A** — trình duyệt mở `http://localhost:5109`, đăng nhập một tài khoản độc giả.
- **Cửa sổ B** — terminal chạy `./scripts/dev/staff.sh ...`.

Mỗi thao tác ở B xong thì **F5** ở A để thấy kết quả. Đây chính là "test song song":
API là nguồn sự thật duy nhất, Web và Desktop chỉ đọc lại dữ liệu đó.

### 1.3 Ví dụ 60 giây

```bash
# 1. Xem sách còn bản nào
./scripts/dev/staff.sh books

# 2. Lập phiếu đã quá hạn 5 ngày cho độc giả #1, sách #1
./scripts/dev/staff.sh overdue-loan 1 1 1 5
#    → F5 Web: banner "Bạn đang có 1 phiếu quá hạn", badge 1, pill "Quá hạn"

# 3. Trả hết sách của phiếu vừa lập (mã phiếu in ở bước 2)
./scripts/dev/staff.sh return-all 3
#    → tiền phạt = 5 ngày × 5.000đ = 25.000đ, trạng thái "Chưa thu"

# 4. Thu tiền phạt
./scripts/dev/staff.sh pay-all 3
#    → F5 Web: "Đã thu đủ", còn nợ 0đ

# 5. Duyệt yêu cầu mượn do độc giả gửi từ Web
./scripts/dev/staff.sh requests cho-duyet
./scripts/dev/staff.sh approve 1
#    → Web /yeu-cau: "Đã duyệt", xuất hiện phiếu PM-...
```

### 1.4 Danh sách lệnh

| Lệnh | Việc nó làm |
|---|---|
| `books [search]` | Sách còn hàng để lập phiếu |
| `readers [search]` | Danh sách độc giả |
| `staff [search]` | Danh sách nhân viên |
| `accounts [search]` | Danh sách tài khoản (chỉ Admin) |
| `loans [search]` / `loans --overdue` | Phiếu mượn / phiếu quá hạn |
| `overdue` | Mọi cuốn đang quá hạn |
| `loan <maPhieu>` | Chi tiết phiếu (kèm `maChiTiet` để trả) |
| `fines <maPhieu>` | Tiền phạt từng lần trả |
| `dashboard` | Số liệu tổng quan của Thủ thư |
| `reports` | Ba báo cáo: đang mượn / mượn nhiều / tiền phạt |
| `new-reader <hoTen> <sdt> [user] [pass]` | Tạo độc giả + tài khoản `DOC_GIA` |
| `new-staff <hoTen> <user> [pass]` | Tạo nhân viên + tài khoản `THU_THU` |
| `new-loan <maDocGia> <maSach> <soLuong> [ngayMuon] [hanTra]` | Lập phiếu mượn |
| `overdue-loan <maDocGia> <maSach> [soLuong] [soNgayTre]` | Lập phiếu đã quá hạn |
| `return <maPhieu> <maChiTiet> <soLuong> [ngayTra] [tinhTrang]` | Trả một phần |
| `return-all <maPhieu> [tinhTrang]` | Trả hết phần còn lại |
| `pay <maPhieu> <maLanTra> <soTien>` | Thu tiền phạt một lần trả |
| `pay-all <maPhieu>` | Thu nốt toàn bộ còn nợ |
| `set-fine <maPhieu> <maLanTra> <soTien>` | Sửa số tiền phạt phải thu |
| `rate [mucMoiNgay]` | Xem / đổi mức phạt mỗi ngày |
| `requests [trangThai\|search]` | Yêu cầu mượn từ Web (`cho-duyet`, `da-duyet`, `tu-choi`, `da-huy`) |
| `request <maYeuCau>` | Chi tiết một yêu cầu mượn |
| `approve <maYeuCau>` | Duyệt yêu cầu → tạo phiếu mượn 14 ngày |
| `reject <maYeuCau> [lyDo]` | Từ chối yêu cầu (kèm lý do hiển thị trên Web) |

Tình trạng sách hợp lệ: `Nguyên vẹn`, `Rách hoặc bẩn nhẹ`, `Hỏng nặng`, `Mất`.

### 1.5 Test vai trò Thủ thư (không phải Admin)

```bash
./scripts/dev/staff.sh new-staff "Trần Thị Thủ Thư" thuthu.test
# thêm MANAGER_USER/MANAGER_PASS vừa in ra vào scripts/dev/.env.dev

AS=manager ./scripts/dev/staff.sh dashboard   # 200
AS=manager ./scripts/dev/staff.sh accounts    # 403 — đúng: quản lý tài khoản chỉ Admin
```

Đây là cách kiểm tra ma trận phân quyền mà không cần mở Desktop.

### 1.6 Dùng file request có sẵn

`scripts/dev/api.http` chứa các request mẫu, gồm cả luồng yêu cầu mượn. Cài extension **REST Client** trong VS Code,
mở file và bấm *Send Request* trên từng khối (chạy khối đăng nhập trước).

File này không chứa mật khẩu: nó đọc `ADMIN_USER`, `ADMIN_PASS`, `READER_USER`,
`READER_PASS` từ `.env` ở thư mục `HeThongQuanLyThuVien` (đã gitignore, dùng chung với
docker compose). Thiếu biến thì request đăng nhập sẽ lỗi — điền rồi thử lại.

---

## 2. Cách 2 — chạy Desktop thật (Windows VM hoặc PC)

Máy Intel Mac chạy được Windows qua Parallels Desktop, VMware Fusion hoặc VirtualBox.
Máy Apple Silicon dùng Parallels/UTM với Windows 11 ARM (WinForms vẫn chạy).

### 2.1 Chuẩn bị trên macOS

1. Cho API nghe ra LAN thay vì chỉ localhost:

   ```bash
   ASPNETCORE_ENVIRONMENT=Development \
   dotnet run --project SourceCode/QuanLyThuVien.Api/QuanLyThuVien.Api.csproj \
     --urls http://0.0.0.0:5058
   ```

2. Lấy IP LAN của Mac:

   ```bash
   ipconfig getifaddr en0     # ví dụ 192.168.1.23
   ```

3. Kiểm tra từ Windows: mở PowerShell, chạy
   `curl http://<IP-MAC>:5058/api/dev/ping` → phải thấy JSON `API dang hoat dong`.
   Nếu không được: bật tường lửa macOS cho `dotnet`, kiểm tra VM đang ở chế độ NAT/Bridged.

### 2.2 Chuẩn bị trên Windows

```powershell
# Cài .NET 10 SDK (hoặc Desktop Runtime nếu chỉ chạy)
dotnet --version

cd <thư mục repo>
$env:QUANLYTHUVIEN_API_URL = "http://<IP-MAC>:5058/"
dotnet run --project SourceCode\QuanLyThuVien.Desktop\QuanLyThuVien.Desktop.csproj
```

Desktop đọc địa chỉ API từ biến môi trường `QUANLYTHUVIEN_API_URL` (fallback
`http://localhost:5058/`) — **không cần sửa code**.

SQL Server vẫn chạy Docker trên Mac (cổng `14333`); API giữ nguyên connection string
`127.0.0.1,14333` vì API chạy trên Mac.

### 2.3 Lưu ý

- VM NAT của Parallels thường thấy host ở `10.211.55.2`; VMware ở gateway `192.168.x.1`.
  Dùng IP đó nếu không gọi được IP LAN.
- Không mở API ra Internet. Chỉ dùng trong LAN/VM.
- Nếu repo nằm trong shared folder của VM, nên build bằng bản copy để tránh lỗi quyền file.

---

## 3. Kịch bản test song song đầy đủ

| # | Thao tác phía Thủ thư/Admin (Cách 1) | Kỳ vọng trên Web độc giả |
|---|---|---|
| 1 | `new-loan` cho độc giả đang đăng nhập | `/lich-su-muon` xuất hiện phiếu mới; trang chủ hiện sách đang mượn |
| 2 | `overdue-loan` | Banner cảnh báo, badge thông báo, pill "Quá hạn", "Quá hạn N ngày" |
| 3 | `return-all` đúng hạn | Phiếu chuyển "Đã hoàn tất", cảnh báo biến mất, kho sách tăng lại |
| 4 | `return-all` trễ N ngày | `/lich-su-muon/{id}` có bảng phạt: N × mức phạt, "Chưa thu" |
| 5 | `pay` một phần | Trạng thái "Đã thu một phần", còn nợ giảm |
| 6 | `pay-all` | "Đã thu đủ", còn nợ 0đ |
| 7 | `rate 10000` rồi trả trễ | Tiền phạt tính theo mức mới |
| 8 | `new-loan` hết bản sách | `/sach/{id}` hiện "Tạm hết sách" |
| 9 | `AS=manager ... accounts` | 403 — phân quyền đúng |
| 10 | Xóa/khóa tài khoản (API `PATCH /api/taikhoan/{id}/trang-thai`) | Đăng nhập Web bị từ chối |
| 11 | Độc giả bấm **Gửi yêu cầu mượn** ở `/sach/{id}` | `/yeu-cau` có dòng "Chờ duyệt", badge menu tăng |
| 12 | `requests cho-duyet` rồi `approve <maYeuCau>` | Yêu cầu chuyển "Đã duyệt" + số phiếu; `/sach/{id}` báo bạn đang mượn cuốn này |
| 13 | `reject <maYeuCau> "Sách đang bảo trì"` | Yêu cầu chuyển "Từ chối", hiện lý do trên Web |
| 14 | Độc giả bấm **Hủy** (tab Chờ duyệt) | Chuyển "Đã hủy", biến khỏi tab Chờ duyệt |
| 15 | Gửi trùng yêu cầu cho cùng đầu sách | Web báo "Bạn đã có một yêu cầu đang chờ duyệt cho cuốn sách này." |

Sau mỗi bước, **F5** trang Web tương ứng. Không cần đăng xuất/đăng nhập lại.

### 3.1 Đăng ký độc giả mới trên Web

1. Mở `http://localhost:5109/dang-ky` (hoặc bấm **Đăng ký** ở góc phải topbar).
2. Điền tên đăng nhập, mật khẩu ≥ 12 ký tự đủ 4 nhóm, họ tên, ngày sinh, giới tính,
   số điện thoại bắt đầu bằng `0`, email. Địa chỉ không bắt buộc.
3. Bấm **Tạo tài khoản** → chuyển về `/`, topbar hiện tên độc giả và menu đầy đủ.
   Không cần thủ thư duyệt: `DocGia.TrangThai` và `TaiKhoan.TrangThai` đều là `Hoạt động`.
4. Vào `/sach/16`, gửi yêu cầu mượn → `/yeu-cau` có dòng **Chờ duyệt**.
5. Kiểm tra lỗi: trùng tên đăng nhập hoặc email → thông báo ngay trên form; mật khẩu yếu
   hoặc xác nhận lệch → form giữ dữ liệu và báo lỗi, không tạo bản ghi.
6. API đăng ký giới hạn **5 lần/phút/IP**; gửi quá sẽ nhận thông báo chờ một phút.

Dọn dữ liệu test (đổi email/tên đăng nhập cho đúng):

```sql
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;
DELETE s FROM dbo.TaiKhoan_Session s
JOIN dbo.TaiKhoan t ON t.MaTaiKhoan = s.MaTaiKhoan
WHERE t.TenDangNhap = N'docgia.web';
DELETE y FROM dbo.YeuCauMuon y
JOIN dbo.DocGia d ON d.MaDocGia = y.MaDocGia
WHERE d.Email = N'docgia.web@example.invalid';
DELETE t FROM dbo.TaiKhoan t
JOIN dbo.DocGia d ON d.MaDocGia = t.MaDocGia
WHERE d.Email = N'docgia.web@example.invalid';
DELETE FROM dbo.DocGia WHERE Email = N'docgia.web@example.invalid';
COMMIT TRANSACTION;
```

### 3.2 Kiểm tra danh mục 35 sách và gợi ý tìm kiếm

Bộ dữ liệu demo có **7 thể loại × 5 đầu sách = 35 sách hiển thị**, số bản mỗi đầu từ 1–5
(ngoài ra còn 1 sách đã ẩn để kiểm thử lọc). Kiểm tra nhanh:

```sql
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SELECT t.TenTheLoai, COUNT(*) AS SoDauSach, MIN(s.SoLuong) AS MinBan, MAX(s.SoLuong) AS MaxBan
FROM dbo.Sach s
JOIN dbo.TheLoai t ON t.MaTheLoai = s.MaTheLoai
WHERE s.IsDeleted = 0
GROUP BY t.TenTheLoai
ORDER BY t.TenTheLoai;
```

Kỳ vọng: 7 dòng, mỗi dòng `SoDauSach = 5`, `MinBan`/`MaxBan` nằm trong 1–5.

Trên Web:

1. Mở `http://localhost:5109/sach` → đếm đủ 35 thẻ sách; bấm từng thể loại → đúng 5 sách.
2. Gõ ít nhất 2 ký tự vào ô tìm kiếm ở topbar hoặc ô của `/sach` (ví dụ `lập trình`,
   `DEMO`, `Nguyễn`) → sau ~220 ms hiện tối đa 6 gợi ý kèm tác giả, thể loại và badge
   `Còn N` / `Hết bản`; bấm gợi ý mở đúng `/sach/{id}`.
3. Dùng `↓`/`↑` để chọn, `Enter` để mở, `Esc` hoặc bấm ra ngoài để đóng. Gõ chuỗi không
   khớp (ví dụ `zzzz`) → không hiện dropdown.
4. Gọi trực tiếp `curl 'http://localhost:5109/sach/suggest?q=DEMO'` → JSON tối đa 6 phần tử,
   có `maSach`, `tenSach`, `tenTacGia`, `tenTheLoai`, `soLuongCon`, `conHang`, `url`.

---

## 4. Dữ liệu test và dọn dẹp

- Tài khoản test đã tạo trên máy dev này: `docgia.test` (độc giả #2),
  `thuthu.test` (thủ thư). Mật khẩu ghi trong `docs/DEMO_ACCOUNTS.local.md` (gitignored).
- Muốn database sạch: xem mục "Xóa sạch để làm lại từ đầu" trong file trên, sau đó chạy
  lại schema + `Database/AllTablesAndDemoData.sql`.
- Nên tạo database riêng (`QuanLyThuVienDB_test`) nếu cần thử phá dữ liệu mà không ảnh
  hưởng dữ liệu demo: đổi connection string bằng user-secrets.
- Script **không** dùng cho production: nó tạo độc giả/nhân viên thật trong DB đang trỏ tới.

## 5. Bảo mật

- Không commit `scripts/dev/.env.dev` hay `scripts/dev/.tokens/` (đã gitignore).
- Không đưa `ADMIN_PASS` vào ảnh chụp màn hình, PR hay báo cáo.
- Sau buổi demo, đổi mật khẩu tài khoản test hoặc xóa database demo.
