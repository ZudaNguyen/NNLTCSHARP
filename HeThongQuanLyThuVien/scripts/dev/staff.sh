#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# staff.sh — điều khiển API như Admin / Thủ thư ngay trên macOS.
#
# Dùng để test song song: trình duyệt mở Web độc giả, terminal chạy script này
# để lập phiếu mượn, trả sách, thu tiền phạt, xem dashboard... giống hệt thao
# tác trên ứng dụng Desktop (Desktop chỉ là client khác của cùng REST API).
#
# Cấu hình: copy scripts/dev/.env.dev.example -> scripts/dev/.env.dev rồi điền
# tài khoản. File .env.dev đã nằm trong .gitignore.
#
# Ví dụ:
#   ./scripts/dev/staff.sh books
#   ./scripts/dev/staff.sh overdue-loan 1 1
#   ./scripts/dev/staff.sh return-all 3
#   ./scripts/dev/staff.sh pay-all 3
# ---------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOKEN_DIR="$SCRIPT_DIR/.tokens"

# Nạp cấu hình cục bộ (không commit)
if [ -f "$SCRIPT_DIR/.env.dev" ]; then
  set -a
  # shellcheck disable=SC1091
  . "$SCRIPT_DIR/.env.dev"
  set +a
fi

API="${QUANLYTHUVIEN_API_URL:-http://localhost:5058}"
API="${API%/}"

AS="${AS:-admin}"
if [ "$AS" = "manager" ] || [ "$AS" = "thu_thu" ]; then
  LOGIN_USER="${MANAGER_USER:-}"
  LOGIN_PASS="${MANAGER_PASS:-}"
  ROLE_LABEL="thủ thư"
else
  LOGIN_USER="${ADMIN_USER:-admin.dev}"
  LOGIN_PASS="${ADMIN_PASS:-}"
  ROLE_LABEL="admin"
fi

AUTH_TOKEN=""
MY_NAME="Nhân viên kiểm thử"

mkdir -p "$TOKEN_DIR"

# ------------------------------- tiện ích ----------------------------------

die() { printf '✖ %s\n' "$*" >&2; exit 1; }
note() { printf '→ %s\n' "$*" >&2; }

pretty() {
  python3 -c 'import json,sys
raw = sys.stdin.read().strip()
if not raw:
    sys.exit(0)
try:
    print(json.dumps(json.loads(raw), ensure_ascii=False, indent=2))
except json.JSONDecodeError:
    print(raw)'
}

# mkjson key=value ... : value dạng số/bool được tự chuyển kiểu.
# Thêm tiền tố str: để giữ nguyên chuỗi, ví dụ soDienThoai=str:0912345678
mkjson() {
  python3 - "$@" <<'PY'
import json, sys
data = {}
for pair in sys.argv[1:]:
    key, _, value = pair.partition("=")
    if value.startswith("str:"):
        data[key] = value[4:]
        continue
    low = value.lower()
    if low == "true":
        data[key] = True
    elif low == "false":
        data[key] = False
    elif low == "null":
        data[key] = None
    else:
        try:
            data[key] = int(value)
        except ValueError:
            try:
                data[key] = float(value)
            except ValueError:
                data[key] = value
print(json.dumps(data, ensure_ascii=False))
PY
}

date_shift() { # date_shift +14d | -20d  -> YYYY-MM-DD
  if date -v"$1" +%F >/dev/null 2>&1; then
    date -v"$1" +%F          # macOS/BSD
  else
    local sign="${1:0:1}" amount="${1:1}"
    date -d "$sign$amount" +%F   # GNU
  fi
}

api_request() { # method path [json-body]
  local method="$1" path="$2" body="${3-}" out code payload
  local -a args=(-sS -X "$method" "$API$path" -H 'Accept: application/json' -w $'\n%{http_code}')
  if [ -n "$AUTH_TOKEN" ]; then args+=(-H "Authorization: Bearer $AUTH_TOKEN"); fi
  if [ -n "$body" ]; then args+=(-H 'Content-Type: application/json' --data "$body"); fi
  out="$(curl "${args[@]}")" || die "Không gọi được $API$path"
  code="${out##*$'\n'}"
  payload="${out%$'\n'*}"
  if [ "$code" -ge 400 ]; then
    printf '✖ HTTP %s — %s %s\n' "$code" "$method" "$path" >&2
    printf '%s\n' "$payload" | pretty >&2
    return 1
  fi
  printf '%s' "$payload"
}

