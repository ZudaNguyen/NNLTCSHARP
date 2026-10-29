using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtTaiKhoan = null!;
        private TextBox txtMatKhau = null!;
        private Button btnLogin = null!;

        public LoginForm()
        {
            InitializeComponent();
            SetupLibraryLoginUI();
        }

        private void SetupLibraryLoginUI()
        {
            // Tông màu giấy cổ và gỗ trầm
            Color paperBg = Color.FromArgb(247, 243, 233);
            Color woodDark = Color.FromArgb(44, 30, 22);
            Color burgundy = Color.FromArgb(122, 40, 34);
            Color goldAccent = Color.FromArgb(197, 155, 39);

            this.Text = "📖 Cổng Tri Thức - Đăng Nhập Hệ Thống";
            this.Size = new Size(440, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = paperBg;

            // Viền trang trí phía trên giống gáy sách
            Panel pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 8,
                BackColor = goldAccent
            };

            // Tiêu đề mang chất văn học
            Label lblTitle = new Label
            {
                Text = "📚 THƯ VIỆN TRI THỨC",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = woodDark,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            Label lblSubTitle = new Label
            {
                Text = "✒️ Sổ đăng ký phiên làm việc của Thủ thư & Quản trị",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(110, 85, 70),
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 35
            };

            // Nhãn & Ô nhập Tài khoản
            Label lblTaiKhoan = new Label
            {
                Text = "🔖 Mã định danh (Tài khoản):",
                Location = new Point(50, 115),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = woodDark,
                AutoSize = true
            };

            txtTaiKhoan = new TextBox
            {
                Location = new Point(50, 140),
                Width = 325,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            // Nhãn & Ô nhập Mật khẩu
            Label lblMatKhau = new Label
            {
                Text = "🗝️ Mật ngữ (Mật khẩu):",
                Location = new Point(50, 185),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = woodDark,
                AutoSize = true
            };

            txtMatKhau = new TextBox
            {
                Location = new Point(50, 210),
                Width = 325,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                MaxLength = 255
            };

            // Nút Đăng nhập màu đỏ đô bìa sách
            btnLogin = new Button
            {
                Text = "📖 Mở Sổ Nghiệp Vụ",
                Location = new Point(50, 265),
                Width = 325,
                Height = 42,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = burgundy,
                ForeColor = Color.FromArgb(255, 248, 231),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderColor = goldAccent;
            btnLogin.FlatAppearance.BorderSize = 1;
            btnLogin.Click += BtnLogin_Click;

            this.AcceptButton = btnLogin;

            this.Controls.Add(btnLogin);
            this.Controls.Add(txtMatKhau);
            this.Controls.Add(lblMatKhau);
            this.Controls.Add(txtTaiKhoan);
            this.Controls.Add(lblTaiKhoan);
            this.Controls.Add(lblSubTitle);
            this.Controls.Add(lblTitle);
            this.Controls.Add(pnlTopBar);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string taiKhoan = txtTaiKhoan.Text.Trim();
            string matKhau = txtMatKhau.Text; // Không Trim mật khẩu: khoảng trắng đầu/cuối là ký tự hợp lệ

            if (string.IsNullOrEmpty(taiKhoan) || string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Xin vui lòng điền đầy đủ Mã định danh và Mật ngữ!",
                                "Nhắc nhở từ Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus();
                return;
            }

            // TODO(API): thay bằng gọi API đăng nhập; vai trò lấy từ claim của JWT.
            // Hiện chỉ là bản giả lập để dựng giao diện: mọi tài khoản đều vào được.
            string roleGiaLap = taiKhoan.Equals("admin", StringComparison.OrdinalIgnoreCase) ? Models.Roles.Admin : Models.Roles.ThuThu;

            MainForm mainForm = new MainForm(roleGiaLap, taiKhoan);
            mainForm.FormClosed += (s, args) => this.Close();
            mainForm.Show();

            this.Hide();
        }
    }
}