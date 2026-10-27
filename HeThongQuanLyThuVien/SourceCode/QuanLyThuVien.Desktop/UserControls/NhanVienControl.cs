using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class NhanVienControl : UserControl
    {
        private readonly NhanVienApiClient _apiClient = new();
        private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 350 };

        private TextBox txtMaNhanVien = null!, txtHoTen = null!, txtSoDienThoai = null!, txtEmail = null!, txtTimKiem = null!;
        private ComboBox cboChucVu = null!, cboLocChucVu = null!, cboLocTrangThai = null!;
        private DataGridView dgvNhanVien = null!;
        private AppButton btnThem = null!, btnSua = null!, btnTrangThai = null!;
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
            // ----- Biểu mẫu hồ sơ nhân viên -----
            txtMaNhanVien = Ui.ReadOnlyBox();
            txtHoTen = Ui.TextBox();
            cboChucVu = Ui.ComboBox(new object[] { "Thủ thư", "Nhân viên" }, style: ComboBoxStyle.DropDown);
            cboChucVu.Text = "Thủ thư";
            txtSoDienThoai = Ui.TextBox();
            txtEmail = Ui.TextBox();

            var fields = new FormGrid(4);
            fields.Add("Mã nhân viên", txtMaNhanVien);
            fields.Add("Họ và tên", txtHoTen, 2);
            fields.Add("Chức vụ", cboChucVu);
            fields.Add("Điện thoại", txtSoDienThoai, 2);
            fields.Add("Email", txtEmail, 2);

            var note = Ui.Hint("Tài khoản đăng nhập và phân quyền được quản lý riêng ở mục Tài khoản. Màn hình này chỉ quản lý hồ sơ nhân viên.");

            btnThem = Ui.Button("Thêm nhân viên", ButtonKind.Primary);
            btnSua = Ui.Button("Cập nhật hồ sơ");
            btnTrangThai = Ui.Button("Ngừng hoạt động", ButtonKind.Danger);
            var btnLichSu = Ui.Button("Lịch sử trạng thái");
            var btnLamMoi = Ui.Button("Làm mới");
            btnThem.Click += async (_, _) => await CreateEmployeeAsync();
            btnSua.Click += async (_, _) => await UpdateEmployeeAsync();
            btnTrangThai.Click += async (_, _) => await ToggleStatusAsync();
            btnLichSu.Click += async (_, _) => await ViewStatusHistoryAsync();
            btnLamMoi.Click += (_, _) => ClearForm();

            var formCard = new CardPanel("Hồ sơ nhân viên");
            formCard.AddRow(fields);
            formCard.AddRow(note);
            formCard.AddRow(Ui.ButtonBar(btnThem, btnSua, btnTrangThai, btnLichSu, btnLamMoi));

            // ----- Danh sách -----
            txtTimKiem = Ui.TextBox("Tên, điện thoại hoặc email", 280);
            txtTimKiem.TextChanged += (_, _) => ScheduleLoad();
            cboLocChucVu = Ui.ComboBox(new object[] { "Tất cả chức vụ" }, width: 190);
            cboLocTrangThai = Ui.ComboBox(new object[] { "Hoạt động", "Ngừng hoạt động", "Tất cả trạng thái" }, width: 190);
            cboLocChucVu.SelectedIndexChanged += FilterComboChanged;
            cboLocTrangThai.SelectedIndexChanged += (_, _) => ScheduleLoad();

            var toolbar = new ToolbarRow();
            toolbar.AddField("Tra cứu", txtTimKiem);
            toolbar.AddField("Chức vụ", cboLocChucVu);
            toolbar.AddField("Trạng thái", cboLocTrangThai);

            dgvNhanVien = Ui.Grid();
            dgvNhanVien.Columns.Add("MaNV", "Mã NV"); dgvNhanVien.Columns["MaNV"].FillWeight = 50;
            dgvNhanVien.Columns.Add("HoTen", "Họ và tên"); dgvNhanVien.Columns["HoTen"].FillWeight = 140;
            dgvNhanVien.Columns.Add("SoDienThoai", "Điện thoại"); dgvNhanVien.Columns["SoDienThoai"].FillWeight = 90;
            dgvNhanVien.Columns.Add("Email", "Email"); dgvNhanVien.Columns["Email"].FillWeight = 130;
            dgvNhanVien.Columns.Add("ChucVu", "Chức vụ"); dgvNhanVien.Columns["ChucVu"].FillWeight = 85;
            dgvNhanVien.Columns.Add("TrangThai", "Trạng thái"); dgvNhanVien.Columns["TrangThai"].FillWeight = 90;
            dgvNhanVien.CellClick += DgvNhanVien_CellClick;
            dgvNhanVien.SelectionChanged += (_, _) => UpdateStatusButton();

            var listCard = new CardPanel("Danh sách nhân viên", autoHeight: false);
            listCard.AddRow(toolbar);
            listCard.AddRow(dgvNhanVien, fill: true);

            Ui.SetupPage(this, Ui.ScrollStack(300, formCard, listCard));
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
                        $"{item.ThayDoiLuc:dd/MM/yyyy HH:mm:ss}: {item.TrangThaiCu} sang {item.TrangThaiMoi}"));
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
            var active = status == "Hoạt động";
            btnTrangThai.Text = active ? "Ngừng hoạt động" : "Khôi phục hoạt động";
            btnTrangThai.Kind = active ? ButtonKind.Danger : ButtonKind.Success;
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
