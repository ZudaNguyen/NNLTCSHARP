using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthApiClient _authApi = new();
        private TextBox txtTaiKhoan = null!;
        private TextBox txtMatKhau = null!;
        private Button btnLogin = null!;
        private CheckBox chkShowPassword = null!;
        private bool _returningToLogin;

        public LoginForm()
        {
            InitializeComponent();
            SetupLibraryLoginUI();
        }

        private void SetupLibraryLoginUI()
        {
            Color paperBg = Color.FromArgb(247, 243, 233);
            Color woodDark = Color.FromArgb(44, 30, 22);
            Color burgundy = Color.FromArgb(122, 40, 34);
            Color goldAccent = Color.FromArgb(197, 155, 39);

            Text = "📖 Cổng Tri Thức - Đăng Nhập Hệ Thống";
            Size = new Size(440, 410);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = paperBg;

            var pnlTopBar = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = goldAccent };
            var lblTitle = new Label { Text = "📚 THƯ VIỆN TRI THỨC", Font = new Font("Georgia", 16, FontStyle.Bold), ForeColor = woodDark, AutoSize = false, TextAlign = ContentAlignment.BottomCenter, Dock = DockStyle.Top, Height = 60 };
            var lblSubTitle = new Label { Text = "✒️ Đăng nhập bằng tài khoản thư viện", Font = new Font("Georgia", 9.5f, FontStyle.Italic), ForeColor = Color.FromArgb(110, 85, 70), AutoSize = false, TextAlign = ContentAlignment.TopCenter, Dock = DockStyle.Top, Height = 35 };
            var lblTaiKhoan = new Label { Text = "🔖 Tên đăng nhập:", Location = new Point(50, 115), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = woodDark, AutoSize = true };
            txtTaiKhoan = new TextBox { Location = new Point(50, 140), Width = 325, Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle, MaxLength = 64 };
            var lblMatKhau = new Label { Text = "🗝️ Mật khẩu:", Location = new Point(50, 185), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = woodDark, AutoSize = true };
            txtMatKhau = new TextBox { Location = new Point(50, 210), Width = 325, Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle, PasswordChar = '●', MaxLength = 128 };
            chkShowPassword = new CheckBox { Text = "Hiện mật khẩu", Location = new Point(50, 240), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = woodDark };
            chkShowPassword.CheckedChanged += (_, _) => txtMatKhau.PasswordChar = chkShowPassword.Checked ? '\0' : '●';
            btnLogin = new Button { Text = "📖 Đăng nhập", Location = new Point(50, 275), Width = 325, Height = 42, Font = new Font("Georgia", 11, FontStyle.Bold), BackColor = burgundy, ForeColor = Color.FromArgb(255, 248, 231), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderColor = goldAccent; btnLogin.FlatAppearance.BorderSize = 1;
            btnLogin.Click += async (_, _) => await LoginAsync();
            AcceptButton = btnLogin;

            Controls.Add(btnLogin); Controls.Add(chkShowPassword); Controls.Add(txtMatKhau); Controls.Add(lblMatKhau); Controls.Add(txtTaiKhoan); Controls.Add(lblTaiKhoan); Controls.Add(lblSubTitle); Controls.Add(lblTitle); Controls.Add(pnlTopBar);
        }

        private async Task LoginAsync()
        {
            var userName = txtTaiKhoan.Text.Trim();
            var password = txtMatKhau.Text;
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Nhập tên đăng nhập và mật khẩu.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus();
                return;
            }

            btnLogin.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var session = await _authApi.LoginAsync(new LoginRequestDto { TenDangNhap = userName, MatKhau = password }, CancellationToken.None);
                txtMatKhau.Clear();
                _returningToLogin = false;
                var mainForm = new MainForm(session.User, ReturnToLogin);
                mainForm.FormClosed += (_, _) => { if (!_returningToLogin) Close(); };
                mainForm.Show();
                Hide();
            }
            catch (Exception ex)
            {
                txtMatKhau.Clear();
                MessageBox.Show(ex.Message, "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            txtTaiKhoan.Focus();
        }
    }
}
