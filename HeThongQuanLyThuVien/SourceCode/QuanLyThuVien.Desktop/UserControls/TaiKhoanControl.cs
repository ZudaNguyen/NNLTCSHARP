using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class TaiKhoanControl : UserControl
    {
        private readonly AccountApiClient _api = new();
        private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 300 };
        private TextBox txtId = null!, txtUserName = null!, txtDisplayName = null!, txtPassword = null!, txtSearch = null!;
        private NumericUpDown numEmployeeId = null!, numReaderId = null!;
        private ComboBox cboRole = null!, cboRoleFilter = null!, cboStatusFilter = null!;
        private DataGridView grid = null!;
        private AppButton btnCreate = null!, btnUpdate = null!, btnStatus = null!, btnReset = null!;

        public TaiKhoanControl()
        {
            BuildUi();
            Load += async (_, _) => await LoadAccountsAsync();
            _filterTimer.Tick += async (_, _) => { _filterTimer.Stop(); await LoadAccountsAsync(); };
        }

        private void BuildUi()
        {
            // ----- Biểu mẫu tài khoản -----
            txtId = Ui.ReadOnlyBox();
            txtUserName = Ui.TextBox();
            txtDisplayName = Ui.TextBox();
            cboRole = Ui.ComboBox(new object[] { AccountRoles.Admin, AccountRoles.Librarian, AccountRoles.Reader }, 1);
            numEmployeeId = Ui.Number(0, int.MaxValue, 0);
            numReaderId = Ui.Number(0, int.MaxValue, 0);
            txtPassword = Ui.TextBox();
            txtPassword.UseSystemPasswordChar = true;

            var fields = new FormGrid(4);
            fields.Add("Mã tài khoản", txtId);
            fields.Add("Tên đăng nhập", txtUserName);
            fields.Add("Tên hiển thị", txtDisplayName, 2);
            fields.Add("Vai trò", cboRole);
            fields.Add("Mã nhân viên", numEmployeeId);
            fields.Add("Mã độc giả", numReaderId);
            fields.Add("Mật khẩu mới", txtPassword);

            var note = Ui.Hint("Thủ thư và độc giả cần liên kết với hồ sơ đang hoạt động. Mật khẩu mới tối thiểu 12 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.");

            btnCreate = Ui.Button("Tạo tài khoản", ButtonKind.Primary);
            btnUpdate = Ui.Button("Cập nhật vai trò");
            btnStatus = Ui.Button("Khóa tài khoản", ButtonKind.Danger);
            btnReset = Ui.Button("Đặt lại mật khẩu");
            var clear = Ui.Button("Làm mới");
            btnCreate.Click += async (_, _) => await CreateAsync();
            btnUpdate.Click += async (_, _) => await UpdateAsync();
            btnStatus.Click += async (_, _) => await ToggleStatusAsync();
            btnReset.Click += async (_, _) => await ResetPasswordAsync();
            clear.Click += (_, _) => ClearForm();

            var formCard = new CardPanel("Tài khoản & phân quyền");
            formCard.AddRow(fields);
            formCard.AddRow(note);
            formCard.AddRow(Ui.ButtonBar(btnCreate, btnUpdate, btnStatus, btnReset, clear));

            // ----- Danh sách -----
            txtSearch = Ui.TextBox("Tên đăng nhập hoặc tên hiển thị", 280);
            txtSearch.TextChanged += (_, _) => ScheduleLoad();
            cboRoleFilter = CreateFilter("Tất cả vai trò", new object[] { AccountRoles.Admin, AccountRoles.Librarian, AccountRoles.Reader });
            cboStatusFilter = CreateFilter("Tất cả trạng thái", new object[] { "Hoạt động", "Bị khóa" });
            cboRoleFilter.SelectedIndexChanged += (_, _) => ScheduleLoad();
            cboStatusFilter.SelectedIndexChanged += (_, _) => ScheduleLoad();

            var toolbar = new ToolbarRow();
            toolbar.AddField("Tra cứu", txtSearch);
            toolbar.AddField("Vai trò", cboRoleFilter);
            toolbar.AddField("Trạng thái", cboStatusFilter);

            grid = Ui.Grid();
            grid.Columns.Add("Id", "Mã TK");
            grid.Columns.Add("Username", "Tên đăng nhập");
            grid.Columns.Add("Display", "Tên hiển thị");
            grid.Columns.Add("Role", "Vai trò");
            grid.Columns.Add("Link", "Mã hồ sơ");
            grid.Columns.Add("Status", "Trạng thái");
            grid.CellClick += (_, e) => { if (e.RowIndex >= 0) LoadSelectedRow(grid.Rows[e.RowIndex]); };
            grid.SelectionChanged += (_, _) => UpdateStatusButton();

            var listCard = new CardPanel("Danh sách tài khoản", autoHeight: false);
            listCard.AddRow(toolbar);
            listCard.AddRow(grid, fill: true);

            Ui.SetupPage(this, Ui.ScrollStack(300, formCard, listCard));
        }

        private static ComboBox CreateFilter(string all, object[] items)
        {
            var combo = Ui.ComboBox(new object[] { all }, width: 190);
            combo.Items.AddRange(items);
            return combo;
        }

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

        private void UpdateStatusButton()
        {
            if (btnStatus == null) return;
            var active = grid?.CurrentRow?.Cells["Status"].Value?.ToString() == "Hoạt động";
            btnStatus.Text = active ? "Khóa tài khoản" : "Mở tài khoản";
            btnStatus.Kind = active ? ButtonKind.Danger : ButtonKind.Success;
        }

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
