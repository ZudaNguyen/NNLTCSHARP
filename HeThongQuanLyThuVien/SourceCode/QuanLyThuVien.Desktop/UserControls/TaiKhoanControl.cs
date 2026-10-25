using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class TaiKhoanControl : UserControl
    {
        private readonly AccountApiClient _api = new();
        private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 300 };
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);
        private TextBox txtId = null!, txtUserName = null!, txtDisplayName = null!, txtPassword = null!, txtSearch = null!;
        private NumericUpDown numEmployeeId = null!, numReaderId = null!;
        private ComboBox cboRole = null!, cboRoleFilter = null!, cboStatusFilter = null!;
        private DataGridView grid = null!;
        private Button btnCreate = null!, btnUpdate = null!, btnStatus = null!, btnReset = null!;

        public TaiKhoanControl()
        {
            BuildUi();
            Load += async (_, _) => await LoadAccountsAsync();
            _filterTimer.Tick += async (_, _) => { _filterTimer.Stop(); await LoadAccountsAsync(); };
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill; BackColor = PaperCream; Padding = new Padding(20);
            var form = new Panel { Dock = DockStyle.Top, Height = 215, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle };
            form.Controls.Add(new Label { Text = "Quản lý Tài khoản & Phân quyền", Location = new Point(15, 10), AutoSize = true, Font = new Font("Georgia", 11.5f, FontStyle.Bold), ForeColor = Burgundy });
            AddLabel(form, "Mã TK:", 15, 45); txtId = AddTextBox(form, 105, 42, 85); txtId.ReadOnly = true;
            AddLabel(form, "Tên đăng nhập:", 15, 82); txtUserName = AddTextBox(form, 130, 79, 190);
            AddLabel(form, "Tên hiển thị:", 335, 82); txtDisplayName = AddTextBox(form, 435, 79, 230);
            AddLabel(form, "Mật khẩu mới:", 15, 120); txtPassword = AddTextBox(form, 130, 117, 190); txtPassword.PasswordChar = '●';
            AddLabel(form, "Vai trò:", 335, 120);
            cboRole = new ComboBox { Location = new Point(435, 117), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboRole.Items.AddRange(new object[] { AccountRoles.Admin, AccountRoles.Librarian, AccountRoles.Reader }); cboRole.SelectedItem = AccountRoles.Librarian; form.Controls.Add(cboRole);
            AddLabel(form, "Mã nhân viên:", 605, 82); numEmployeeId = AddNumber(form, 710, 79);
            AddLabel(form, "Mã độc giả:", 605, 120); numReaderId = AddNumber(form, 710, 117);
            form.Controls.Add(new Label { Text = "Tài khoản THU_THU/DOC_GIA phải liên kết đúng hồ sơ đang hoạt động. Tạo/đặt lại mật khẩu: tối thiểu 12 ký tự, chữ hoa, chữ thường, số và ký tự đặc biệt.", Location = new Point(15, 153), Size = new Size(870, 30), Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = Burgundy });

            var buttons = new FlowLayoutPanel { Location = new Point(15, 183), Size = new Size(900, 32), WrapContents = false };
            btnCreate = CreateButton("➕ Tạo tài khoản", ForestGreen);
            btnUpdate = CreateButton("✏️ Cập nhật vai trò", Color.FromArgb(180, 120, 20));
            btnStatus = CreateButton("🔒 Khóa / Mở", Burgundy);
            btnReset = CreateButton("🔑 Đặt lại mật khẩu", Color.FromArgb(74, 53, 37));
            var clear = CreateButton("Làm mới", Color.FromArgb(90, 75, 65));
            btnCreate.Click += async (_, _) => await CreateAsync(); btnUpdate.Click += async (_, _) => await UpdateAsync();
            btnStatus.Click += async (_, _) => await ToggleStatusAsync(); btnReset.Click += async (_, _) => await ResetPasswordAsync();
            clear.Click += (_, _) => ClearForm();
            buttons.Controls.AddRange(new Control[] { btnCreate, btnUpdate, btnStatus, btnReset, clear }); form.Controls.Add(buttons);

            var filter = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = PaperCream };
            AddLabel(filter, "Tra cứu:", 5, 16); txtSearch = new TextBox { Location = new Point(85, 12), Width = 230, Font = new Font("Segoe UI", 10) }; txtSearch.TextChanged += (_, _) => ScheduleLoad();
            cboRoleFilter = CreateFilter(325, 11, 150, "Tất cả vai trò", new object[] { AccountRoles.Admin, AccountRoles.Librarian, AccountRoles.Reader });
            cboStatusFilter = CreateFilter(485, 11, 150, "Tất cả trạng thái", new object[] { "Hoạt động", "Bị khóa" });
            cboRoleFilter.SelectedIndexChanged += (_, _) => ScheduleLoad(); cboStatusFilter.SelectedIndexChanged += (_, _) => ScheduleLoad();
            filter.Controls.AddRange(new Control[] { txtSearch, cboRoleFilter, cboStatusFilter });

            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = CardBg, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 38, RowTemplate = { Height = 32 }, GridColor = Color.FromArgb(225, 215, 195) };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37); grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215); grid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
            grid.DefaultCellStyle.BackColor = CardBg; grid.DefaultCellStyle.ForeColor = WoodDark; grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160); grid.DefaultCellStyle.SelectionForeColor = WoodDark;
            grid.Columns.Add("Id", "Mã TK"); grid.Columns.Add("Username", "Tên đăng nhập"); grid.Columns.Add("Display", "Tên hiển thị"); grid.Columns.Add("Role", "Vai trò"); grid.Columns.Add("Link", "Mã hồ sơ"); grid.Columns.Add("Status", "Trạng thái");
            grid.CellClick += (_, e) => { if (e.RowIndex >= 0) LoadSelectedRow(grid.Rows[e.RowIndex]); };
            grid.SelectionChanged += (_, _) => UpdateStatusButton(); panel.Controls.Add(grid);
            Controls.Add(panel); Controls.Add(filter); Controls.Add(form);
        }

        private NumericUpDown AddNumber(Control parent, int x, int y)
        {
            var number = new NumericUpDown { Location = new Point(x, y), Width = 95, Minimum = 0, Maximum = int.MaxValue, Font = new Font("Segoe UI", 10) };
            parent.Controls.Add(number); return number;
        }
        private ComboBox CreateFilter(int x, int y, int width, string all, object[] items)
        {
            var combo = new ComboBox { Location = new Point(x, y), Width = width, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
            combo.Items.Add(all); combo.Items.AddRange(items); combo.SelectedIndex = 0; return combo;
        }
        private void AddLabel(Control parent, string text, int x, int y) => parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark });
        private TextBox AddTextBox(Control parent, int x, int y, int width) { var t = new TextBox { Location = new Point(x, y), Width = width, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle }; parent.Controls.Add(t); return t; }
        private Button CreateButton(string text, Color color) { var b = new Button { Text = text, Size = new Size(165, 30), Margin = new Padding(0, 0, 8, 0), Font = new Font("Georgia", 8.5f, FontStyle.Bold), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
        private void ScheduleLoad() { _filterTimer.Stop(); _filterTimer.Start(); }

        private async Task LoadAccountsAsync()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                var role = cboRoleFilter.SelectedIndex <= 0 ? null : cboRoleFilter.SelectedItem?.ToString();
                var status = cboStatusFilter.SelectedIndex <= 0 ? null : cboStatusFilter.SelectedItem?.ToString();
                var rows = await _api.SearchAsync(txtSearch.Text, role, status, CancellationToken.None);
                grid.Rows.Clear();
                foreach (var account in rows)
                {
                    var link = account.MaNhanVien.HasValue ? $"NV #{account.MaNhanVien}" : account.MaDocGia.HasValue ? $"ĐG #{account.MaDocGia}" : "—";
                    grid.Rows.Add(account.MaTaiKhoan, account.TenDangNhap, account.TenHienThi, account.VaiTro, link, account.TrangThai);
                }
                UpdateStatusButton();
            }
            catch (Exception ex) { if (!IsDisposed) MessageBox.Show(ex.Message, "Lỗi API tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task CreateAsync()
        {
            if (!ValidateForm(true)) return;
            try
            {
                SetBusy(true);
                var result = await _api.CreateAsync(new CreateAccountRequestDto { TenDangNhap = txtUserName.Text.Trim(), TenHienThi = txtDisplayName.Text.Trim(), MatKhau = txtPassword.Text, VaiTro = cboRole.Text, MaNhanVien = NumberOrNull(numEmployeeId), MaDocGia = NumberOrNull(numReaderId) }, CancellationToken.None);
                await LoadAccountsAsync(); ClearForm(); MessageBox.Show($"Đã tạo tài khoản #{result.MaTaiKhoan}.", "Quản lý tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetBusy(false); }
        }

        private async Task UpdateAsync()
        {
            if (!int.TryParse(txtId.Text, out var id)) { MessageBox.Show("Chọn tài khoản cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!ValidateForm(false)) return;
            try
            {
                SetBusy(true); await _api.UpdateAsync(id, new UpdateAccountRequestDto { TenHienThi = txtDisplayName.Text.Trim(), VaiTro = cboRole.Text, MaNhanVien = NumberOrNull(numEmployeeId), MaDocGia = NumberOrNull(numReaderId) }, CancellationToken.None);
                await LoadAccountsAsync(); MessageBox.Show("Đã cập nhật hồ sơ và vai trò.", "Quản lý tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetBusy(false); }
        }

        private async Task ToggleStatusAsync()
        {
            if (!int.TryParse(txtId.Text, out var id) || grid.CurrentRow == null) return;
            var current = grid.CurrentRow.Cells["Status"].Value?.ToString(); var next = current == "Hoạt động" ? "Bị khóa" : "Hoạt động";
            if (MessageBox.Show($"Chuyển tài khoản sang trạng thái {next}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { SetBusy(true); await _api.UpdateStatusAsync(id, next, CancellationToken.None); await LoadAccountsAsync(); MessageBox.Show("Đã cập nhật trạng thái.", "Quản lý tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetBusy(false); }
        }

        private async Task ResetPasswordAsync()
        {
            if (!int.TryParse(txtId.Text, out var id)) { MessageBox.Show("Chọn tài khoản cần đặt lại mật khẩu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtPassword.Text)) { MessageBox.Show("Nhập mật khẩu mới vào ô Mật khẩu mới.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try { SetBusy(true); await _api.ResetPasswordAsync(id, txtPassword.Text, CancellationToken.None); txtPassword.Clear(); MessageBox.Show("Đã đặt lại mật khẩu và thu hồi mọi phiên hiện tại của tài khoản.", "Quản lý tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetBusy(false); }
        }

        private void LoadSelectedRow(DataGridViewRow row)
        {
            txtId.Text = row.Cells["Id"].Value?.ToString(); txtUserName.Text = row.Cells["Username"].Value?.ToString(); txtDisplayName.Text = row.Cells["Display"].Value?.ToString();
            cboRole.SelectedItem = row.Cells["Role"].Value?.ToString(); numEmployeeId.Value = 0; numReaderId.Value = 0;
            var link = row.Cells["Link"].Value?.ToString() ?? "";
            if (link.StartsWith("NV #") && int.TryParse(link[4..], out var employeeId)) numEmployeeId.Value = employeeId;
            if (link.StartsWith("ĐG #") && int.TryParse(link[4..], out var readerId)) numReaderId.Value = readerId;
            txtPassword.Clear(); UpdateStatusButton();
        }

        private void UpdateStatusButton() => btnStatus.Text = grid?.CurrentRow?.Cells["Status"].Value?.ToString() == "Hoạt động" ? "🔒 Khóa tài khoản" : "♻ Mở tài khoản";
        private bool ValidateForm(bool creating)
        {
            if (string.IsNullOrWhiteSpace(txtDisplayName.Text) || cboRole.SelectedItem == null) { MessageBox.Show("Tên hiển thị và vai trò là bắt buộc.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (creating && string.IsNullOrWhiteSpace(txtUserName.Text)) { MessageBox.Show("Tên đăng nhập là bắt buộc.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (creating && string.IsNullOrWhiteSpace(txtPassword.Text)) { MessageBox.Show("Mật khẩu là bắt buộc khi tạo tài khoản.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }
        private static int? NumberOrNull(NumericUpDown n) => n.Value == 0 ? null : (int)n.Value;
        private void SetBusy(bool busy) { btnCreate.Enabled = btnUpdate.Enabled = btnStatus.Enabled = btnReset.Enabled = !busy; Cursor = busy ? Cursors.WaitCursor : Cursors.Default; }
        private void ClearForm() { txtId.Clear(); txtUserName.Clear(); txtDisplayName.Clear(); txtPassword.Clear(); cboRole.SelectedItem = AccountRoles.Librarian; numEmployeeId.Value = 0; numReaderId.Value = 0; grid.ClearSelection(); UpdateStatusButton(); }
    }
}