# ------------------------------- đăng nhập ---------------------------------

cache_file() { printf '%s/%s_%s.json' "$TOKEN_DIR" "$AS" "$LOGIN_USER"; }

login() {
  [ -n "$LOGIN_USER" ] || die "Thiếu tài khoản. Đặt ADMIN_USER/ADMIN_PASS (hoặc MANAGER_USER/MANAGER_PASS) trong scripts/dev/.env.dev"
  [ -n "$LOGIN_PASS" ] || die "Thiếu mật khẩu cho '$LOGIN_USER' trong scripts/dev/.env.dev"

  local cache; cache="$(cache_file)"
  if [ -f "$cache" ]; then
    local cached_token cached_exp now
    cached_token="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["token"])' "$cache")"
    cached_exp="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["exp"])' "$cache")"
    now="$(date +%s)"
    if [ -n "$cached_token" ] && [ "$cached_exp" -gt $((now + 60)) ]; then
      AUTH_TOKEN="$cached_token"
      MY_NAME="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1]))["name"])' "$cache")"
      return 0
    fi
  fi

  note "Đăng nhập $ROLE_LABEL: $LOGIN_USER"
  local payload token exp name
  payload="$(api_request POST /api/auth/login "$(python3 -c 'import json,sys;print(json.dumps({"tenDangNhap":sys.argv[1],"matKhau":sys.argv[2]}))' "$LOGIN_USER" "$LOGIN_PASS")")"
  token="$(printf '%s' "$payload" | python3 -c 'import json,sys;print(json.load(sys.stdin)["accessToken"])')"
  name="$(printf '%s' "$payload" | python3 -c 'import json,sys;print(json.load(sys.stdin)["user"]["tenHienThi"])')"
  exp="$(printf '%s' "$payload" | python3 -c '
import json, sys
from datetime import datetime, timezone
raw = json.load(sys.stdin)["expiresAtUtc"].replace("Z", "+00:00")
dt = datetime.fromisoformat(raw)
if dt.tzinfo is None:
    dt = dt.replace(tzinfo=timezone.utc)
print(int(dt.timestamp()))')"

  AUTH_TOKEN="$token"
  MY_NAME="$name"
  python3 -c 'import json,sys;json.dump({"token":sys.argv[2],"exp":int(sys.argv[3]),"name":sys.argv[4]},open(sys.argv[1],"w"))' \
    "$cache" "$token" "$exp" "$name"
}

need_auth() { [ -n "$AUTH_TOKEN" ] || login; }

# -------------------------------- lệnh -------------------------------------

cmd_help() {
  cat <<'TXT'
staff.sh — điều khiển API như Admin/Thủ thư (test song song với Web độc giả)

Đọc dữ liệu:
  books [search]                 sách còn hàng để lập phiếu
  readers [search]               danh sách độc giả
  staff [search]                 danh sách nhân viên
  accounts [search]              danh sách tài khoản (chỉ Admin)
  loans [search]                 phiếu mượn (lọc: --overdue)
  overdue                        mọi cuốn đang quá hạn
  loan <maPhieu>                 chi tiết một phiếu
  fines <maPhieu>                tiền phạt của phiếu
  dashboard                      số liệu tổng quan (Thủ thư)
  reports                        báo cáo sách đang mượn / mượn nhiều / tiền phạt
  whoami                         tài khoản đang dùng

Thao tác nghiệp vụ:
  new-reader <hoTen> <sdt> [username] [password]
                                 tạo độc giả + tài khoản DOC_GIA
  new-staff <hoTen> <username> [password]
                                 tạo nhân viên + tài khoản THU_THU (chỉ Admin)
  new-loan <maDocGia> <maSach> <soLuong> [ngayMuon] [hanTra]
                                 lập phiếu mượn (mặc định: hôm nay → +14 ngày)
  overdue-loan <maDocGia> <maSach> [soLuong] [soNgayQuaHan]
                                 lập phiếu đã quá hạn để test cảnh báo trên Web
  return <maPhieu> <maChiTiet> <soLuong> [ngayTra] [tinhTrang]
                                 trả một phần / toàn bộ một đầu sách
  return-all <maPhieu> [tinhTrang]  trả hết sách còn lại của phiếu
  pay <maPhieu> <maLanTra> <soTien> thu tiền phạt của một lần trả
  pay-all <maPhieu>              thu nốt toàn bộ tiền phạt còn nợ
  set-fine <maPhieu> <maLanTra> <soTien>
                                 sửa số tiền phạt phải thu
  rate [mucMoiNgay]              xem / đổi mức phạt mỗi ngày (đổi: chỉ Admin)

Tùy chọn:
  AS=manager ./scripts/dev/staff.sh ...   dùng tài khoản thủ thư thay vì admin
  API=http://ip:5058 ./scripts/dev/staff.sh ...  trỏ tới API khác

Tình trạng sách hợp lệ: "Nguyên vẹn", "Rách hoặc bẩn nhẹ", "Hỏng nặng", "Mất".
TXT
}

