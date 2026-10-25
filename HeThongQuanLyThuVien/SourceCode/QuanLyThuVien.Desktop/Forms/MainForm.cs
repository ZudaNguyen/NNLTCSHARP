using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.UserControls;

namespace QuanLyThuVien.Desktop.Forms
{
    public partial class MainForm : Form
    {
        private string _role;
        private string _username;
        private readonly Action _returnToLogin;
        private readonly AuthApiClient _authApi = new();
        private bool _handlingSessionExpiry;

        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Label lblHeaderTitle;
        private Button currentActiveButton;

        // Bảng màu Thư viện cổ điển
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);       // Nâu gỗ đậm
        private readonly Color WoodDarker = Color.FromArgb(30, 20, 14);     // Nâu đen
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);      // Đỏ đô bìa sách
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);  // Màu giấy cũ
        private readonly Color PaperHeader = Color.FromArgb(238, 230, 212); // Màu giấy đậm hơn cho Header
        private readonly Color GoldText = Color.FromArgb(218, 176, 63);     // Vàng đồng

        public MainForm(AuthUserDto user, Action returnToLogin)
        {
            InitializeComponent();
            _role = user.VaiTro;
            _username = user.TenHienThi;
            _returnToLogin = returnToLogin;

            SetupMainLayout();
            ApiSession.SessionExpired += HandleSessionExpired;
            FormClosed += (_, _) => ApiSession.SessionExpired -= HandleSessionExpired;
            if (_role == AccountRoles.Reader)
                LoadUserControl(new ReaderCatalogControl(), "📚 Tra cứu Sách");
            else
                LoadUserControl(new DashboardControl(), "📜 Tổng quan Sổ sách Thư viện");
        }

        private void SetupMainLayout()
        {
            this.Text = $"📖 Hệ thống Quản lý Thư viện - Thủ thư trực: {_username} ({_role})";
            this.Size = new Size(1280, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1024, 620);

            // 1. SIDEBAR (Tủ gỗ danh mục bên trái)
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = WoodDark
            };

            Label lblBrand = new Label
            {
                Text = "🏛️ THƯ VIỆN CỔ",
                Font = new Font("Georgia", 14, FontStyle.Bold),
                ForeColor = GoldText,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = WoodDarker
            };

            // Các nút menu mang biểu tượng sách, bút lông, thẻ đọc
            Button btnLogout = CreateMenuButton("🚪 Đăng xuất");
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.BackColor = Color.FromArgb(90, 25, 20);
            btnLogout.Click += async (s, e) => await LogoutAsync();

            Button btnChangePassword = CreateMenuButton("🔑 Đổi mật khẩu");
            btnChangePassword.Click += async (s, e) => await ChangePasswordAsync();

            Button btnBaoCao = CreateMenuButton("📊 Thống kê & Báo cáo");
            Button btnNhanVien = CreateMenuButton("👥 Quản lý Nhân viên");
            Button btnTaiKhoan = CreateMenuButton("🔐 Quản lý Tài khoản");
            Button btnPhieuMuon = CreateMenuButton("📝 Sổ Mượn / Trả sách");
            Button btnDocGia = CreateMenuButton("🪪 Thẻ Độc giả");
            Button btnTacGiaTheLoai = CreateMenuButton("✒️ Tác giả & Thể loại");
            Button btnSach = CreateMenuButton("📚 Kho Sách & Ấn phẩm");
            Button btnDashboard = CreateMenuButton("📜 Tổng quan Thư viện");
            Button btnReaderAccount = CreateMenuButton("👤 Hồ sơ & Mượn phạt");

            var isAdmin = _role == AccountRoles.Admin;
            var isReader = _role == AccountRoles.Reader;
            btnNhanVien.Visible = isAdmin;
            btnTaiKhoan.Visible = isAdmin;
            btnDashboard.Visible = !isReader;
            btnTacGiaTheLoai.Visible = !isReader;
            btnDocGia.Visible = !isReader;
            btnPhieuMuon.Visible = !isReader;
            btnBaoCao.Visible = !isReader;
            btnReaderAccount.Visible = isReader;

            // Gắn sự kiện click
            btnDashboard.Click += (s, e) => { SetActiveButton(btnDashboard); LoadUserControl(new DashboardControl(), "📜 Tổng quan Sổ sách Thư viện"); };
            btnSach.Click += (s, e) =>
            {
                SetActiveButton(btnSach);
                if (isReader) LoadUserControl(new ReaderCatalogControl(), "📚 Tra cứu Sách");
                else LoadUserControl(new SachControl(), "📚 Kho Sách & Ấn phẩm");
            };
            btnReaderAccount.Click += (s, e) => { SetActiveButton(btnReaderAccount); LoadUserControl(new ReaderSelfServiceControl(), "👤 Hồ sơ & Lịch sử Mượn trả"); };
            btnTacGiaTheLoai.Click += (s, e) => { SetActiveButton(btnTacGiaTheLoai); LoadUserControl(new TacGiaTheLoaiControl(), "✒️ Danh mục Tác giả & Thể loại"); };
            btnDocGia.Click += (s, e) => { SetActiveButton(btnDocGia); LoadUserControl(new DocGiaControl(), "🪪 Hồ sơ Thẻ Độc giả"); };
            btnPhieuMuon.Click += (s, e) => { SetActiveButton(btnPhieuMuon); LoadUserControl(new MuonTraControl(_username), "📝 Sổ theo dõi Mượn / Trả sách"); };
            btnNhanVien.Click += (s, e) => { SetActiveButton(btnNhanVien); LoadUserControl(new NhanVienControl(), "🔑 Hồ sơ Nhân viên & Phân quyền"); };
            btnTaiKhoan.Click += (s, e) => { SetActiveButton(btnTaiKhoan); LoadUserControl(new TaiKhoanControl(), "🔐 Quản lý Tài khoản"); };
            btnBaoCao.Click += (s, e) => { SetActiveButton(btnBaoCao); LoadUserControl(new BaoCaoControl(), "📊 Báo cáo & Thống kê Thư viện"); };

            pnlSidebar.Controls.Add(btnBaoCao);
            pnlSidebar.Controls.Add(btnTaiKhoan);
            pnlSidebar.Controls.Add(btnNhanVien);
            pnlSidebar.Controls.Add(btnPhieuMuon);
            pnlSidebar.Controls.Add(btnDocGia);
            pnlSidebar.Controls.Add(btnTacGiaTheLoai);
            pnlSidebar.Controls.Add(btnSach);
            pnlSidebar.Controls.Add(btnDashboard);
            pnlSidebar.Controls.Add(btnReaderAccount);
            pnlSidebar.Controls.Add(lblBrand);
            pnlSidebar.Controls.Add(btnChangePassword);
            pnlSidebar.Controls.Add(btnLogout);

            SetActiveButton(isReader ? btnSach : btnDashboard);

            // 2. HEADER (Thanh tiêu đề màu giấy ngả vàng)
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = PaperHeader,
                Padding = new Padding(25, 0, 25, 0)
            };

            // Đường kẻ viền mảnh dưới Header
            Panel pnlHeaderBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 2,
                BackColor = Color.FromArgb(205, 190, 165)
            };

            lblHeaderTitle = new Label
            {
                Text = "📜 Tổng quan Sổ sách Thư viện",
                Font = new Font("Georgia", 15, FontStyle.Bold),
                ForeColor = WoodDark,
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 20, 0, 0)
            };

            string chucDanh = _role switch
            {
                AccountRoles.Admin => "Quản trị viên",
                AccountRoles.Reader => "Độc giả",
                _ => "Thủ thư"
            };
            Label lblUserInfo = new Label
            {
                Text = $"🪶 Người trực: {_username}  |  Cấp bậc: {chucDanh}",
                Font = new Font("Georgia", 10, FontStyle.Italic | FontStyle.Bold),
                ForeColor = Burgundy,
                Dock = DockStyle.Right,
                AutoSize = true,
                Padding = new Padding(0, 25, 0, 0)
            };

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblUserInfo);
            pnlHeader.Controls.Add(pnlHeaderBorder);

            // 3. CONTENT (Trang giấy làm việc chính)
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PaperCream
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateMenuButton(string text)
        {
            Button btn = new Button
            {
                Text = "  " + text,
                Dock = DockStyle.Top,
                Height = 52,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(230, 220, 205),
                BackColor = WoodDark,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SetActiveButton(Button activeBtn)
        {
            if (currentActiveButton != null)
            {
                currentActiveButton.BackColor = WoodDark;
                currentActiveButton.ForeColor = Color.FromArgb(230, 220, 205);
                currentActiveButton.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
            }

            currentActiveButton = activeBtn;
            currentActiveButton.BackColor = Burgundy; // Đổi sang màu đỏ bìa sách khi chọn
            currentActiveButton.ForeColor = GoldText;
            currentActiveButton.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        }

        public void LoadUserControl(UserControl uc, string title)
        {
            lblHeaderTitle.Text = title;
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }

        private void ShowPlaceholder(string screenName)
        {
            lblHeaderTitle.Text = screenName;
            pnlContent.Controls.Clear();
            Label lbl = new Label
            {
                Text = $"✒️ Trang sổ [{screenName}] đang được biên.soạn...",
                Font = new Font("Georgia", 13, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 95, 80),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlContent.Controls.Add(lbl);
        }

        private async Task LogoutAsync()
        {
            try { await _authApi.LogoutAsync(CancellationToken.None); }
            catch (Exception ex)
            {
                ApiSession.ClearAccessToken();
                MessageBox.Show($"Không thể xác nhận thu hồi phiên ở máy chủ. Token đã được xóa khỏi ứng dụng này.\n{ex.Message}", "Đăng xuất", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            _returnToLogin();
            Close();
        }

        private async Task ChangePasswordAsync()
        {
            using var dialog = new Form
            {
                Text = "Đổi mật khẩu",
                Size = new Size(430, 285),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = PaperCream
            };
            var current = PasswordField(dialog, "Mật khẩu hiện tại:", 30);
            var next = PasswordField(dialog, "Mật khẩu mới:", 85);
            var confirm = PasswordField(dialog, "Nhập lại mật khẩu:", 140);
            var save = new Button { Text = "Cập nhật", Location = new Point(115, 200), Size = new Size(90, 34), DialogResult = DialogResult.OK, BackColor = Burgundy, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var cancel = new Button { Text = "Hủy", Location = new Point(220, 200), Size = new Size(90, 34), DialogResult = DialogResult.Cancel };
            dialog.Controls.Add(save); dialog.Controls.Add(cancel); dialog.AcceptButton = save; dialog.CancelButton = cancel;
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (next.Text != confirm.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                await _authApi.ChangePasswordAsync(new ChangePasswordRequestDto { MatKhauHienTai = current.Text, MatKhauMoi = next.Text }, CancellationToken.None);
                MessageBox.Show("Đã đổi mật khẩu. Hãy đăng nhập lại bằng mật khẩu mới.", "Đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _returnToLogin();
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void HandleSessionExpired()
        {
            if (IsDisposed || _handlingSessionExpiry) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(HandleSessionExpired)); } catch (InvalidOperationException) { }
                return;
            }
            _handlingSessionExpiry = true;
            MessageBox.Show("Phiên đăng nhập đã hết hạn hoặc bị thu hồi. Vui lòng đăng nhập lại.", "Phiên hết hạn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _returnToLogin();
            Close();
        }

        private TextBox PasswordField(Control parent, string label, int y)
        {
            parent.Controls.Add(new Label { Text = label, Location = new Point(20, y + 3), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark });
            var textBox = new TextBox { Location = new Point(190, y), Width = 210, Font = new Font("Segoe UI", 10), PasswordChar = '●' };
            parent.Controls.Add(textBox);
            return textBox;
        }
    }
}
