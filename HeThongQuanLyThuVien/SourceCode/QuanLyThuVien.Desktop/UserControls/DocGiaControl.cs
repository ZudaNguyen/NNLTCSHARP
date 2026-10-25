using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class DocGiaControl : UserControl
    {
        private readonly DocGiaApiClient _apiClient = new();
        private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 300 };
        private CancellationTokenSource? _loadCancellation;
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
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
            Load += async (_, _) => await LoadReadersAsync();
            _filterTimer.Tick += async (_, _) =>
            {
                _filterTimer.Stop();
                await LoadReadersAsync();
            };
        }

        private void SetupDocGiaUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(14);

            var pnlForm = new Panel
            {
                Dock = DockStyle.Top, Height = 284, BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10), AutoScroll = true,
                AutoScrollMinSize = new Size(760, 0)
            };
            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 7,
                BackColor = CardBg, Padding = new Padding(2), Margin = Padding.Empty
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            for (var row = 1; row <= 5; row++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            fields.Controls.Add(new Label
            {
                Text = "🪪 Hồ sơ đăng ký & quản lý thẻ độc giả", Dock = DockStyle.Fill,
                Font = new Font("Georgia", 11.5f, FontStyle.Bold), ForeColor = Burgundy,
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Margin = new Padding(3)
            }, 0, 0);
            fields.SetColumnSpan(fields.GetControlFromPosition(0, 0)!, 4);

            txtMaDocGia = AddTextBox();
            txtMaDocGia.ReadOnly = true;
            txtMaDocGia.BackColor = Color.FromArgb(235, 230, 220);
            txtMaDocGia.PlaceholderText = "Tự động";
            cboGioiTinh = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 4, 6, 4) };
            cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" }); cboGioiTinh.SelectedIndex = 0;
            txtHoTen = AddTextBox();
            dtpNgaySinh = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Value = new DateTime(2005, 1, 1), Margin = new Padding(3, 4, 6, 4) };
            dtpNgayDangKy = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Value = DateTime.Now, Enabled = false, Margin = new Padding(3, 4, 6, 4) };
            txtSoDienThoai = AddTextBox();
            txtEmail = AddTextBox();
            txtDiaChi = AddTextBox();
            cboTrangThai = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 4, 6, 4) };
            cboTrangThai.Items.AddRange(new object[] { "Hoạt động", "Khóa thẻ" }); cboTrangThai.SelectedIndex = 0;
            AddField(fields, "Mã thẻ:", txtMaDocGia, 0, 1);
            AddField(fields, "Trạng thái:", cboTrangThai, 2, 1);
            AddField(fields, "Họ và tên:", txtHoTen, 0, 2);
            AddField(fields, "Giới tính:", cboGioiTinh, 2, 2);
            AddField(fields, "Ngày sinh:", dtpNgaySinh, 0, 3);
            AddField(fields, "Ngày cấp thẻ:", dtpNgayDangKy, 2, 3);
            AddField(fields, "Điện thoại:", txtSoDienThoai, 0, 4);
            AddField(fields, "Email:", txtEmail, 2, 4);
            fields.Controls.Add(CreateFieldLabel("Địa chỉ:"), 0, 5);
            fields.Controls.Add(txtDiaChi, 1, 5);
            fields.SetColumnSpan(txtDiaChi, 3);

            var flpButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = true, AutoScroll = false,
                FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0, 3, 0, 0)
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
            fields.Controls.Add(flpButtons, 0, 6);
            fields.SetColumnSpan(flpButtons, 4);
            pnlForm.Controls.Add(fields);

            var pnlFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 62, WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight, AutoScroll = false,
                BackColor = PaperCream, Padding = new Padding(2, 7, 2, 3)
            };
            var lblTim = CreateFieldLabel("🔍 Tìm độc giả:"); lblTim.Width = 120;
            txtTimKiem = new TextBox { Width = 235, Font = new Font("Segoe UI", 10), Margin = new Padding(3, 5, 12, 3) };
            txtTimKiem.TextChanged += (s, e) => ScheduleFilter();
            var lblLoc = CreateFieldLabel("Trạng thái thẻ:"); lblLoc.Width = 110;
            cboLocTrangThai = new ComboBox { Width = 150, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 4, 8, 3) };
            cboLocTrangThai.Items.AddRange(new object[] { "-- Tất cả --", "Hoạt động", "Khóa thẻ" }); cboLocTrangThai.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndexChanged += (s, e) => ScheduleFilter();
            pnlFilter.Controls.AddRange(new Control[] { lblTim, txtTimKiem, lblLoc, cboLocTrangThai });

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
            QuanLyThuVien.Desktop.Services.DataGridViewStyleHelper.ApplyLibraryStyle(dgvDocGia);

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

            var pageLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = PaperCream, Margin = Padding.Empty, Padding = Padding.Empty
            };
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 284));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnlForm.Dock = DockStyle.Fill;
            pnlFilter.Dock = DockStyle.Fill;
            pnlGrid.Dock = DockStyle.Fill;
            pageLayout.Controls.Add(pnlForm, 0, 0);
            pageLayout.Controls.Add(pnlFilter, 0, 1);
            pageLayout.Controls.Add(pnlGrid, 0, 2);
            Controls.Add(pageLayout);
        }

        private static Label CreateFieldLabel(string text) => new()
        {
            Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 30, 22),
            AutoEllipsis = true, Margin = new Padding(3)
        };

        private static void AddField(TableLayoutPanel layout, string label, Control input, int labelColumn, int row)
        {
            layout.Controls.Add(CreateFieldLabel(label), labelColumn, row);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 4, 6, 4);
            layout.Controls.Add(input, labelColumn + 1, row);
        }

        private static TextBox AddTextBox()
        {
            TextBox txt = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(3, 4, 6, 4)
            };
            return txt;
        }

        private Button CreateButton(string text, Color bg)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(170, 36),
                Margin = new Padding(0, 0, 6, 0),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
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

            }

            return true;
        }

        private async void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateDocGia()) return;
            try
            {
                await _apiClient.CreateAsync(new CreateDocGiaDto
                {
                    HoTen = txtHoTen.Text.Trim(),
                    NgaySinh = dtpNgaySinh.Value.Date,
                    GioiTinh = cboGioiTinh.SelectedItem?.ToString() ?? "Nam",
                    SoDienThoai = txtSoDienThoai.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    DiaChi = txtDiaChi.Text.Trim(),
                    TrangThai = cboTrangThai.SelectedItem?.ToString() ?? "Hoạt động"
                }, CancellationToken.None);
                await LoadReadersAsync();
                MessageBox.Show("Đã cấp thẻ độc giả mới thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception exception) { ShowApiError(exception); }
        }

        private async void BtnSua_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaDocGia.Text) || dgvDocGia.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một độc giả dưới bảng để cập nhật!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateDocGia(txtMaDocGia.Text)) return;

            if (!int.TryParse(txtMaDocGia.Text, out var id)) return;
            try
            {
                var updated = await _apiClient.UpdateAsync(id, new UpdateDocGiaDto
                {
                    HoTen = txtHoTen.Text.Trim(),
                    NgaySinh = dtpNgaySinh.Value.Date,
                    GioiTinh = cboGioiTinh.SelectedItem?.ToString() ?? "Nam",
                    SoDienThoai = txtSoDienThoai.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    DiaChi = txtDiaChi.Text.Trim()
                }, CancellationToken.None);
                await LoadReadersAsync();
                SelectReader(updated.MaDocGia);
                MessageBox.Show("Cập nhật hồ sơ độc giả thành công!", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) { ShowApiError(exception); }
        }

        private async void BtnKhoaMo_Click(object? sender, EventArgs e)
        {
            if (dgvDocGia.CurrentRow == null) return;
            if (!int.TryParse(dgvDocGia.CurrentRow.Cells["MaDocGia"].Value?.ToString(), out var id)) return;
            var currentStatus = dgvDocGia.CurrentRow.Cells["TrangThai"].Value?.ToString() ?? "Hoạt động";
            var newStatus = currentStatus == "Hoạt động" ? "Khóa thẻ" : "Hoạt động";
            try
            {
                await _apiClient.UpdateTrangThaiAsync(id, newStatus, CancellationToken.None);
                await LoadReadersAsync();
                SelectReader(id);
                MessageBox.Show($"Đã chuyển trạng thái thẻ của độc giả sang [{newStatus}]!", "Phân quyền thẻ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) { ShowApiError(exception); }
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

        private void ScheduleFilter()
        {
            _filterTimer.Stop();
            _filterTimer.Start();
        }

        private async Task LoadReadersAsync()
        {
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
            _loadCancellation = new CancellationTokenSource();
            var token = _loadCancellation.Token;
            var status = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem?.ToString() : null;
            try
            {
                var readers = await _apiClient.GetAllAsync(txtTimKiem.Text, status, token);
                dgvDocGia.Rows.Clear();
                foreach (var reader in readers)
                    dgvDocGia.Rows.Add(reader.MaDocGia, reader.HoTen, reader.NgaySinh.ToString("dd/MM/yyyy"), reader.GioiTinh,
                        reader.SoDienThoai, reader.Email ?? string.Empty, reader.DiaChi ?? string.Empty,
                        reader.NgayDangKy.ToString("dd/MM/yyyy"), reader.TrangThai);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception) { ShowApiError(exception); }
        }

        private void SelectReader(int id)
        {
            foreach (DataGridViewRow row in dgvDocGia.Rows)
            {
                if (row.Cells["MaDocGia"].Value?.ToString() != id.ToString()) continue;
                row.Selected = true;
                dgvDocGia.CurrentCell = row.Cells[0];
                DgvDocGia_CellClick(dgvDocGia, new DataGridViewCellEventArgs(0, row.Index));
                return;
            }
        }

        private static void ShowApiError(Exception exception)
        {
            var message = exception is HttpRequestException
                ? $"Lỗi API độc giả: {exception.Message}"
                : $"Không thể xử lý dữ liệu độc giả: {exception.Message}";
            MessageBox.Show(message, "Lỗi API độc giả", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
