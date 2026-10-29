using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class DocGiaControl : UserControl
    {
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        // Controls bám sát bảng DocGia
        private TextBox txtMaDocGia;
        private TextBox txtHoTen;
        private DateTimePicker dtpNgaySinh;
        private ComboBox cboGioiTinh;
        private TextBox txtSoDienThoai;
        private TextBox txtEmail;
        private TextBox txtDiaChi;
        private DateTimePicker dtpNgayDangKy;
        private ComboBox cboTrangThai;

        // Tìm kiếm & Bảng
        private TextBox txtTimKiem;
        private ComboBox cboLocTrangThai;
        private DataGridView dgvDocGia;

        public DocGiaControl()
        {
            InitializeComponent();
            SetupDocGiaUI();
            LoadMockData();
        }

        private void SetupDocGiaUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(20);

            // ==========================================
            // 1. FORM THÔNG TIN THẺ ĐỘC GIẢ (TOP PANEL)
            // ==========================================
            Panel pnlForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 235,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "🪪 Hồ sơ Đăng ký & Quản lý Thẻ Độc Giả (Bảng DocGia)",
                Font = new Font("Georgia", 11.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlForm.Controls.Add(lblTitle);

            // Cột 1: Mã ĐG, Họ tên, Ngày sinh, Giới tính
            AddLabel(pnlForm, "Mã thẻ ĐG:", 15, 45);
            txtMaDocGia = AddTextBox(pnlForm, 105, 42, 95);
            txtMaDocGia.ReadOnly = true;
            txtMaDocGia.BackColor = Color.FromArgb(235, 230, 220);
            txtMaDocGia.PlaceholderText = "Tự động";

            AddLabel(pnlForm, "Giới tính:", 215, 45);
            cboGioiTinh = new ComboBox
            {
                Location = new Point(285, 42),
                Width = 105,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cboGioiTinh.SelectedIndex = 0;
            pnlForm.Controls.Add(cboGioiTinh);

            AddLabel(pnlForm, "Họ và tên (*):", 15, 85);
            txtHoTen = AddTextBox(pnlForm, 105, 82, 285);

            AddLabel(pnlForm, "Ngày sinh:", 15, 125);
            dtpNgaySinh = new DateTimePicker
            {
                Location = new Point(105, 122),
                Width = 135,
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2005, 1, 1)
            };
            pnlForm.Controls.Add(dtpNgaySinh);

            // Cột 2: SĐT, Email, Địa chỉ, Ngày ĐK, Trạng thái
            AddLabel(pnlForm, "Điện thoại (*):", 420, 45);
            txtSoDienThoai = AddTextBox(pnlForm, 520, 42, 150);

            AddLabel(pnlForm, "Ngày cấp thẻ:", 685, 45);
            dtpNgayDangKy = new DateTimePicker
            {
                Location = new Point(785, 42),
                Width = 130,
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Enabled = false
            };
            pnlForm.Controls.Add(dtpNgayDangKy);

            AddLabel(pnlForm, "Email (Unique):", 420, 85);
            txtEmail = AddTextBox(pnlForm, 520, 82, 200);

            AddLabel(pnlForm, "Trạng thái:", 730, 85);
            cboTrangThai = new ComboBox
            {
                Location = new Point(805, 82),
                Width = 110,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTrangThai.Items.AddRange(new object[] { "Hoạt động", "Khóa thẻ" });
            cboTrangThai.SelectedIndex = 0;
            pnlForm.Controls.Add(cboTrangThai);

            AddLabel(pnlForm, "Địa chỉ:", 420, 125);
            txtDiaChi = AddTextBox(pnlForm, 520, 122, 395);

            // Các nút chức năng
            FlowLayoutPanel flpButtons = new FlowLayoutPanel
            {
                Location = new Point(15, 175),
                Size = new Size(900, 42),
                WrapContents = false
            };

            Button btnThem = CreateButton("➕ Cấp Thẻ Mới (Thêm)", ForestGreen);
            Button btnSua = CreateButton("✏️ Cập Nhật Hồ Sơ", Color.FromArgb(180, 120, 20));
            Button btnKhoaMo = CreateButton("🔒 Khóa / Mở Thẻ", Burgundy);
            Button btnLamMoi = CreateButton("🔄 Làm Mới Form", Color.FromArgb(90, 75, 65));

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnKhoaMo.Click += BtnKhoaMo_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            flpButtons.Controls.AddRange(new Control[] { btnThem, btnSua, btnKhoaMo, btnLamMoi });
            pnlForm.Controls.Add(flpButtons);

            // ==========================================
            // 2. THANH TÌM KIẾM & LỌC (MIDDLE PANEL)
            // ==========================================
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = PaperCream
            };

            Label lblTim = new Label
            {
                Text = "🔍 Tìm độc giả (Họ tên / SĐT / Email):",
                Font = new Font("Georgia", 10, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(5, 16),
                AutoSize = true
            };

            txtTimKiem = new TextBox
            {
                Location = new Point(275, 13),
                Width = 240,
                Font = new Font("Segoe UI", 10)
            };
            txtTimKiem.TextChanged += (s, e) => FilterGrid();

            Label lblLoc = new Label
            {
                Text = "Trạng thái thẻ:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(535, 16),
                AutoSize = true
            };

            cboLocTrangThai = new ComboBox
            {
                Location = new Point(645, 13),
                Width = 150,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLocTrangThai.Items.AddRange(new object[] { "-- Tất cả --", "Hoạt động", "Khóa thẻ" });
            cboLocTrangThai.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndexChanged += (s, e) => FilterGrid();

            pnlFilter.Controls.AddRange(new Control[] { lblTim, txtTimKiem, lblLoc, cboLocTrangThai });

            // ==========================================
            // 3. BẢNG DANH SÁCH ĐỘC GIẢ (BOTTOM FILL)
            // ==========================================
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            dgvDocGia = new DataGridView
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

            dgvDocGia.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvDocGia.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvDocGia.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);

            dgvDocGia.DefaultCellStyle.BackColor = CardBg;
            dgvDocGia.DefaultCellStyle.ForeColor = WoodDark;
            dgvDocGia.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgvDocGia.DefaultCellStyle.SelectionForeColor = WoodDark;

            dgvDocGia.Columns.Add("MaDocGia", "Mã ĐG");
            dgvDocGia.Columns["MaDocGia"].FillWeight = 50;
            dgvDocGia.Columns.Add("HoTen", "Họ và Tên");
            dgvDocGia.Columns["HoTen"].FillWeight = 130;
            dgvDocGia.Columns.Add("NgaySinh", "Ngày Sinh");
            dgvDocGia.Columns["NgaySinh"].FillWeight = 75;
            dgvDocGia.Columns.Add("GioiTinh", "Giới Tính");
            dgvDocGia.Columns["GioiTinh"].FillWeight = 55;
            dgvDocGia.Columns.Add("SoDienThoai", "Số Điện Thoại");
            dgvDocGia.Columns["SoDienThoai"].FillWeight = 85;
            dgvDocGia.Columns.Add("Email", "Email");
            dgvDocGia.Columns["Email"].FillWeight = 130;
            dgvDocGia.Columns.Add("DiaChi", "Địa Chỉ");
            dgvDocGia.Columns["DiaChi"].FillWeight = 140;
            dgvDocGia.Columns.Add("NgayDangKy", "Ngày ĐK");
            dgvDocGia.Columns["NgayDangKy"].FillWeight = 75;
            dgvDocGia.Columns.Add("TrangThai", "Trạng Thái");
            dgvDocGia.Columns["TrangThai"].FillWeight = 75;

            dgvDocGia.CellClick += DgvDocGia_CellClick;
            pnlGrid.Controls.Add(dgvDocGia);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlForm);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                AutoSize = true
            });
        }

        private TextBox AddTextBox(Control parent, int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateButton(string text, Color bg)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(195, 36),
                Margin = new Padding(0, 0, 12, 0),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadMockData()
        {
            dgvDocGia.Rows.Add("1", "Nguyễn Văn A", "15/05/2004", "Nam", "0901234567", "nguyenvana@gmail.com", "Quận 5, TP.HCM", "10/01/2026", "Hoạt động");
            dgvDocGia.Rows.Add("2", "Trần Thị B", "20/11/2005", "Nữ", "0912345678", "tranthib@gmail.com", "Quận 1, TP.HCM", "12/02/2026", "Hoạt động");
            dgvDocGia.Rows.Add("3", "Lê Hoàng C", "08/03/2003", "Nam", "0987654321", "lehoangc@gmail.com", "Nhà Bè, TP.HCM", "05/03/2026", "Khóa thẻ");
        }

        private bool ValidateDocGia(string currentId = "")
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Họ và tên độc giả không được để trống!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoTen.Focus();
                return false;
            }

            if (!Regex.IsMatch(txtSoDienThoai.Text.Trim(), @"^0\d{9,10}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ (Phải bắt đầu bằng số 0 và có 10-11 chữ số)!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoDienThoai.Focus();
                return false;
            }

            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Định dạng Email không hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }

                // Kiểm tra Unique Email theo yêu cầu rà soát Database
                foreach (DataGridViewRow row in dgvDocGia.Rows)
                {
                    string rowId = row.Cells["MaDocGia"].Value?.ToString();
                    string rowEmail = row.Cells["Email"].Value?.ToString();
                    if (rowId != currentId && string.Equals(rowEmail, email, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Email này đã được đăng ký cho một độc giả khác (Vi phạm Unique Index)!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateDocGia()) return;

            int newId = dgvDocGia.Rows.Count + 1;
            dgvDocGia.Rows.Add(
                newId.ToString(),
                txtHoTen.Text.Trim(),
                dtpNgaySinh.Value.ToString("dd/MM/yyyy"),
                cboGioiTinh.SelectedItem.ToString(),
                txtSoDienThoai.Text.Trim(),
                txtEmail.Text.Trim(),
                txtDiaChi.Text.Trim(),
                DateTime.Now.ToString("dd/MM/yyyy"),
                cboTrangThai.SelectedItem.ToString()
            );

            MessageBox.Show("Đã cấp thẻ độc giả mới thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaDocGia.Text) || dgvDocGia.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một độc giả dưới bảng để cập nhật!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateDocGia(txtMaDocGia.Text)) return;

            DataGridViewRow row = dgvDocGia.CurrentRow;
            row.Cells["HoTen"].Value = txtHoTen.Text.Trim();
            row.Cells["NgaySinh"].Value = dtpNgaySinh.Value.ToString("dd/MM/yyyy");
            row.Cells["GioiTinh"].Value = cboGioiTinh.SelectedItem.ToString();
            row.Cells["SoDienThoai"].Value = txtSoDienThoai.Text.Trim();
            row.Cells["Email"].Value = txtEmail.Text.Trim();
            row.Cells["DiaChi"].Value = txtDiaChi.Text.Trim();
            row.Cells["TrangThai"].Value = cboTrangThai.SelectedItem.ToString();

            MessageBox.Show("Cập nhật hồ sơ độc giả thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnKhoaMo_Click(object sender, EventArgs e)
        {
            if (dgvDocGia.CurrentRow == null) return;

            string currentStatus = dgvDocGia.CurrentRow.Cells["TrangThai"].Value.ToString();
            string newStatus = currentStatus == "Hoạt động" ? "Khóa thẻ" : "Hoạt động";

            // Quy tắc: Không xóa cứng dữ liệu đã phát sinh phiếu, chỉ khóa thẻ
            dgvDocGia.CurrentRow.Cells["TrangThai"].Value = newStatus;
            cboTrangThai.SelectedItem = newStatus;

            MessageBox.Show($"Đã chuyển trạng thái thẻ của độc giả sang [{newStatus}]!", "Phân quyền thẻ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DgvDocGia_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvDocGia.Rows[e.RowIndex];

            txtMaDocGia.Text = row.Cells["MaDocGia"].Value?.ToString();
            txtHoTen.Text = row.Cells["HoTen"].Value?.ToString();
            cboGioiTinh.SelectedItem = row.Cells["GioiTinh"].Value?.ToString();
            txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
            txtEmail.Text = row.Cells["Email"].Value?.ToString();
            txtDiaChi.Text = row.Cells["DiaChi"].Value?.ToString();
            cboTrangThai.SelectedItem = row.Cells["TrangThai"].Value?.ToString();

            if (DateTime.TryParseExact(row.Cells["NgaySinh"].Value?.ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ns))
                dtpNgaySinh.Value = ns;
            if (DateTime.TryParseExact(row.Cells["NgayDangKy"].Value?.ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ndk))
                dtpNgayDangKy.Value = ndk;
        }

        private void FilterGrid()
        {
            string kw = txtTimKiem.Text.Trim().ToLower();
            string st = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem.ToString() : "";

            foreach (DataGridViewRow row in dgvDocGia.Rows)
            {
                string ten = row.Cells["HoTen"].Value?.ToString().ToLower() ?? "";
                string sdt = row.Cells["SoDienThoai"].Value?.ToString().ToLower() ?? "";
                string mail = row.Cells["Email"].Value?.ToString().ToLower() ?? "";
                string trangThai = row.Cells["TrangThai"].Value?.ToString() ?? "";

                bool matchKw = string.IsNullOrEmpty(kw) || ten.Contains(kw) || sdt.Contains(kw) || mail.Contains(kw);
                bool matchSt = string.IsNullOrEmpty(st) || trangThai == st;

                row.Visible = matchKw && matchSt;
            }
        }

        private void ClearForm()
        {
            txtMaDocGia.Clear();
            txtHoTen.Clear();
            txtSoDienThoai.Clear();
            txtEmail.Clear();
            txtDiaChi.Clear();
            cboGioiTinh.SelectedIndex = 0;
            cboTrangThai.SelectedIndex = 0;
            dtpNgayDangKy.Value = DateTime.Now;
            txtHoTen.Focus();
        }
    }
}