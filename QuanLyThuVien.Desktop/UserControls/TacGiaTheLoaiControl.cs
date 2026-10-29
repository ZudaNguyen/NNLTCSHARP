using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class TacGiaTheLoaiControl : UserControl
    {
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        // Nửa trái: Bảng TheLoai
        private TextBox txtMaTheLoai, txtTenTheLoai, txtMoTaTL;
        private DataGridView dgvTheLoai;

        // Nửa phải: Bảng TacGia
        private TextBox txtMaTacGia, txtTenTacGia, txtQuocTich, txtGioiThieuTG;
        private DataGridView dgvTacGia;

        public TacGiaTheLoaiControl()
        {
            InitializeComponent();
            SetupSplitUI();
            ApDungGioiHanNhap();
            LoadMockData();
            UI.ScrollableView.Configure(this, new Size(1060, 560));
        }

        private void SetupSplitUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(18);

            // Dùng TableLayoutPanel chia đôi màn hình 50% - 50%
            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = PaperCream
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));

            // =========================================================
            // PHÂN KHU TRÁI: QUẢN LÝ THỂ LOẠI SÁCH (BẢNG TheLoai)
            // =========================================================
            Panel pnlTheLoai = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(12)
            };

            Panel pnlTLTop = new Panel { Dock = DockStyle.Top, Height = 215 };
            Label lblTLTitle = new Label
            {
                Text = "🔖 Danh Mục Thể Loại (Bảng TheLoai)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(5, 5),
                AutoSize = true
            };
            pnlTLTop.Controls.Add(lblTLTitle);

            AddLabel(pnlTLTop, "Mã TL:", 5, 38);
            txtMaTheLoai = AddTextBox(pnlTLTop, 85, 35, 90);
            txtMaTheLoai.ReadOnly = true;
            txtMaTheLoai.BackColor = Color.FromArgb(235, 230, 220);
            txtMaTheLoai.PlaceholderText = "Tự động";

            AddLabel(pnlTLTop, "Tên TL (*):", 5, 74);
            txtTenTheLoai = AddTextBox(pnlTLTop, 85, 71, 280);

            AddLabel(pnlTLTop, "Mô tả:", 5, 110);
            txtMoTaTL = new TextBox
            {
                Location = new Point(85, 107),
                Size = new Size(280, 48),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlTLTop.Controls.Add(txtMoTaTL);

            FlowLayoutPanel flpTLBtns = new FlowLayoutPanel { Location = new Point(5, 165), Size = new Size(420, 40) };
            Button btnThemTL = CreateMiniButton("➕ Thêm", ForestGreen);
            Button btnSuaTL = CreateMiniButton("✏️ Sửa", Color.FromArgb(180, 120, 20));
            Button btnXoaTL = CreateMiniButton("🗃️ Ẩn (Xóa)", Burgundy);
            Button btnMoiTL = CreateMiniButton("🔄 Mới", Color.FromArgb(90, 75, 65));

            btnThemTL.Click += BtnThemTL_Click;
            btnSuaTL.Click += BtnSuaTL_Click;
            btnXoaTL.Click += BtnXoaTL_Click;
            btnMoiTL.Click += (s, e) => { UI.GridSelection.Clear(dgvTheLoai); txtMaTheLoai.Clear(); txtTenTheLoai.Clear(); txtMoTaTL.Clear(); };

            flpTLBtns.Controls.AddRange(new Control[] { btnThemTL, btnSuaTL, btnXoaTL, btnMoiTL });
            pnlTLTop.Controls.Add(flpTLBtns);

            dgvTheLoai = CreateStyledGrid();
            dgvTheLoai.Columns.Add("MaTL", "Mã TL");
            dgvTheLoai.Columns["MaTL"].FillWeight = 45;
            dgvTheLoai.Columns.Add("TenTL", "Tên Thể Loại");
            dgvTheLoai.Columns["TenTL"].FillWeight = 120;
            dgvTheLoai.Columns.Add("MoTa", "Mô Tả");
            dgvTheLoai.Columns["MoTa"].FillWeight = 150;
            dgvTheLoai.Columns.Add("TrangThai", "Trạng Thái");
            dgvTheLoai.Columns["TrangThai"].FillWeight = 70;
            UI.GridSelection.Bind(dgvTheLoai, row =>
            {
                txtMaTheLoai.Text = row.Cells["MaTL"].Value?.ToString();
                txtTenTheLoai.Text = row.Cells["TenTL"].Value?.ToString();
                txtMoTaTL.Text = row.Cells["MoTa"].Value?.ToString();
            }, () => txtMaTheLoai.Clear(), btnSuaTL, btnXoaTL);

            pnlTheLoai.Controls.Add(dgvTheLoai);
            pnlTheLoai.Controls.Add(pnlTLTop);

            // =========================================================
            // PHÂN KHU PHẢI: QUẢN LÝ TÁC GIẢ (BẢNG TacGia)
            // =========================================================
            Panel pnlTacGia = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10, 0, 0, 0),
                Padding = new Padding(12)
            };

            Panel pnlTGTop = new Panel { Dock = DockStyle.Top, Height = 215 };
            Label lblTGTitle = new Label
            {
                Text = "✒️ Hồ Sơ Tác Giả (Bảng TacGia)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(5, 5),
                AutoSize = true
            };
            pnlTGTop.Controls.Add(lblTGTitle);

            AddLabel(pnlTGTop, "Mã TG:", 5, 38);
            txtMaTacGia = AddTextBox(pnlTGTop, 90, 35, 80);
            txtMaTacGia.ReadOnly = true;
            txtMaTacGia.BackColor = Color.FromArgb(235, 230, 220);
            txtMaTacGia.PlaceholderText = "Tự động";

            AddLabel(pnlTGTop, "Quốc tịch:", 185, 38);
            txtQuocTich = AddTextBox(pnlTGTop, 260, 35, 160);

            AddLabel(pnlTGTop, "Tên TG (*):", 5, 74);
            txtTenTacGia = AddTextBox(pnlTGTop, 90, 71, 330);

            AddLabel(pnlTGTop, "Giới thiệu:", 5, 110);
            txtGioiThieuTG = new TextBox
            {
                Location = new Point(90, 107),
                Size = new Size(330, 48),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlTGTop.Controls.Add(txtGioiThieuTG);

            FlowLayoutPanel flpTGBtns = new FlowLayoutPanel { Location = new Point(5, 165), Size = new Size(450, 40) };
            Button btnThemTG = CreateMiniButton("➕ Thêm", ForestGreen);
            Button btnSuaTG = CreateMiniButton("✏️ Sửa", Color.FromArgb(180, 120, 20));
            Button btnXoaTG = CreateMiniButton("🗃️ Ẩn (Xóa)", Burgundy);
            Button btnMoiTG = CreateMiniButton("🔄 Mới", Color.FromArgb(90, 75, 65));

            btnThemTG.Click += BtnThemTG_Click;
            btnSuaTG.Click += BtnSuaTG_Click;
            btnXoaTG.Click += BtnXoaTG_Click;
            btnMoiTG.Click += (s, e) => { UI.GridSelection.Clear(dgvTacGia); txtMaTacGia.Clear(); txtTenTacGia.Clear(); txtQuocTich.Clear(); txtGioiThieuTG.Clear(); };

            flpTGBtns.Controls.AddRange(new Control[] { btnThemTG, btnSuaTG, btnXoaTG, btnMoiTG });
            pnlTGTop.Controls.Add(flpTGBtns);

            dgvTacGia = CreateStyledGrid();
            dgvTacGia.Columns.Add("MaTG", "Mã TG");
            dgvTacGia.Columns["MaTG"].FillWeight = 45;
            dgvTacGia.Columns.Add("TenTG", "Tên Tác Giả");
            dgvTacGia.Columns["TenTG"].FillWeight = 120;
            dgvTacGia.Columns.Add("QuocTich", "Quốc Tịch");
            dgvTacGia.Columns["QuocTich"].FillWeight = 80;
            dgvTacGia.Columns.Add("GioiThieu", "Tiểu Sử / Giới Thiệu");
            dgvTacGia.Columns["GioiThieu"].FillWeight = 140;
            dgvTacGia.Columns.Add("TrangThai", "Trạng Thái");
            dgvTacGia.Columns["TrangThai"].FillWeight = 70;
            UI.GridSelection.Bind(dgvTacGia, row =>
            {
                txtMaTacGia.Text = row.Cells["MaTG"].Value?.ToString();
                txtTenTacGia.Text = row.Cells["TenTG"].Value?.ToString();
                txtQuocTich.Text = row.Cells["QuocTich"].Value?.ToString();
                txtGioiThieuTG.Text = row.Cells["GioiThieu"].Value?.ToString();
            }, () => txtMaTacGia.Clear(), btnSuaTG, btnXoaTG);

            pnlTacGia.Controls.Add(dgvTacGia);
            pnlTacGia.Controls.Add(pnlTGTop);

            // Ghép 2 phân khu vào TableLayoutPanel
            tlpMain.Controls.Add(pnlTheLoai, 0, 0);
            tlpMain.Controls.Add(pnlTacGia, 1, 0);
            this.Controls.Add(tlpMain);
        }

        private DataGridView CreateStyledGrid()
        {
            DataGridView dgv = new DataGridView
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
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 30 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            dgv.DefaultCellStyle.BackColor = CardBg;
            dgv.DefaultCellStyle.ForeColor = WoodDark;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgv.DefaultCellStyle.SelectionForeColor = WoodDark;
            return dgv;
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
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
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateMiniButton(string text, Color bg)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(92, 32),
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // Giới hạn độ dài khớp cột trong DB
        private void ApDungGioiHanNhap()
        {
            txtTenTheLoai.MaxLength = 100;   // TheLoai.TenTheLoai nvarchar(100)
            txtMoTaTL.MaxLength = 500;       // TheLoai.MoTa nvarchar(500)
            txtTenTacGia.MaxLength = 150;    // TacGia.TenTacGia nvarchar(150)
            txtQuocTich.MaxLength = 100;     // TacGia.QuocTich nvarchar(100)
            txtGioiThieuTG.MaxLength = 500;  // TacGia.GioiThieu nvarchar(500)
        }

        private static int MaTiepTheo(DataGridView dgv, string cot)
        {
            int max = 0;
            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (int.TryParse(r.Cells[cot].Value?.ToString(), out int v) && v > max) max = v;
            }
            return max + 1;
        }

        // Kiểm tra trùng tên (không phân biệt hoa/thường), tính cả dòng đã ẩn
        private static bool TrungTen(DataGridView dgv, string cotMa, string cotTen, string ten, string maHienTai)
        {
            foreach (DataGridViewRow r in dgv.Rows)
            {
                string ma = r.Cells[cotMa].Value?.ToString() ?? "";
                if (ma == maHienTai) continue;
                if (string.Equals(r.Cells[cotTen].Value?.ToString()?.Trim(), ten, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void LoadMockData()
        {
            dgvTheLoai.Rows.Add("1", "Công nghệ thông tin", "Sách lập trình, CSDL, mạng máy tính", "Hoạt động");
            dgvTheLoai.Rows.Add("2", "Văn học kinh điển", "Tiểu thuyết, tác phẩm văn học nổi tiếng", "Hoạt động");
            dgvTheLoai.Rows.Add("3", "Kinh tế - Quản trị", "Kiến thức quản lý, tài chính doanh nghiệp", "Hoạt động");
            dgvTheLoai.Rows.Add("4", "Kỹ năng sống", "Phát triển bản thân, giao tiếp", "Hoạt động");

            dgvTacGia.Rows.Add("1", "Robert C. Martin", "Mỹ", "Chuyên gia kỹ nghệ phần mềm (Uncle Bob)", "Hoạt động");
            dgvTacGia.Rows.Add("2", "Đỗ Như Tài", "Việt Nam", "Giảng viên CNTT, tác giả giáo trình C#", "Hoạt động");
            dgvTacGia.Rows.Add("3", "Paulo Coelho", "Brazil", "Tiểu thuyết gia nổi tiếng với Nhà Giả Kim", "Hoạt động");
            dgvTacGia.Rows.Add("4", "Dale Carnegie", "Mỹ", "Tác giả Đắc Nhân Tâm", "Hoạt động");
            dgvTacGia.Rows.Add("5", "Nguyễn Nhật Ánh", "Việt Nam", "Nhà văn Việt Nam", "Hoạt động");
        }

        // --- SỰ KIỆN THỂ LOẠI ---
        private void BtnThemTL_Click(object? sender, EventArgs e)
        {
            string ten = txtTenTheLoai.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Tên thể loại không được để trống!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (TrungTen(dgvTheLoai, "MaTL", "TenTL", ten, ""))
            {
                MessageBox.Show("Thể loại này đã tồn tại!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            dgvTheLoai.Rows.Add(MaTiepTheo(dgvTheLoai, "MaTL").ToString(), ten, txtMoTaTL.Text.Trim(), "Hoạt động");
            UI.GridSelection.Clear(dgvTheLoai);
            txtMaTheLoai.Clear(); txtTenTheLoai.Clear(); txtMoTaTL.Clear();
            MessageBox.Show("Đã thêm Thể loại!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSuaTL_Click(object? sender, EventArgs e)
        {
            if (dgvTheLoai.CurrentRow == null || string.IsNullOrEmpty(txtMaTheLoai.Text))
            {
                MessageBox.Show("Vui lòng chọn một thể loại dưới bảng để cập nhật!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string ten = txtTenTheLoai.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Tên thể loại không được để trống!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (TrungTen(dgvTheLoai, "MaTL", "TenTL", ten, txtMaTheLoai.Text))
            {
                MessageBox.Show("Tên thể loại này đã tồn tại!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            dgvTheLoai.CurrentRow.Cells["TenTL"].Value = ten;
            dgvTheLoai.CurrentRow.Cells["MoTa"].Value = txtMoTaTL.Text.Trim();
            MessageBox.Show("Đã cập nhật Thể loại!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXoaTL_Click(object? sender, EventArgs e)
        {
            if (dgvTheLoai.CurrentRow == null || string.IsNullOrEmpty(txtMaTheLoai.Text))
            {
                MessageBox.Show("Vui lòng chọn một thể loại dưới bảng trước khi ẩn!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string ten = dgvTheLoai.CurrentRow.Cells["TenTL"].Value?.ToString() ?? "";
            // TODO(API): chặn ẩn nếu vẫn còn sách đang dùng thể loại này
            if (MessageBox.Show($"Chuyển thể loại [{ten}] sang trạng thái Ẩn (xóa mềm)?", "Xác nhận xóa mềm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            dgvTheLoai.CurrentRow.Cells["TrangThai"].Value = "Đã ẩn";
            MessageBox.Show("Đã chuyển Thể loại sang trạng thái Ẩn (IsDeleted = 1)!", "Xóa mềm", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // --- SỰ KIỆN TÁC GIẢ ---
        private void BtnThemTG_Click(object? sender, EventArgs e)
        {
            string ten = txtTenTacGia.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Tên tác giả không được để trống!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (TrungTen(dgvTacGia, "MaTG", "TenTG", ten, ""))
            {
                MessageBox.Show("Tác giả này đã tồn tại!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            dgvTacGia.Rows.Add(MaTiepTheo(dgvTacGia, "MaTG").ToString(), ten, txtQuocTich.Text.Trim(), txtGioiThieuTG.Text.Trim(), "Hoạt động");
            UI.GridSelection.Clear(dgvTacGia);
            txtMaTacGia.Clear(); txtTenTacGia.Clear(); txtQuocTich.Clear(); txtGioiThieuTG.Clear();
            MessageBox.Show("Đã thêm Tác giả!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSuaTG_Click(object? sender, EventArgs e)
        {
            if (dgvTacGia.CurrentRow == null || string.IsNullOrEmpty(txtMaTacGia.Text))
            {
                MessageBox.Show("Vui lòng chọn một tác giả dưới bảng để cập nhật!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string ten = txtTenTacGia.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Tên tác giả không được để trống!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (TrungTen(dgvTacGia, "MaTG", "TenTG", ten, txtMaTacGia.Text))
            {
                MessageBox.Show("Tên tác giả này đã tồn tại!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            dgvTacGia.CurrentRow.Cells["TenTG"].Value = ten;
            dgvTacGia.CurrentRow.Cells["QuocTich"].Value = txtQuocTich.Text.Trim();
            dgvTacGia.CurrentRow.Cells["GioiThieu"].Value = txtGioiThieuTG.Text.Trim();
            MessageBox.Show("Đã cập nhật Tác giả!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXoaTG_Click(object? sender, EventArgs e)
        {
            if (dgvTacGia.CurrentRow == null || string.IsNullOrEmpty(txtMaTacGia.Text))
            {
                MessageBox.Show("Vui lòng chọn một tác giả dưới bảng trước khi ẩn!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string ten = dgvTacGia.CurrentRow.Cells["TenTG"].Value?.ToString() ?? "";
            // TODO(API): chặn ẩn nếu vẫn còn sách đang dùng tác giả này
            if (MessageBox.Show($"Chuyển tác giả [{ten}] sang trạng thái Ẩn (xóa mềm)?", "Xác nhận xóa mềm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            dgvTacGia.CurrentRow.Cells["TrangThai"].Value = "Đã ẩn";
            MessageBox.Show("Đã chuyển Tác giả sang trạng thái Ẩn (IsDeleted = 1)!", "Xóa mềm", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