cmd_whoami() {
  need_auth
  printf 'API : %s\nVai trò : %s\nTài khoản: %s (%s)\n' "$API" "$ROLE_LABEL" "$LOGIN_USER" "$MY_NAME"
}

cmd_books() {
  need_auth
  api_request GET "/api/muontra/sach-con-hang${1:+?search=$(python3 -c 'import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1]))' "$1")}" | pretty
}

cmd_readers() {
  need_auth
  api_request GET "/api/docgia${1:+?search=$(python3 -c 'import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1]))' "$1")}" | pretty
}

cmd_staff() {
  need_auth
  api_request GET "/api/nhanvien${1:+?search=$(python3 -c 'import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1]))' "$1")}" | pretty
}

cmd_accounts() {
  need_auth
  api_request GET "/api/taikhoan${1:+?search=$(python3 -c 'import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1]))' "$1")}" | pretty
}

cmd_loans() {
  need_auth
  if [ "${1-}" = "--overdue" ]; then
    api_request GET "/api/muontra?chiQuaHan=true" | pretty
  elif [ -n "${1-}" ]; then
    api_request GET "/api/muontra?search=$(python3 -c 'import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1]))' "$1")" | pretty
  else
    api_request GET "/api/muontra" | pretty
  fi
}

cmd_overdue() { need_auth; api_request GET "/api/muontra/qua-han" | pretty; }
cmd_loan() { need_auth; api_request GET "/api/muontra/$1" | pretty; }
cmd_fines() { need_auth; api_request GET "/api/muontra/$1/tien-phat" | pretty; }
cmd_dashboard() { need_auth; api_request GET "/api/dashboard/tong-quan" | pretty; }

cmd_reports() {
  need_auth
  printf '=== Sách đang mượn\n'
  api_request GET "/api/baocao/sach-dang-muon" | pretty
  printf '\n=== Sách mượn nhiều\n'
  api_request GET "/api/baocao/sach-muon-nhieu" | pretty
  printf '\n=== Tiền phạt\n'
  api_request GET "/api/baocao/tien-phat" | pretty
}

cmd_new_reader() {
  local ho_ten="${1:?Thiếu họ tên}" sdt="${2:?Thiếu số điện thoại}"
  local username="${3:-}" password="${4:-}"
  need_auth

  [ -n "$username" ] || username="docgia$(date +%s | tail -c 5)"
  if [ -z "$password" ]; then
    password="DocGia_$(python3 -c 'import secrets;print(secrets.token_urlsafe(9))')!A9"
  fi

  local reader_id payload
  payload="$(api_request POST /api/docgia "$(mkjson \
    hoTen="$ho_ten" ngaySinh=2000-01-01 gioiTinh="Nam" soDienThoai="str:$sdt" \
    email=null diaChi=null trangThai="Hoạt động")")"
  reader_id="$(printf '%s' "$payload" | python3 -c 'import json,sys;print(json.load(sys.stdin)["maDocGia"])')" \
    || die "Không đọc được mã độc giả từ phản hồi API."

  api_request POST /api/taikhoan "$(mkjson \
    tenDangNhap="$username" tenHienThi="$ho_ten" matKhau="$password" \
    vaiTro=DOC_GIA maDocGia="$reader_id")" >/dev/null

  cat <<EOF
✔ Đã tạo độc giả #$reader_id
  Tên đăng nhập : $username
  Mật khẩu      : $password
  Đăng nhập Web : http://localhost:5109/dang-nhap
EOF
}

