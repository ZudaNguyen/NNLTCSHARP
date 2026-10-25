using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class NhanVienControl : UserControl
    {
        private readonly NhanVienApiClient _apiClient = new();
        private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 350 };
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        private TextBox txtMaNhanVien, txtHoTen, txtSoDienThoai, txtEmail, txtTimKiem;
        private ComboBox cboChucVu, cboLocChucVu, cboLocTrangThai;
        private DataGridView dgvNhanVien;
        private Button btnThem, btnSua, btnTrangThai;
        private CancellationTokenSource? _loadCancellation;

        public NhanVienControl()
        {
            InitializeComponent();
            SetupNhanVienUI();
            Load += async (_, _) => await LoadEmployeesAsync();
            _filterTimer.Tick += async (_, _) =>
            {
                _filterTimer.Stop();
                await LoadEmployeesAsync();
            };
        }

        private void SetupNhanVienUI()
        {
            Dock = DockStyle.Fill;
            BackColor = PaperCream;
            Padding = new Padding(20);

            var pnlForm = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle };
            pnlForm.Controls.Add(new Label
            {
                Text = "Hồ sơ Nhân viên Thư viện", Font = new Font("Georgia", 11.5f, FontStyle.Bold),
                ForeColor = Burgundy, Location = new Point(15, 10), AutoSize = true
            });

            AddLabel(pnlForm, "Mã NV:", 15, 45);
            txtMaNhanVien = AddTextBox(pnlForm, 105, 42, 95);
            txtMaNhanVien.ReadOnly = true;
            txtMaNhanVien.BackColor = Color.FromArgb(235, 230, 220);
            txtMaNhanVien.PlaceholderText = "Tự động";
            AddLabel(pnlForm, "Họ và tên:", 15, 85);
            txtHoTen = AddTextBox(pnlForm, 105, 82, 285);
            AddLabel(pnlForm, "Điện thoại:", 15, 125);
            txtSoDienThoai = AddTextBox(pnlForm, 105, 122, 130);
            AddLabel(pnlForm, "Email:", 245, 125);
            txtEmail = AddTextBox(pnlForm, 295, 122, 225);

            AddLabel(pnlForm, "Chức vụ:", 530, 45);
            cboChucVu = new ComboBox
            {
                Location = new Point(650, 42), Width = 180, Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            cboChucVu.Items.AddRange(new object[] { "Thủ thư", "Nhân viên" });
            cboChucVu.Text = "Thủ thư";
            pnlForm.Controls.Add(cboChucVu);
            var lblSecurity = new Label
            {
                Text = "Thông tin đăng nhập và phân quyền được quản lý riêng; màn hình này chỉ quản lý hồ sơ nhân viên.",
                Location = new Point(530, 82), Size = new Size(350, 48), Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Burgundy
            };
            pnlForm.Controls.Add(lblSecurity);

            var flpButtons = new FlowLayoutPanel { Location = new Point(15, 165), Size = new Size(880, 78), WrapContents = true, AutoScroll = false, Anchor = AnchorStyles.Top | AnchorStyles.Left, Padding = new Padding(0, 0, 6, 0) };
            btnThem = CreateButton("➕ Thêm nhân viên", ForestGreen);
            btnSua = CreateButton("✏️ Cập nhật hồ sơ", Color.FromArgb(180, 120, 20));
            btnTrangThai = CreateButton("🔒 Ngừng hoạt động", Burgundy);
            var btnLichSu = CreateButton("📜 Lịch sử", Color.FromArgb(74, 53, 37));
            var btnLamMoi = CreateButton("🔄 Làm mới Form", Color.FromArgb(90, 75, 65));
            btnThem.Click += async (_, _) => await CreateEmployeeAsync();
            btnSua.Click += async (_, _) => await UpdateEmployeeAsync();
            btnTrangThai.Click += async (_, _) => await ToggleStatusAsync();
            btnLichSu.Click += async (_, _) => await ViewStatusHistoryAsync();
            btnLamMoi.Click += (_, _) => ClearForm();
            flpButtons.Controls.AddRange(new Control[] { btnThem, btnSua, btnTrangThai, btnLichSu, btnLamMoi });
            pnlForm.Controls.Add(flpButtons);

            var pnlFilter = new FlowLayoutPanel { Dock = DockStyle.Fill, Height = 56, BackColor = PaperCream, WrapContents = true, AutoScroll = false, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(2, 8, 2, 2) };
            pnlFilter.Controls.Add(new Label { Text = "Tra cứu:", Font = new Font("Georgia", 10, FontStyle.Bold), ForeColor = WoodDark, Width = 75, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3) });
            txtTimKiem = new TextBox { Width = 235, Font = new Font("Segoe UI", 10), Margin = new Padding(3, 4, 8, 2) };
            txtTimKiem.TextChanged += (_, _) => ScheduleLoad();
            cboLocChucVu = CreateFilterCombo(0, 0, 180, "Tất cả chức vụ"); cboLocChucVu.Margin = new Padding(3, 4, 6, 2);
            cboLocTrangThai = CreateFilterCombo(0, 0, 180, "Hoạt động"); cboLocTrangThai.Margin = new Padding(3, 4, 6, 2);
            cboLocTrangThai.Items.Insert(1, "Ngừng hoạt động");
            cboLocTrangThai.Items.Insert(2, "Tất cả trạng thái");
            cboLocChucVu.SelectedIndexChanged += FilterComboChanged;
            cboLocTrangThai.SelectedIndexChanged += (_, _) => ScheduleLoad();
            pnlFilter.Controls.AddRange(new Control[] { txtTimKiem, cboLocChucVu, cboLocTrangThai });

            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            dgvNhanVien = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = CardBg, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
                EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 }, GridColor = Color.FromArgb(225, 215, 195)
            };
            dgvNhanVien.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvNhanVien.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
            QuanLyThuVien.Desktop.Services.DataGridViewStyleHelper.ApplyLibraryStyle(dgvNhanVien);
            dgvNhanVien.Columns.Add("MaNV", "Mã NV"); dgvNhanVien.Columns["MaNV"].FillWeight = 50;
            dgvNhanVien.Columns.Add("HoTen", "Họ và Tên"); dgvNhanVien.Columns["HoTen"].FillWeight = 140;
            dgvNhanVien.Columns.Add("SoDienThoai", "Số Điện Thoại"); dgvNhanVien.Columns["SoDienThoai"].FillWeight = 90;
            dgvNhanVien.Columns.Add("Email", "Email"); dgvNhanVien.Columns["Email"].FillWeight = 130;
            dgvNhanVien.Columns.Add("ChucVu", "Chức Vụ"); dgvNhanVien.Columns["ChucVu"].FillWeight = 85;
            dgvNhanVien.Columns.Add("TrangThai", "Trạng Thái"); dgvNhanVien.Columns["TrangThai"].FillWeight = 90;
            dgvNhanVien.CellClick += DgvNhanVien_CellClick;
            dgvNhanVien.SelectionChanged += (_, _) => UpdateStatusButton();
            pnlGrid.Controls.Add(dgvNhanVien);
            var pageLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = PaperCream, Margin = Padding.Empty, Padding = Padding.Empty };
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 255));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pageLayout.Controls.Add(pnlForm, 0, 0); pageLayout.Controls.Add(pnlFilter, 0, 1); pageLayout.Controls.Add(pnlGrid, 0, 2);
            Controls.Add(pageLayout);
        }

        private ComboBox CreateFilterCombo(int x, int y, int width, string first)
        {
            var combo = new ComboBox { Location = new Point(x, y), Width = width, Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Items.Add(first);
            combo.SelectedIndex = 0;
            return combo;
        }

        private void AddLabel(Control parent, string text, int x, int y) =>
            parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark, AutoSize = true });

        private TextBox AddTextBox(Control parent, int x, int y, int width)
        {
            var txt = new TextBox { Location = new Point(x, y), Width = width, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateButton(string text, Color bg)
        {
            var button = new Button { Text = text, Size = new Size(165, 36), Margin = new Padding(0, 0, 12, 0), Font = new Font("Georgia", 9.5f, FontStyle.Bold), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void ScheduleLoad()
        {
            _filterTimer.Stop();
            _filterTimer.Start();
        }

        private async Task LoadEmployeesAsync()
        {
            if (IsDisposed || !IsHandleCreated) return;
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
            _loadCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            try
            {
                var status = cboLocTrangThai.SelectedItem?.ToString();
                if (status == "Tất cả trạng thái") status = null;
                var job = cboLocChucVu.SelectedItem?.ToString();
                if (job == "Tất cả chức vụ") job = null;
                var employees = await _apiClient.GetAllAsync(txtTimKiem.Text, status, job, _loadCancellation.Token);
                dgvNhanVien.Rows.Clear();
                foreach (var employee in employees)
                    dgvNhanVien.Rows.Add(employee.MaNhanVien, employee.HoTen, employee.SoDienThoai, employee.Email, employee.ChucVu, employee.TrangThai);
                var selectedJob = cboLocChucVu.SelectedItem?.ToString();
                var jobs = employees.Select(x => x.ChucVu).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
                cboLocChucVu.SelectedIndexChanged -= FilterComboChanged;
                cboLocChucVu.Items.Clear(); cboLocChucVu.Items.Add("Tất cả chức vụ"); cboLocChucVu.Items.AddRange(jobs.Cast<object>().ToArray());
                cboLocChucVu.SelectedItem = jobs.Contains(selectedJob ?? "") ? selectedJob : "Tất cả chức vụ";
                cboLocChucVu.SelectedIndexChanged += FilterComboChanged;
                UpdateStatusButton();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!IsDisposed) MessageBox.Show($"Không tải được danh sách nhân viên. Kiểm tra API ({Environment.GetEnvironmentVariable("QUANLYTHUVIEN_API_URL") ?? "http://localhost:5058/"}).\n{ex.Message}", "Lỗi API nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterComboChanged(object? sender, EventArgs e) => ScheduleLoad();

        private async Task CreateEmployeeAsync()
        {
            if (!ValidateInput()) return;
            try
            {
                SetBusy(true);
                var created = await _apiClient.CreateAsync(BuildCreateDto(), CancellationToken.None);
                await LoadEmployeesAsync();
                ClearForm();
                MessageBox.Show($"Đã thêm nhân viên #{created.MaNhanVien}.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowApiError(ex); }
            finally { SetBusy(false); }
        }

        private async Task UpdateEmployeeAsync()
        {
            if (!int.TryParse(txtMaNhanVien.Text, out var id))
            {
                MessageBox.Show("Chọn nhân viên cần cập nhật.", "Thiếu nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput()) return;
            try
            {
                SetBusy(true);
                await _apiClient.UpdateAsync(id, BuildUpdateDto(), CancellationToken.None);
                await LoadEmployeesAsync();
                MessageBox.Show("Đã cập nhật hồ sơ nhân viên.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowApiError(ex); }
            finally { SetBusy(false); }
        }

        private async Task ToggleStatusAsync()
        {
            if (!int.TryParse(txtMaNhanVien.Text, out var id) || dgvNhanVien.CurrentRow == null) return;
            var current = dgvNhanVien.CurrentRow.Cells["TrangThai"].Value?.ToString();
            var next = current == "Hoạt động" ? "Ngừng hoạt động" : "Hoạt động";
            var prompt = next == "Ngừng hoạt động" ? "ngừng hoạt động" : "khôi phục hoạt động";
            if (MessageBox.Show($"Bạn muốn {prompt} nhân viên này?", "Xác nhận trạng thái", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                SetBusy(true);
                await _apiClient.UpdateStatusAsync(id, next, CancellationToken.None);
                await LoadEmployeesAsync();
                MessageBox.Show($"Đã cập nhật trạng thái: {next}.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowApiError(ex); }
            finally { SetBusy(false); }
        }

        private async Task ViewStatusHistoryAsync()
        {
            if (!int.TryParse(txtMaNhanVien.Text, out var id))
            {
                MessageBox.Show("Chọn nhân viên cần xem lịch sử.", "Thiếu nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var history = await _apiClient.GetStatusHistoryAsync(id, CancellationToken.None);
                var message = history.Count == 0
                    ? "Nhân viên chưa có lần thay đổi trạng thái nào."
                    : string.Join(Environment.NewLine, history.Select(item =>
                        $"{item.ThayDoiLuc:dd/MM/yyyy HH:mm:ss}: {item.TrangThaiCu} → {item.TrangThaiMoi}"));
                MessageBox.Show(message, "Lịch sử trạng thái", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private CreateNhanVienDto BuildCreateDto() => new()
        {
            HoTen = txtHoTen.Text.Trim(), SoDienThoai = EmptyToNull(txtSoDienThoai.Text),
            Email = EmptyToNull(txtEmail.Text), ChucVu = cboChucVu.Text.Trim()
        };

        private UpdateNhanVienDto BuildUpdateDto() => new()
        {
            HoTen = txtHoTen.Text.Trim(), SoDienThoai = EmptyToNull(txtSoDienThoai.Text),
            Email = EmptyToNull(txtEmail.Text), ChucVu = cboChucVu.Text.Trim()
        };

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text) || txtHoTen.Text.Trim().Length > 200 || string.IsNullOrWhiteSpace(cboChucVu.Text) || cboChucVu.Text.Trim().Length > 100)
            {
                MessageBox.Show("Họ tên (tối đa 200 ký tự) và chức vụ (tối đa 100 ký tự) là bắt buộc.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            var phone = txtSoDienThoai.Text.Trim();
            if (phone.Length > 0 && !Regex.IsMatch(phone, "^0[0-9]{9,10}$"))
            {
                MessageBox.Show("Số điện thoại phải bắt đầu bằng 0 và có 10 hoặc 11 chữ số.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            var email = txtEmail.Text.Trim();
            if (email.Length > 254 || (email.Length > 0 && !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$")))
            {
                MessageBox.Show("Email không hợp lệ hoặc vượt quá 254 ký tự.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void DgvNhanVien_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvNhanVien.Rows[e.RowIndex];
            txtMaNhanVien.Text = row.Cells["MaNV"].Value?.ToString();
            txtHoTen.Text = row.Cells["HoTen"].Value?.ToString();
            txtSoDienThoai.Text = row.Cells["SoDienThoai"].Value?.ToString();
            txtEmail.Text = row.Cells["Email"].Value?.ToString();
            cboChucVu.Text = row.Cells["ChucVu"].Value?.ToString();
            UpdateStatusButton();
        }

        private void UpdateStatusButton()
        {
            if (btnTrangThai == null) return;
            var status = dgvNhanVien?.CurrentRow?.Cells["TrangThai"].Value?.ToString();
            btnTrangThai.Text = status == "Hoạt động" ? "🔒 Ngừng hoạt động" : "♻ Khôi phục hoạt động";
        }

        private void SetBusy(bool busy)
        {
            btnThem.Enabled = btnSua.Enabled = btnTrangThai.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void ShowApiError(Exception ex) => MessageBox.Show(ex.Message, "Lỗi API nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private void ClearForm()
        {
            txtMaNhanVien.Clear(); txtHoTen.Clear(); txtSoDienThoai.Clear(); txtEmail.Clear();
            cboChucVu.Text = "Thủ thư"; dgvNhanVien.ClearSelection(); UpdateStatusButton(); txtHoTen.Focus();
        }
    }
}
