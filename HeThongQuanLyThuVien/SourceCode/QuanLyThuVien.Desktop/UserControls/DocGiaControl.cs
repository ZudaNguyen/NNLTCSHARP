using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class DocGiaControl : UserControl
    {
        private readonly DocGiaApiClient _apiClient = new();
        private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 300 };
        private CancellationTokenSource? _loadCancellation;

        // Controls bám sát bảng DocGia
        private TextBox txtMaDocGia = null!;
        private TextBox txtHoTen = null!;
        private DateTimePicker dtpNgaySinh = null!;
        private ComboBox cboGioiTinh = null!;
        private TextBox txtSoDienThoai = null!;
        private TextBox txtEmail = null!;
        private TextBox txtDiaChi = null!;
        private DateTimePicker dtpNgayDangKy = null!;
        private ComboBox cboTrangThai = null!;

        // Tìm kiếm & Bảng
        private TextBox txtTimKiem = null!;
        private ComboBox cboLocTrangThai = null!;
        private DataGridView dgvDocGia = null!;

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
            // ----- Biểu mẫu thông tin độc giả -----
            txtMaDocGia = Ui.ReadOnlyBox();
            txtHoTen = Ui.TextBox();
            cboTrangThai = Ui.ComboBox(new object[] { "Hoạt động", "Khóa thẻ" });
            dtpNgaySinh = Ui.DatePicker(new DateTime(2005, 1, 1));
            cboGioiTinh = Ui.ComboBox(new object[] { "Nam", "Nữ", "Khác" });
            txtSoDienThoai = Ui.TextBox();
            dtpNgayDangKy = Ui.DatePicker(DateTime.Now);
            dtpNgayDangKy.Enabled = false;
            txtEmail = Ui.TextBox();
            txtDiaChi = Ui.TextBox();

            var fields = new FormGrid(4);
            fields.Add("Mã thẻ", txtMaDocGia);
            fields.Add("Họ và tên", txtHoTen, 2);
            fields.Add("Trạng thái", cboTrangThai);
            fields.Add("Ngày sinh", dtpNgaySinh);
            fields.Add("Giới tính", cboGioiTinh);
            fields.Add("Điện thoại", txtSoDienThoai);
            fields.Add("Ngày cấp thẻ", dtpNgayDangKy);
            fields.Add("Email", txtEmail, 2);
            fields.Add("Địa chỉ", txtDiaChi, 2);

            var btnThem = Ui.Button("Cấp thẻ mới", ButtonKind.Primary);
            var btnSua = Ui.Button("Cập nhật hồ sơ");
            var btnKhoaMo = Ui.Button("Khóa / Mở thẻ", ButtonKind.Danger);
            var btnLamMoi = Ui.Button("Làm mới");
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnKhoaMo.Click += BtnKhoaMo_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            var formCard = new CardPanel("Thông tin độc giả");
            formCard.AddRow(fields);
            formCard.AddRow(Ui.ButtonBar(btnThem, btnSua, btnKhoaMo, btnLamMoi));

            // ----- Danh sách -----
            txtTimKiem = Ui.TextBox("Tên, điện thoại hoặc email", 300);
            txtTimKiem.TextChanged += (s, e) => ScheduleFilter();
            cboLocTrangThai = Ui.ComboBox(new object[] { "Tất cả", "Hoạt động", "Khóa thẻ" }, width: 170);
            cboLocTrangThai.SelectedIndexChanged += (s, e) => ScheduleFilter();

            var toolbar = new ToolbarRow();
            toolbar.AddField("Tìm độc giả", txtTimKiem);
            toolbar.AddField("Trạng thái thẻ", cboLocTrangThai);

            dgvDocGia = Ui.Grid();
            dgvDocGia.Columns.Add("MaDocGia", "Mã thẻ");
            dgvDocGia.Columns["MaDocGia"].FillWeight = 50;
            dgvDocGia.Columns.Add("HoTen", "Họ và tên");
            dgvDocGia.Columns["HoTen"].FillWeight = 130;
            dgvDocGia.Columns.Add("NgaySinh", "Ngày sinh");
            dgvDocGia.Columns["NgaySinh"].FillWeight = 75;
            dgvDocGia.Columns.Add("GioiTinh", "Giới tính");
            dgvDocGia.Columns["GioiTinh"].FillWeight = 55;
            dgvDocGia.Columns.Add("SoDienThoai", "Điện thoại");
            dgvDocGia.Columns["SoDienThoai"].FillWeight = 85;
            dgvDocGia.Columns.Add("Email", "Email");
            dgvDocGia.Columns["Email"].FillWeight = 130;
            dgvDocGia.Columns.Add("DiaChi", "Địa chỉ");
            dgvDocGia.Columns["DiaChi"].FillWeight = 140;
            dgvDocGia.Columns.Add("NgayDangKy", "Ngày cấp thẻ");
            dgvDocGia.Columns["NgayDangKy"].FillWeight = 75;
            dgvDocGia.Columns.Add("TrangThai", "Trạng thái");
            dgvDocGia.Columns["TrangThai"].FillWeight = 75;
            dgvDocGia.CellClick += DgvDocGia_CellClick;

            var listCard = new CardPanel("Danh sách độc giả", autoHeight: false);
            listCard.AddRow(toolbar);
            listCard.AddRow(dgvDocGia, fill: true);

            Ui.SetupPage(this, Ui.ScrollStack(300, formCard, listCard));
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

        private void DgvDocGia_CellClick(object? sender, DataGridViewCellEventArgs e)
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
