using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthApiClient _authApi = new();
        private TextBox txtTaiKhoan = null!;
        private TextBox txtMatKhau = null!;
        private AppButton btnLogin = null!;
        private CheckBox chkShowPassword = null!;
        private Label lblError = null!;
        private bool _returningToLogin;

        public LoginForm()
        {
            InitializeComponent();
            SetupLoginUI();
        }

        private void SetupLoginUI()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "Đăng nhập - Hệ thống quản lý thư viện";
            ClientSize = new Size(780, 460);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = UiTheme.Surface;
            Font = UiTheme.Body;

            // ----- Cột trái: thương hiệu -----
            var brand = new TableLayoutPanel
            {
                Dock = DockStyle.Left, Width = 300, ColumnCount = 1, RowCount = 5, BackColor = UiTheme.Sidebar,
                Padding = new Padding(36, 0, 28, 28)
            };
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            brand.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            brand.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            brand.Controls.Add(new Label
            {
                Text = "THƯ VIỆN", AutoSize = true, Font = UiTheme.Font(18, FontStyle.Bold),
                ForeColor = Color.White, Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Margin = new Padding(0, 0, 0, 6)
            }, 0, 1);
            brand.Controls.Add(new Label
            {
                Text = "Hệ thống quản lý thư viện", AutoSize = true, Font = UiTheme.Font(10),
                ForeColor = UiTheme.SidebarText, Anchor = AnchorStyles.Left | AnchorStyles.Top
            }, 0, 2);
            brand.Controls.Add(new Label
            {
                Text = "Quản lý sách, độc giả, mượn trả và báo cáo trong một ứng dụng.", AutoSize = true,
                MaximumSize = new Size(230, 0), Font = UiTheme.Font(9), ForeColor = UiTheme.SidebarCaption,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Margin = new Padding(0, 0, 0, 24)
            }, 0, 4);

            // ----- Cột phải: biểu mẫu đăng nhập -----
            txtTaiKhoan = Ui.TextBox();
            txtTaiKhoan.MaxLength = 64;
            txtTaiKhoan.TextChanged += (_, _) => HideError();
            txtMatKhau = Ui.TextBox();
            txtMatKhau.MaxLength = 128;
            txtMatKhau.UseSystemPasswordChar = true;
            txtMatKhau.TextChanged += (_, _) => HideError();
            chkShowPassword = new CheckBox
            {
                Text = "Hiện mật khẩu", AutoSize = true, Font = UiTheme.Font(9.5f), ForeColor = UiTheme.TextSecondary,
                Margin = new Padding(0, 0, 0, 10), Cursor = Cursors.Hand
            };
            chkShowPassword.CheckedChanged += (_, _) => txtMatKhau.UseSystemPasswordChar = !chkShowPassword.Checked;
            lblError = new Label
            {
                AutoSize = true, MaximumSize = new Size(320, 0), Font = UiTheme.Font(9.5f), ForeColor = UiTheme.Danger,
                Margin = new Padding(0, 0, 0, 10), Visible = false
            };
            btnLogin = Ui.Button("Đăng nhập", ButtonKind.Primary);
            btnLogin.AutoSize = false;
            btnLogin.Height = 42;
            btnLogin.Margin = new Padding(0, 4, 0, 0);
            btnLogin.Click += async (_, _) => await LoginAsync();
            AcceptButton = btnLogin;

            var heading = new Label { Text = "Đăng nhập", AutoSize = true, Font = UiTheme.Font(18, FontStyle.Bold), ForeColor = UiTheme.Text, Margin = new Padding(0, 0, 0, 4) };
            var intro = new Label { Text = "Dùng tài khoản do thư viện cấp để tiếp tục.", AutoSize = true, Font = UiTheme.Font(10), ForeColor = UiTheme.TextMuted, Margin = new Padding(0, 0, 0, 22) };

            var form = new FormGrid(1);
            form.AddFull(heading);
            form.AddFull(intro);
            form.Add("Tên đăng nhập", txtTaiKhoan);
            form.Add("Mật khẩu", txtMatKhau);
            form.AddFull(chkShowPassword);
            form.AddFull(lblError);
            form.AddFull(btnLogin);
            btnLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var right = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(52, 44, 40, 24) };
            right.Controls.Add(form);

            Controls.Add(right);
            Controls.Add(brand);
            Ui.ApplyDpi(this);
            Shown += (_, _) => txtTaiKhoan.Focus();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void HideError()
        {
            if (lblError.Visible) lblError.Visible = false;
        }

        private async Task LoginAsync()
        {
            var userName = txtTaiKhoan.Text.Trim();
            var password = txtMatKhau.Text;
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                ShowError("Nhập tên đăng nhập và mật khẩu.");
                (string.IsNullOrWhiteSpace(userName) ? txtTaiKhoan : txtMatKhau).Focus();
                return;
            }

            btnLogin.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var session = await _authApi.LoginAsync(new LoginRequestDto { TenDangNhap = userName, MatKhau = password }, CancellationToken.None);
                txtMatKhau.Clear();
                HideError();
                _returningToLogin = false;
                var mainForm = new MainForm(session.User, ReturnToLogin);
                mainForm.FormClosed += (_, _) => { if (!_returningToLogin) Close(); };
                mainForm.Show();
                Hide();
            }
            catch (Exception ex)
            {
                txtMatKhau.Clear();
                ShowError(ex.Message);
                txtMatKhau.Focus();
            }
            finally { btnLogin.Enabled = true; Cursor = Cursors.Default; }
        }

        private void ReturnToLogin()
        {
            _returningToLogin = true;
            Show();
            Activate();
            txtMatKhau.Clear();
            HideError();
            txtTaiKhoan.Focus();
        }
    }
}
