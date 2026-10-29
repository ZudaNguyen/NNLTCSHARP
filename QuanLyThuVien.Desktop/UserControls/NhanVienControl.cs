using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QuanLyThuVien.Desktop.Models;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class NhanVienControl : UserControl
    {
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        // Các Control nhập liệu (Bảng NhanVien)
        private TextBox txtMaNhanVien, txtHoTen, txtSoDienThoai, txtEmail;
        private TextBox txtTaiKhoan, txtMatKhau;
        private ComboBox cboChucVu;

        // Bảng danh sách
        private TextBox txtTimKiem;
        private DataGridView dgvNhanVien;

        private readonly string _currentUser;   // tài khoản đang đăng nhập (để chặn tự khóa/tự hạ quyền)

        public NhanVienControl() : this(string.Empty) { }

        public NhanVienControl(string currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            SetupNhanVienUI();
            LoadMockData();
            UI.ScrollableView.Configure(this, new Size(1000, 540));
        }

        private void SetupNhanVienUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(20);

            // ==========================================
            // 1. FORM HỒ SƠ THỦ THƯ (TOP PANEL)
            // ==========================================
            Panel pnlForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "🔑 Hồ Sơ Quản Trị & Thủ Thư (Bảng NhanVien)",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlForm.Controls.Add(lblTitle);

            // Cột 1: Thông tin cá nhân
            AddLabel(pnlForm, "Mã NV:", 15, 45);
            txtMaNhanVien = AddTextBox(pnlForm, 105, 42, 95);
            txtMaNhanVien.ReadOnly = true;
            txtMaNhanVien.BackColor = Color.FromArgb(235, 230, 220);
            txtMaNhanVien.PlaceholderText = "Tự động";

            AddLabel(pnlForm, "Họ và tên (*):", 15, 85);
            txtHoTen = AddTextBox(pnlForm, 105, 82, 285);
            txtHoTen.MaxLength = 150;        // NhanVien.HoTen nvarchar(150)

            AddLabel(pnlForm, "Điện thoại:", 15, 125);
            txtSoDienThoai = AddTextBox(pnlForm, 105, 122, 130);
            txtSoDienThoai.MaxLength = 15;   // varchar(15)

            AddLabel(pnlForm, "Email:", 245, 125);
            txtEmail = AddTextBox(pnlForm, 295, 122, 225);
            txtEmail.MaxLength = 150;        // varchar(150)

            // Cột 2: Thông tin tài khoản & Phân quyền
            AddLabel(pnlForm, "Tên tài khoản (*):", 420, 45);
            txtTaiKhoan = AddTextBox(pnlForm, 550, 42, 180);
            txtTaiKhoan.MaxLength = 100;     // varchar(100)

            AddLabel(pnlForm, "Mật khẩu (*):", 420, 85);
            txtMatKhau = AddTextBox(pnlForm, 550, 82, 180);
            txtMatKhau.PasswordChar = '●'; // Ẩn mật khẩu
            txtMatKhau.MaxLength = 100;

            AddLabel(pnlForm, "Phân quyền (Chức vụ):", 530, 125);
            cboChucVu = new ComboBox
            {
                Location = new Point(690, 122),
                Width = 140,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboChucVu.Items.AddRange(new object[] { "Admin", "Thủ thư" });
            cboChucVu.SelectedIndex = 1;
            pnlForm.Controls.Add(cboChucVu);

            // Nút chức năng
            FlowLayoutPanel flpButtons = new FlowLayoutPanel
            {
                Location = new Point(15, 165),
                Size = new Size(900, 42),
                WrapContents = false
            };

            Button btnThem = CreateButton("➕ Cấp Tài Khoản", ForestGreen);
            Button btnSua = CreateButton("✏️ Cập Nhật Quyền", Color.FromArgb(180, 120, 20));
            Button btnXoa = CreateButton("🔒 Khóa / Thu Hồi", Burgundy);
            Button btnLamMoi = CreateButton("🔄 Làm Mới Form", Color.FromArgb(90, 75, 65));

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            flpButtons.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi });
            pnlForm.Controls.Add(flpButtons);

            // ==========================================
            // 2. TÌM KIẾM & LƯỚI DỮ LIỆU (MIDDLE & BOTTOM)
            // ==========================================
            Panel pnlFilter = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = PaperCream };
            Label lblTim = new Label { Text = "🔍 Tra cứu Nhân sự:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = WoodDark, Location = new Point(5, 16), AutoSize = true };
            txtTimKiem = new TextBox { Location = new Point(165, 13), Width = 300, Font = new Font("Segoe UI", 10) };
            txtTimKiem.TextChanged += (s, e) => FilterGrid();
            pnlFilter.Controls.AddRange(new Control[] { lblTim, txtTimKiem });

            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            dgvNhanVien = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgvNhanVien.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvNhanVien.DefaultCellStyle.BackColor = CardBg; dgvNhanVien.DefaultCellStyle.ForeColor = WoodDark;
            dgvNhanVien.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160); dgvNhanVien.DefaultCellStyle.SelectionForeColor = WoodDark;

            dgvNhanVien.Columns.Add("MaNV", "Mã NV"); dgvNhanVien.Columns["MaNV"].FillWeight = 50;
            dgvNhanVien.Columns.Add("HoTen", "Họ và Tên"); dgvNhanVien.Columns["HoTen"].FillWeight = 140;
            dgvNhanVien.Columns.Add("SoDienThoai", "Số Điện Thoại"); dgvNhanVien.Columns["SoDienThoai"].FillWeight = 90;
            dgvNhanVien.Columns.Add("Email", "Email"); dgvNhanVien.Columns["Email"].FillWeight = 120;
            dgvNhanVien.Columns.Add("TaiKhoan", "Tài Khoản"); dgvNhanVien.Columns["TaiKhoan"].FillWeight = 100;
            dgvNhanVien.Columns.Add("ChucVu", "Chức Vụ"); dgvNhanVien.Columns["ChucVu"].FillWeight = 80;
            dgvNhanVien.Columns.Add("TrangThai", "Trạng Thái"); dgvNhanVien.Columns["TrangThai"].FillWeight = 80;

            UI.GridSelection.Bind(dgvNhanVien, LoadSelectedNhanVien, () => txtMaNhanVien.Clear(), btnSua, btnXoa);
            pnlGrid.Controls.Add(dgvNhanVien);

            this.Controls.Add(pnlGrid); this.Controls.Add(pnlFilter); this.Controls.Add(pnlForm);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark, AutoSize = true });
        }

        private TextBox AddTextBox(Control parent, int x, int y, int width)
        {
            TextBox txt = new TextBox { Location = new Point(x, y), Width = width, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(txt); return txt;
        }

        private Button CreateButton(string text, Color bg)
        {
            Button btn = new Button { Text = text, Size = new Size(185, 36), Margin = new Padding(0, 0, 12, 0), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0; return btn;
        }

        private void LoadMockData()
        {
            dgvNhanVien.Rows.Add("1", "Quản Trị Viên Hệ Thống", "0900000000", "admin@thuvien.com", "admin", "Admin", "Hoạt động");
            dgvNhanVien.Rows.Add("2", "Nguyễn Thủ Thư", "0911111111", "thuthu01@thuvien.com", "thuthu01", "Thủ thư", "Hoạt động");
        }

        private bool ValidateInput(string currentId = "")
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtTaiKhoan.Text))
            {
                MessageBox.Show("Họ tên và Tài khoản không được để trống!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string taiKhoan = txtTaiKhoan.Text.Trim();
            if (!Regex.IsMatch(taiKhoan, @"^[A-Za-z0-9._]{3,100}$"))
            {
                MessageBox.Show("Tên tài khoản chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới, tối thiểu 3 ký tự!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus();
                return false;
            }

            string sdt = txtSoDienThoai.Text.Trim();
            if (sdt.Length > 0 && !Regex.IsMatch(sdt, @"^0\d{9,10}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ (Phải bắt đầu bằng số 0 và có 10-11 chữ số)!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoDienThoai.Focus();
                return false;
            }

            string email = txtEmail.Text.Trim();
            if (email.Length > 0 && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Định dạng Email không hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            // Kiểm tra UNIQUE Tài khoản và Email
            foreach (DataGridViewRow row in dgvNhanVien.Rows)
            {
                string? rowId = row.Cells["MaNV"].Value?.ToString();
                if (rowId == currentId) continue;

                string? rowTaiKhoan = row.Cells["TaiKhoan"].Value?.ToString();
                if (string.Equals(rowTaiKhoan, taiKhoan, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Tên tài khoản này đã tồn tại! Vui lòng chọn tên đăng nhập khác.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtTaiKhoan.Focus();
                    return false;
                }

                string? rowEmail = row.Cells["Email"].Value?.ToString();
                if (email.Length > 0 && string.Equals(rowEmail, email, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Email này đã được dùng cho một nhân viên khác!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEmail.Focus();
                    return false;
                }
            }
            return true;
        }

        // Mật khẩu: kiểm tra độ dài tối thiểu. Việc BĂM mật khẩu phải làm ở API, Desktop chỉ gửi mật khẩu qua kết nối (nên HTTPS).
        private bool ValidateMatKhau(string matKhau)
        {
            if (matKhau.Length < BusinessRules.MatKhauToiThieu)
            {
                MessageBox.Show($"Mật khẩu phải có ít nhất {BusinessRules.MatKhauToiThieu} ký tự!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return false;
            }
            return true;
        }

        // Số Admin đang hoạt động (dùng để không bao giờ để hệ thống không còn Admin)
        private int DemAdminHoatDong()
        {
            int n = 0;
            foreach (DataGridViewRow r in dgvNhanVien.Rows)
            {
                if (r.Cells["ChucVu"].Value?.ToString() == "Admin" && r.Cells["TrangThai"].Value?.ToString() == "Hoạt động")
                    n++;
            }
            return n;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            if (string.IsNullOrEmpty(txtMatKhau.Text))
            {
                MessageBox.Show("Vui lòng khởi tạo mật khẩu cho nhân viên mới!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return;
            }
            if (!ValidateMatKhau(txtMatKhau.Text)) return;

            dgvNhanVien.Rows.Add(
                (dgvNhanVien.Rows.Count + 1).ToString(),
                txtHoTen.Text.Trim(), txtSoDienThoai.Text.Trim(), txtEmail.Text.Trim(),
                txtTaiKhoan.Text.Trim(), cboChucVu.SelectedItem.ToString(), "Hoạt động"
            );
            MessageBox.Show("Đã cấp tài khoản nhân viên mới thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaNhanVien.Text) || dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên dưới bảng để cập nhật!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput(txtMaNhanVien.Text)) return;

            DataGridViewRow row = dgvNhanVien.CurrentRow;
            string? taiKhoanCu = row.Cells["TaiKhoan"].Value?.ToString();
            string? chucVuCu = row.Cells["ChucVu"].Value?.ToString();
            string chucVuMoi = cboChucVu.SelectedItem?.ToString() ?? "Thủ thư";
            bool laChinhMinh = string.Equals(taiKhoanCu, _currentUser, StringComparison.OrdinalIgnoreCase);

            // Chặn các thao tác làm mất quyền quản trị
            if (chucVuCu == "Admin" && chucVuMoi != "Admin")
            {
                if (laChinhMinh)
                {
                    MessageBox.Show("Bạn không thể tự hạ quyền của chính mình!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (row.Cells["TrangThai"].Value?.ToString() == "Hoạt động" && DemAdminHoatDong() <= 1)
                {
                    MessageBox.Show("Đây là Admin hoạt động cuối cùng, không thể hạ quyền!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Mật khẩu chỉ đổi khi có nhập mới
            bool doiMatKhau = txtMatKhau.Text.Length > 0;
            if (doiMatKhau && !ValidateMatKhau(txtMatKhau.Text)) return;
            // TODO(API): gửi mật khẩu mới lên API để băm và lưu (MatKhauHash)
            txtMatKhau.Clear();

            row.Cells["HoTen"].Value = txtHoTen.Text.Trim();
            row.Cells["SoDienThoai"].Value = txtSoDienThoai.Text.Trim();
            row.Cells["Email"].Value = txtEmail.Text.Trim();
            row.Cells["TaiKhoan"].Value = txtTaiKhoan.Text.Trim();
            row.Cells["ChucVu"].Value = cboChucVu.SelectedItem.ToString();

            MessageBox.Show(doiMatKhau ? "Cập nhật hồ sơ, quyền và mật khẩu mới thành công!" : "Cập nhật hồ sơ và quyền thành công! (Mật khẩu giữ nguyên)",
                            "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null) return;
            var row = dgvNhanVien.CurrentRow;
            string taiKhoan = row.Cells["TaiKhoan"].Value?.ToString() ?? "";

            if (taiKhoan.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể khóa tài khoản Admin tối cao của hệ thống!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.Equals(taiKhoan, _currentUser, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không thể tự khóa tài khoản của chính mình!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string currentStatus = row.Cells["TrangThai"].Value?.ToString() ?? "";
            string newStatus = currentStatus == "Hoạt động" ? "Đã khóa (Nghỉ việc)" : "Hoạt động";

            if (currentStatus == "Hoạt động" && row.Cells["ChucVu"].Value?.ToString() == "Admin" && DemAdminHoatDong() <= 1)
            {
                MessageBox.Show("Đây là Admin hoạt động cuối cùng, không thể khóa!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Chuyển tài khoản [{taiKhoan}] sang trạng thái [{newStatus}]?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            row.Cells["TrangThai"].Value = newStatus;

            MessageBox.Show($"Tài khoản đã được chuyển sang trạng thái: [{newStatus}]", "Cập nhật trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSelectedNhanVien(DataGridViewRow row)
        {
            txtMaNhanVien.Text = row.Cells["MaNV"].Value?.ToString();
            txtHoTen.Text = row.Cells["HoTen"].Value?.ToString();
            txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
            txtEmail.Text = row.Cells["Email"].Value?.ToString();
            txtTaiKhoan.Text = row.Cells["TaiKhoan"].Value?.ToString();
            cboChucVu.SelectedItem = row.Cells["ChucVu"].Value?.ToString();
            txtMatKhau.Clear(); // Không hiển thị lại mật khẩu cũ vì lý do bảo mật
        }

        private void FilterGrid()
        {
            string kw = txtTimKiem.Text.Trim().ToLower();

            // Không thể ẩn dòng đang là dòng hiện hành (ném InvalidOperationException) -> bỏ chọn trước
            dgvNhanVien.CurrentCell = null;

            foreach (DataGridViewRow row in dgvNhanVien.Rows)
            {
                string ten = row.Cells["HoTen"].Value?.ToString()?.ToLower() ?? "";
                string tk = row.Cells["TaiKhoan"].Value?.ToString()?.ToLower() ?? "";
                row.Visible = string.IsNullOrEmpty(kw) || ten.Contains(kw) || tk.Contains(kw);
            }
        }

        private void ClearForm()
        {
            UI.GridSelection.Clear(dgvNhanVien);
            txtMaNhanVien.Clear(); txtHoTen.Clear(); txtSoDienThoai.Clear(); txtEmail.Clear();
            txtTaiKhoan.Clear(); txtMatKhau.Clear(); cboChucVu.SelectedIndex = 1; txtHoTen.Focus();
        }
    }
}