cmd_new_staff() {
  local ho_ten="${1:?Thiếu họ tên}" username="${2:?Thiếu tên đăng nhập}" password="${3:-}"
  need_auth
  [ -n "$password" ] || password="ThuThu_$(python3 -c 'import secrets;print(secrets.token_urlsafe(9))')!A9"

  local staff_id payload
  payload="$(api_request POST /api/nhanvien "$(mkjson \
    hoTen="$ho_ten" soDienThoai="str:0900000000" email=null chucVu="Thủ thư")")"
  staff_id="$(printf '%s' "$payload" | python3 -c 'import json,sys;print(json.load(sys.stdin)["maNhanVien"])')" \
    || die "Không đọc được mã nhân viên từ phản hồi API."

  api_request POST /api/taikhoan "$(mkjson \
    tenDangNhap="$username" tenHienThi="$ho_ten" matKhau="$password" \
    vaiTro=THU_THU maNhanVien="$staff_id")" >/dev/null

  cat <<EOF
✔ Đã tạo nhân viên #$staff_id
  Tên đăng nhập : $username
  Mật khẩu      : $password
  Dùng: MANAGER_USER=$username MANAGER_PASS='$password' AS=manager ./scripts/dev/staff.sh whoami
EOF
}

create_loan() { # maDocGia maSach soLuong ngayMuon hanTra
  local body
  body="$(python3 -c '
import json, sys, uuid
print(json.dumps({
    "maDocGia": int(sys.argv[1]),
    "ngayMuon": sys.argv[4],
    "hanTra": sys.argv[5],
    "nguoiLap": sys.argv[6],
    "idempotencyKey": str(uuid.uuid4()),
    "chiTiet": [{"maSach": int(sys.argv[2]), "soLuong": int(sys.argv[3])}]
}, ensure_ascii=False))' "$1" "$2" "$3" "$4" "$5" "$MY_NAME")"
  api_request POST /api/muontra "$body" | pretty
}

cmd_new_loan() {
  local doc_gia="${1:?Thiếu mã độc giả}" ma_sach="${2:?Thiếu mã sách}" so_luong="${3:-1}"
  local ngay_muon="${4:-$(date +%F)}" han_tra="${5:-$(date_shift +14d)}"
  need_auth
  note "Lập phiếu: độc giả $doc_gia, sách $ma_sach × $so_luong, $ngay_muon → $han_tra"
  create_loan "$doc_gia" "$ma_sach" "$so_luong" "$ngay_muon" "$han_tra"
}

cmd_overdue_loan() {
  local doc_gia="${1:?Thiếu mã độc giả}" ma_sach="${2:?Thiếu mã sách}" so_luong="${3:-1}" tre="${4:-5}"
  need_auth
  local ngay_muon han_tra
  ngay_muon="$(date_shift -"$((tre + 14))"d)"
  han_tra="$(date_shift -"${tre}"d)"
  note "Lập phiếu quá hạn $tre ngày: độc giả $doc_gia, sách $ma_sach × $so_luong, $ngay_muon → $han_tra"
  create_loan "$doc_gia" "$ma_sach" "$so_luong" "$ngay_muon" "$han_tra"
}

cmd_return() {
  local phieu="${1:?Thiếu mã phiếu}" chi_tiet="${2:?Thiếu mã chi tiết}" so_luong="${3:?Thiếu số lượng}"
  local ngay_tra="${4:-$(date +%F)}" tinh_trang="${5:-Nguyên vẹn}"
  need_auth
  api_request PATCH "/api/muontra/$phieu/chi-tiet/$chi_tiet/tra" "$(python3 -c '
import json, sys, uuid
print(json.dumps({
    "soLuongXuLy": int(sys.argv[1]),
    "ngayTra": sys.argv[2],
    "tinhTrangSach": sys.argv[3],
    "soTienThu": 0,
    "nguoiThu": sys.argv[4],
    "idempotencyKey": str(uuid.uuid4())
}, ensure_ascii=False))' "$so_luong" "$ngay_tra" "$tinh_trang" "$MY_NAME")" | pretty
}

