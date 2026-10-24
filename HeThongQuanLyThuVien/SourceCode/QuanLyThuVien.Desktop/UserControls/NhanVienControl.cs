using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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

        public NhanVienControl()
        {
            InitializeComponent();
            SetupNhanVienUI();
            LoadMockData();
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
                Font = new Font("Georgia", 11.5f, FontStyle.Bold),
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

            AddLabel(pnlForm, "Điện thoại:", 15, 125);
            txtSoDienThoai = AddTextBox(pnlForm, 105, 122, 130);

            AddLabel(pnlForm, "Email:", 245, 125);
            txtEmail = AddTextBox(pnlForm, 295, 122, 225);

            // Cột 2: Thông tin tài khoản & Phân quyền
            AddLabel(pnlForm, "Tên tài khoản (*):", 420, 45);
            txtTaiKhoan = AddTextBox(pnlForm, 550, 42, 180);

            AddLabel(pnlForm, "Mật khẩu (*):", 420, 85);
            txtMatKhau = AddTextBox(pnlForm, 550, 82, 180);
            txtMatKhau.PasswordChar = '●'; // Ẩn mật khẩu

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
            Label lblTim = new Label { Text = "🔍 Tra cứu Nhân sự:", Font = new Font("Georgia", 10, FontStyle.Bold), ForeColor = WoodDark, Location = new Point(5, 16), AutoSize = true };
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
            dgvNhanVien.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
            dgvNhanVien.DefaultCellStyle.BackColor = CardBg; dgvNhanVien.DefaultCellStyle.ForeColor = WoodDark;
            dgvNhanVien.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160); dgvNhanVien.DefaultCellStyle.SelectionForeColor = WoodDark;

            dgvNhanVien.Columns.Add("MaNV", "Mã NV"); dgvNhanVien.Columns["MaNV"].FillWeight = 50;
            dgvNhanVien.Columns.Add("HoTen", "Họ và Tên"); dgvNhanVien.Columns["HoTen"].FillWeight = 140;
            dgvNhanVien.Columns.Add("SoDienThoai", "Số Điện Thoại"); dgvNhanVien.Columns["SoDienThoai"].FillWeight = 90;
            dgvNhanVien.Columns.Add("Email", "Email"); dgvNhanVien.Columns["Email"].FillWeight = 120;
            dgvNhanVien.Columns.Add("TaiKhoan", "Tài Khoản"); dgvNhanVien.Columns["TaiKhoan"].FillWeight = 100;
            dgvNhanVien.Columns.Add("ChucVu", "Chức Vụ"); dgvNhanVien.Columns["ChucVu"].FillWeight = 80;
            dgvNhanVien.Columns.Add("TrangThai", "Trạng Thái"); dgvNhanVien.Columns["TrangThai"].FillWeight = 80;

            dgvNhanVien.CellClick += DgvNhanVien_CellClick;
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
            Button btn = new Button { Text = text, Size = new Size(185, 36), Margin = new Padding(0, 0, 12, 0), Font = new Font("Georgia", 9.5f, FontStyle.Bold), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
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

            // Kiểm tra UNIQUE Tài khoản
            string taiKhoan = txtTaiKhoan.Text.Trim();
            foreach (DataGridViewRow row in dgvNhanVien.Rows)
            {
                string rowId = row.Cells["MaNV"].Value?.ToString();
                string rowTaiKhoan = row.Cells["TaiKhoan"].Value?.ToString();
                if (rowId != currentId && string.Equals(rowTaiKhoan, taiKhoan, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Tên tài khoản này đã tồn tại! Vui lòng chọn tên đăng nhập khác.", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtTaiKhoan.Focus();
                    return false;
                }
            }
            return true;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            if (string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                MessageBox.Show("Vui lòng khởi tạo mật khẩu cho nhân viên mới!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
            if (string.IsNullOrEmpty(txtMaNhanVien.Text) || dgvNhanVien.CurrentRow == null) return;
            if (!ValidateInput(txtMaNhanVien.Text)) return;

            DataGridViewRow row = dgvNhanVien.CurrentRow;
            row.Cells["HoTen"].Value = txtHoTen.Text.Trim();
            row.Cells["SoDienThoai"].Value = txtSoDienThoai.Text.Trim();
            row.Cells["Email"].Value = txtEmail.Text.Trim();
            row.Cells["TaiKhoan"].Value = txtTaiKhoan.Text.Trim();
            row.Cells["ChucVu"].Value = cboChucVu.SelectedItem.ToString();

            MessageBox.Show("Cập nhật quyền và hồ sơ thành công! (Mật khẩu chỉ thay đổi nếu có nhập mới)", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null) return;
            if (dgvNhanVien.CurrentRow.Cells["TaiKhoan"].Value.ToString().ToLower() == "admin")
            {
                MessageBox.Show("Không thể khóa tài khoản Admin tối cao của hệ thống!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string currentStatus = dgvNhanVien.CurrentRow.Cells["TrangThai"].Value.ToString();
            string newStatus = currentStatus == "Hoạt động" ? "Đã khóa (Nghỉ việc)" : "Hoạt động";
            dgvNhanVien.CurrentRow.Cells["TrangThai"].Value = newStatus;

            MessageBox.Show($"Tài khoản đã được chuyển sang trạng thái: [{newStatus}]", "Cập nhật trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DgvNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvNhanVien.Rows[e.RowIndex];
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
            foreach (DataGridViewRow row in dgvNhanVien.Rows)
            {
                string ten = row.Cells["HoTen"].Value?.ToString().ToLower() ?? "";
                string tk = row.Cells["TaiKhoan"].Value?.ToString().ToLower() ?? "";
                row.Visible = string.IsNullOrEmpty(kw) || ten.Contains(kw) || tk.Contains(kw);
            }
        }

        private void ClearForm()
        {
            txtMaNhanVien.Clear(); txtHoTen.Clear(); txtSoDienThoai.Clear(); txtEmail.Clear();
            txtTaiKhoan.Clear(); txtMatKhau.Clear(); cboChucVu.SelectedIndex = 1; txtHoTen.Focus();
        }
    }
}