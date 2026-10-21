using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class SachControl : UserControl
    {
        // Bảng màu Thư viện cổ điển
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color GoldBorder = Color.FromArgb(205, 190, 165);

        // Các Control nhập liệu (Bám sát bảng Sach & Sach_TacGia)
        private TextBox txtMaSach;
        private TextBox txtTenSach;
        private TextBox txtISBN;
        private ComboBox cboTheLoai;
        private CheckedListBox clbTacGia;
        private NumericUpDown numNamXB;
        private TextBox txtNhaXB;
        private NumericUpDown numSoLuong;
        private NumericUpDown numSoLuongCon;
        private TextBox txtMoTa;
        private PictureBox picBiaSach;
        private string currentImagePath = "";

        // Các Control Tìm kiếm & Danh sách
        private TextBox txtTimKiem;
        private ComboBox cboLocTheLoai;
        private CheckBox chkHienThiDaXoa;
        private DataGridView dgvSach;

        public SachControl()
        {
            InitializeComponent();
            SetupSachUI();
            LoadMockData();
        }

        private void SetupSachUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(20);

            // ==========================================
            // 1. KHU VỰC NHẬP LIỆU CHI TIẾT SÁCH (TOP PANEL)
            // ==========================================
            Panel pnlForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 290,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblFormTitle = new Label
            {
                Text = "✒️ Thông tin Ấn phẩm & Biên mục Sách",
                Font = new Font("Georgia", 11.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlForm.Controls.Add(lblFormTitle);

            // --- CỘT 1: Mã sách, Tên sách, ISBN, Thể loại ---
            AddLabel(pnlForm, "Mã sách:", 15, 42);
            txtMaSach = AddTextBox(pnlForm, 105, 39, 90);
            txtMaSach.ReadOnly = true;
            txtMaSach.BackColor = Color.FromArgb(235, 230, 220);
            txtMaSach.PlaceholderText = "Tự động";

            AddLabel(pnlForm, "ISBN (*):", 205, 42);
            txtISBN = AddTextBox(pnlForm, 270, 39, 130);

            AddLabel(pnlForm, "Tên sách (*):", 15, 80);
            txtTenSach = AddTextBox(pnlForm, 105, 77, 295);

            AddLabel(pnlForm, "Thể loại (*):", 15, 118);
            cboTheLoai = new ComboBox
            {
                Location = new Point(105, 115),
                Width = 295,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            pnlForm.Controls.Add(cboTheLoai);

            AddLabel(pnlForm, "Nhà XB:", 15, 156);
            txtNhaXB = AddTextBox(pnlForm, 105, 153, 165);

            AddLabel(pnlForm, "Năm XB:", 280, 156);
            numNamXB = new NumericUpDown
            {
                Location = new Point(340, 153),
                Width = 60,
                Font = new Font("Segoe UI", 10),
                Minimum = 1900,
                Maximum = DateTime.Now.Year + 1,
                Value = 2026
            };
            pnlForm.Controls.Add(numNamXB);

            AddLabel(pnlForm, "Tổng SL (*):", 15, 194);
            numSoLuong = new NumericUpDown
            {
                Location = new Point(105, 191),
                Width = 85,
                Font = new Font("Segoe UI", 10),
                Minimum = 0,
                Maximum = 10000,
                Value = 10
            };
            // Khi đổi tổng số lượng lúc thêm mới, tự cập nhật số lượng còn
            numSoLuong.ValueChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtMaSach.Text))
                    numSoLuongCon.Value = numSoLuong.Value;
            };
            pnlForm.Controls.Add(numSoLuong);

            AddLabel(pnlForm, "SL Còn:", 210, 194);
            numSoLuongCon = new NumericUpDown
            {
                Location = new Point(275, 191),
                Width = 125,
                Font = new Font("Segoe UI", 10),
                Minimum = 0,
                Maximum = 10000,
                Value = 10,
                Enabled = false // Khóa cứng theo quy tắc nghiệp vụ, chỉ thay đổi qua Mượn/Trả
            };
            pnlForm.Controls.Add(numSoLuongCon);

            // --- CỘT 2: Chọn nhiều Tác giả (Bảng Sach_TacGia) & Mô tả ---
            AddLabel(pnlForm, "Tác giả (Chọn 1 hoặc nhiều):", 425, 42);
            clbTacGia = new CheckedListBox
            {
                Location = new Point(425, 65),
                Size = new Size(230, 95),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };
            pnlForm.Controls.Add(clbTacGia);

            AddLabel(pnlForm, "Mô tả chi tiết:", 425, 168);
            txtMoTa = new TextBox
            {
                Location = new Point(425, 191),
                Size = new Size(230, 42),
                Font = new Font("Segoe UI", 9.5f),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlForm.Controls.Add(txtMoTa);

            // --- CỘT 3: Ảnh bìa sách (HinhAnh) ---
            AddLabel(pnlForm, "Ảnh bìa:", 680, 42);
            picBiaSach = new PictureBox
            {
                Location = new Point(680, 65),
                Size = new Size(115, 130),
                BackColor = Color.FromArgb(238, 230, 212),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pnlForm.Controls.Add(picBiaSach);

            Button btnChonAnh = new Button
            {
                Text = "🖼️ Chọn bìa...",
                Location = new Point(680, 200),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(238, 230, 212),
                ForeColor = WoodDark,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChonAnh.FlatAppearance.BorderColor = GoldBorder;
            btnChonAnh.Click += BtnChonAnh_Click;
            pnlForm.Controls.Add(btnChonAnh);

            // --- THANH NÚT HÀNH ĐỘNG (CRUD BUTTONS) ---
            FlowLayoutPanel flpButtons = new FlowLayoutPanel
            {
                Location = new Point(15, 240),
                Size = new Size(780, 40),
                WrapContents = false
            };

            Button btnThem = CreateActionButton("➕ Ghi Sổ Mới (Thêm)", Color.FromArgb(45, 106, 79));
            Button btnSua = CreateActionButton("✏️ Tu Chỉnh (Sửa)", Color.FromArgb(180, 120, 20));
            Button btnXoa = CreateActionButton("🗃️ Lưu Kho Ẩn (Xóa mềm)", Burgundy);
            Button btnLamMoi = CreateActionButton("🔄 Làm Mới Form", Color.FromArgb(90, 75, 65));

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            flpButtons.Controls.Add(btnThem);
            flpButtons.Controls.Add(btnSua);
            flpButtons.Controls.Add(btnXoa);
            flpButtons.Controls.Add(btnLamMoi);
            pnlForm.Controls.Add(flpButtons);

            // ==========================================
            // 2. THANH TÌM KIẾM & BỘ LỌC (MIDDLE PANEL)
            // ==========================================
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = PaperCream,
                Padding = new Padding(0, 10, 0, 5)
            };

            Label lblTim = new Label
            {
                Text = "🔍 Tra cứu (Tên / ISBN / Tác giả):",
                Font = new Font("Georgia", 10, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(5, 18),
                AutoSize = true
            };

            txtTimKiem = new TextBox
            {
                Location = new Point(245, 15),
                Width = 220,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            txtTimKiem.TextChanged += (s, e) => FilterGrid();

            Label lblLocTL = new Label
            {
                Text = "Thể loại:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(480, 18),
                AutoSize = true
            };

            cboLocTheLoai = new ComboBox
            {
                Location = new Point(545, 15),
                Width = 170,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLocTheLoai.SelectedIndexChanged += (s, e) => FilterGrid();

            chkHienThiDaXoa = new CheckBox
            {
                Text = "Hiển thị sách đã ẩn (IsDeleted)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = Burgundy,
                Location = new Point(735, 17),
                AutoSize = true
            };
            chkHienThiDaXoa.CheckedChanged += (s, e) => FilterGrid();

            pnlFilter.Controls.Add(chkHienThiDaXoa);
            pnlFilter.Controls.Add(cboLocTheLoai);
            pnlFilter.Controls.Add(lblLocTL);
            pnlFilter.Controls.Add(txtTimKiem);
            pnlFilter.Controls.Add(lblTim);

            // ==========================================
            // 3. BẢNG DANH SÁCH ẤN PHẨM (DATAGRIDVIEW)
            // ==========================================
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            dgvSach = new DataGridView
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

            dgvSach.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvSach.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvSach.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);

            dgvSach.DefaultCellStyle.BackColor = CardBg;
            dgvSach.DefaultCellStyle.ForeColor = WoodDark;
            dgvSach.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgvSach.DefaultCellStyle.SelectionForeColor = WoodDark;

            dgvSach.Columns.Add("MaSach", "Mã Sách");
            dgvSach.Columns["MaSach"].FillWeight = 45;
            dgvSach.Columns.Add("ISBN", "Mã ISBN");
            dgvSach.Columns["ISBN"].FillWeight = 75;
            dgvSach.Columns.Add("TenSach", "Tên Ấn Phẩm");
            dgvSach.Columns["TenSach"].FillWeight = 150;
            dgvSach.Columns.Add("TheLoai", "Thể Loại");
            dgvSach.Columns.Add("TacGia", "Tác Giả");
            dgvSach.Columns.Add("NamXB", "Năm XB");
            dgvSach.Columns["NamXB"].FillWeight = 50;
            dgvSach.Columns.Add("NhaXB", "Nhà Xuất Bản");
            dgvSach.Columns.Add("SoLuong", "Tổng SL");
            dgvSach.Columns["SoLuong"].FillWeight = 50;
            dgvSach.Columns.Add("SoLuongCon", "SL Còn");
            dgvSach.Columns["SoLuongCon"].FillWeight = 50;
            dgvSach.Columns.Add("TrangThai", "Trạng Thái");
            dgvSach.Columns["TrangThai"].FillWeight = 70;

            dgvSach.CellClick += DgvSach_CellClick;

            pnlGrid.Controls.Add(dgvSach);

            // Add theo thứ tự ngược cho Dock
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlForm);
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ TẠO UI & VALIDATION NGHIỆP VỤ
        // ==========================================
        private void AddLabel(Control parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                AutoSize = true
            };
            parent.Controls.Add(lbl);
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

        private Button CreateActionButton(string text, Color bgColor)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(180, 35),
                Margin = new Padding(0, 0, 12, 0),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = bgColor,
                ForeColor = Color.FromArgb(255, 248, 231),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadMockData()
        {
            // Dữ liệu mẫu Thể loại
            string[] theLoais = { "Công nghệ thông tin", "Văn học kinh điển", "Kinh tế - Quản trị", "Kỹ năng sống" };
            cboTheLoai.Items.AddRange(theLoais);
            cboTheLoai.SelectedIndex = 0;

            cboLocTheLoai.Items.Add("-- Tất cả thể loại --");
            cboLocTheLoai.Items.AddRange(theLoais);
            cboLocTheLoai.SelectedIndex = 0;

            // Dữ liệu mẫu Tác giả
            clbTacGia.Items.AddRange(new object[] {
                "Robert C. Martin",
                "Nguyễn Nhật Ánh",
                "Dale Carnegie",
                "Đỗ Như Tài",
                "Paulo Coelho"
            });

            // Dữ liệu mẫu Bảng Sách
            dgvSach.Rows.Add("1", "978-0132350884", "Clean Code - Mã Sạch", "Công nghệ thông tin", "Robert C. Martin", "2022", "NXB Trẻ", "20", "15", "Hoạt động");
            dgvSach.Rows.Add("2", "978-6041145678", "Giáo trình Lập trình C# .NET", "Công nghệ thông tin", "Đỗ Như Tài", "2026", "NXB Đại học", "50", "42", "Hoạt động");
            dgvSach.Rows.Add("3", "978-6042098765", "Nhà Giả Kim", "Văn học kinh điển", "Paulo Coelho", "2020", "NXB Hội Nhà Văn", "15", "0", "Hoạt động");
            dgvSach.Rows.Add("4", "978-6041001122", "Đắc Nhân Tâm (Bản cũ)", "Kỹ năng sống", "Dale Carnegie", "2015", "NXB Tổng Hợp", "5", "5", "Đã ẩn");
        }

        private bool ValidateInput(out string selectedAuthors)
        {
            selectedAuthors = "";
            List<string> authors = new List<string>();
            foreach (var item in clbTacGia.CheckedItems)
                authors.Add(item.ToString());

            selectedAuthors = string.Join(", ", authors);

            if (string.IsNullOrWhiteSpace(txtTenSach.Text))
            {
                MessageBox.Show("Tên ấn phẩm không được để trống!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenSach.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                MessageBox.Show("Vui lòng nhập mã ISBN (Dùng để tra cứu chuẩn quốc tế)!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Focus();
                return false;
            }

            if (authors.Count == 0)
            {
                MessageBox.Show("Vui lòng tích chọn ít nhất 1 Tác giả cho ấn phẩm này!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out string tacGiaStr)) return;

            int newId = dgvSach.Rows.Count + 1;
            dgvSach.Rows.Add(
                newId.ToString(),
                txtISBN.Text.Trim(),
                txtTenSach.Text.Trim(),
                cboTheLoai.SelectedItem?.ToString(),
                tacGiaStr,
                numNamXB.Value.ToString(),
                txtNhaXB.Text.Trim(),
                numSoLuong.Value.ToString(),
                numSoLuong.Value.ToString(),
                "Hoạt động"
            );

            MessageBox.Show("Đã ghi sổ ấn phẩm mới thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSach.Text) || dgvSach.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một ấn phẩm dưới bảng trước khi tu chỉnh!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out string tacGiaStr)) return;

            DataGridViewRow row = dgvSach.CurrentRow;
            int slDangMuon = int.Parse(row.Cells["SoLuong"].Value.ToString()) - int.Parse(row.Cells["SoLuongCon"].Value.ToString());

            if (numSoLuong.Value < slDangMuon)
            {
                MessageBox.Show($"Hiện đang có {slDangMuon} cuốn được độc giả mượn. Tổng số lượng không thể nhỏ hơn {slDangMuon}!",
                                "Vi phạm quy tắc tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            row.Cells["ISBN"].Value = txtISBN.Text.Trim();
            row.Cells["TenSach"].Value = txtTenSach.Text.Trim();
            row.Cells["TheLoai"].Value = cboTheLoai.SelectedItem?.ToString();
            row.Cells["TacGia"].Value = tacGiaStr;
            row.Cells["NamXB"].Value = numNamXB.Value.ToString();
            row.Cells["NhaXB"].Value = txtNhaXB.Text.Trim();
            row.Cells["SoLuong"].Value = numSoLuong.Value.ToString();
            row.Cells["SoLuongCon"].Value = (numSoLuong.Value - slDangMuon).ToString();

            MessageBox.Show("Cập nhật thông tin ấn phẩm thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSach.Text) || dgvSach.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn ấn phẩm cần lưu kho ẩn!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc muốn chuyển sách [{txtTenSach.Text}] sang trạng thái Lưu kho ẩn (Soft Delete)?\nDữ liệu phiếu mượn cũ vẫn sẽ được bảo toàn.",
                "Xác nhận xóa mềm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                dgvSach.CurrentRow.Cells["TrangThai"].Value = "Đã ẩn";
                FilterGrid();
                ClearForm();
            }
        }

        private void DgvSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvSach.Rows[e.RowIndex];
            txtMaSach.Text = row.Cells["MaSach"].Value?.ToString();
            txtISBN.Text = row.Cells["ISBN"].Value?.ToString();
            txtTenSach.Text = row.Cells["TenSach"].Value?.ToString();
            cboTheLoai.SelectedItem = row.Cells["TheLoai"].Value?.ToString();
            numNamXB.Value = decimal.TryParse(row.Cells["NamXB"].Value?.ToString(), out decimal nam) ? nam : 2026;
            txtNhaXB.Text = row.Cells["NhaXB"].Value?.ToString();
            numSoLuong.Value = decimal.TryParse(row.Cells["SoLuong"].Value?.ToString(), out decimal sl) ? sl : 0;
            numSoLuongCon.Value = decimal.TryParse(row.Cells["SoLuongCon"].Value?.ToString(), out decimal slCon) ? slCon : 0;

            // Tích chọn lại các tác giả tương ứng
            string tacGiaCell = row.Cells["TacGia"].Value?.ToString() ?? "";
            for (int i = 0; i < clbTacGia.Items.Count; i++)
            {
                string itemText = clbTacGia.Items[i].ToString();
                clbTacGia.SetItemChecked(i, tacGiaCell.Contains(itemText));
            }
        }

        private void FilterGrid()
        {
            string keyword = txtTimKiem.Text.Trim().ToLower();
            string selectedCategory = cboLocTheLoai.SelectedIndex > 0 ? cboLocTheLoai.SelectedItem.ToString() : "";
            bool showDeleted = chkHienThiDaXoa.Checked;

            foreach (DataGridViewRow row in dgvSach.Rows)
            {
                string tenSach = row.Cells["TenSach"].Value?.ToString().ToLower() ?? "";
                string isbn = row.Cells["ISBN"].Value?.ToString().ToLower() ?? "";
                string tacGia = row.Cells["TacGia"].Value?.ToString().ToLower() ?? "";
                string theLoai = row.Cells["TheLoai"].Value?.ToString() ?? "";
                string trangThai = row.Cells["TrangThai"].Value?.ToString() ?? "";

                bool matchKeyword = string.IsNullOrEmpty(keyword) || tenSach.Contains(keyword) || isbn.Contains(keyword) || tacGia.Contains(keyword);
                bool matchCategory = string.IsNullOrEmpty(selectedCategory) || theLoai == selectedCategory;
                bool matchDeleted = showDeleted || trangThai != "Đã ẩn";

                row.Visible = matchKeyword && matchCategory && matchDeleted;
            }
        }

        private void BtnChonAnh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentImagePath = ofd.FileName;
                    picBiaSach.Image = Image.FromFile(currentImagePath);
                }
            }
        }

        private void ClearForm()
        {
            txtMaSach.Clear();
            txtISBN.Clear();
            txtTenSach.Clear();
            txtNhaXB.Clear();
            txtMoTa.Clear();
            numNamXB.Value = 2026;
            numSoLuong.Value = 10;
            numSoLuongCon.Value = 10;
            picBiaSach.Image = null;
            for (int i = 0; i < clbTacGia.Items.Count; i++)
                clbTacGia.SetItemChecked(i, false);
            txtTenSach.Focus();
        }
    }
}