cmd_return_all() {
  local phieu="${1:?Thiếu mã phiếu}" tinh_trang="${2:-Nguyên vẹn}"
  need_auth
  api_request GET "/api/muontra/$phieu" \
    | python3 -c 'import json,sys
d = json.load(sys.stdin)
for item in d.get("chiTiet", []):
    if item.get("soLuongConMuon", 0) > 0:
        print(item["maChiTiet"], item["soLuongConMuon"], item["tenSach"], sep="\t")' \
    | while IFS=$'\t' read -r ma_chi_tiet con_lai ten_sach; do
        note "Trả $con_lai cuốn: $ten_sach"
        cmd_return "$phieu" "$ma_chi_tiet" "$con_lai" "$(date +%F)" "$tinh_trang" >/dev/null
      done
  note "Tiền phạt sau khi trả:"
  cmd_fines "$phieu"
}

cmd_pay() {
  local phieu="${1:?Thiếu mã phiếu}" lan_tra="${2:?Thiếu mã lần trả}" so_tien="${3:?Thiếu số tiền}"
  need_auth
  api_request POST "/api/muontra/$phieu/tien-phat/thu" "$(python3 -c '
import json, sys, uuid
amount = float(sys.argv[2])
print(json.dumps({
    "maLanTra": int(sys.argv[1]),
    "soTien": int(amount) if amount.is_integer() else amount,
    "nguoiThu": sys.argv[3],
    "idempotencyKey": str(uuid.uuid4())
}, ensure_ascii=False))' "$lan_tra" "$so_tien" "$MY_NAME")" | pretty
}

cmd_pay_all() {
  local phieu="${1:?Thiếu mã phiếu}"
  need_auth
  api_request GET "/api/muontra/$phieu/tien-phat" \
    | python3 -c 'import json,sys
d = json.load(sys.stdin)
for khoan in d.get("cacKhoanPhat", []):
    con_no = khoan.get("conNo") or 0
    if con_no > 0:
        print(khoan["maLanTra"], f"{con_no:g}", sep="\t")' \
    | while IFS=$'\t' read -r lan_tra con_no; do
        note "Thu $con_no đ cho lần trả #$lan_tra"
        cmd_pay "$phieu" "$lan_tra" "$con_no" >/dev/null
      done
  cmd_fines "$phieu"
}

cmd_set_fine() {
  local phieu="${1:?Thiếu mã phiếu}" lan_tra="${2:?Thiếu mã lần trả}" so_tien="${3:?Thiếu số tiền}"
  need_auth
  api_request PATCH "/api/muontra/$phieu/tien-phat" "$(mkjson maLanTra="$lan_tra" soTienPhatPhaiThu="$so_tien")" | pretty
}

cmd_rate() {
  need_auth
  if [ -n "${1-}" ]; then
    api_request PUT /api/muontra/cau-hinh/tien-phat "$(mkjson mucPhatMoiNgay="$1")" | pretty
  else
    api_request GET /api/muontra/cau-hinh/tien-phat | pretty
  fi
}

# -------------------------------- main -------------------------------------

command="${1:-help}"
[ $# -gt 0 ] && shift

case "$command" in
  help|-h|--help) cmd_help ;;
  whoami) cmd_whoami ;;
  books) cmd_books "${1-}" ;;
  readers) cmd_readers "${1-}" ;;
  staff) cmd_staff "${1-}" ;;
  accounts) cmd_accounts "${1-}" ;;
  loans) cmd_loans "${1-}" ;;
  overdue) cmd_overdue ;;
  loan) cmd_loan "${1:?Thiếu mã phiếu}" ;;
  fines) cmd_fines "${1:?Thiếu mã phiếu}" ;;
  dashboard) cmd_dashboard ;;
  reports) cmd_reports ;;
  new-reader) cmd_new_reader "${1-}" "${2-}" "${3-}" "${4-}" ;;
  new-staff) cmd_new_staff "${1-}" "${2-}" "${3-}" ;;
  new-loan) cmd_new_loan "${1-}" "${2-}" "${3-}" "${4-}" "${5-}" ;;
  overdue-loan) cmd_overdue_loan "${1-}" "${2-}" "${3-}" "${4-}" ;;
  return) cmd_return "${1-}" "${2-}" "${3-}" "${4-}" "${5-}" ;;
  return-all) cmd_return_all "${1-}" "${2-}" ;;
  pay) cmd_pay "${1-}" "${2-}" "${3-}" ;;
  pay-all) cmd_pay_all "${1-}" ;;
  set-fine) cmd_set_fine "${1-}" "${2-}" "${3-}" ;;
  rate) cmd_rate "${1-}" ;;
  *) printf 'Lệnh không hợp lệ: %s\n\n' "$command" >&2; cmd_help >&2; exit 2 ;;
esac
