using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;
using QuanLyThuVien.Desktop.UserControls;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Desktop.Forms
{
    public partial class MainForm : Form
    {
        private readonly string _role;
        private readonly string _username;
        private readonly Action _returnToLogin;
        private readonly AuthApiClient _authApi = new();
        private bool _handlingSessionExpiry;

        private Panel pnlContent = null!;
        private Label lblHeaderTitle = null!;
        private NavButton? currentActiveButton;

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
                LoadUserControl(new ReaderCatalogControl(), "Tra cứu sách");
            else
                LoadUserControl(new DashboardControl(), "Tổng quan");
        }

        private string RoleDisplayName => _role switch
        {
            AccountRoles.Admin => "Quản trị viên",
            AccountRoles.Reader => "Độc giả",
            _ => "Thủ thư"
        };

        private void SetupMainLayout()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = $"Hệ thống quản lý thư viện - {_username} ({RoleDisplayName})";
            ClientSize = new Size(1120, 690);
            MinimumSize = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UiTheme.PageBg;
            Font = UiTheme.Body;

            var isAdmin = _role == AccountRoles.Admin;
            var isReader = _role == AccountRoles.Reader;

            // ----- Thanh bên -----
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 248, BackColor = UiTheme.Sidebar };

            var nav = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, BackColor = UiTheme.Sidebar,
                Margin = Padding.Empty, Padding = new Padding(0, 8, 0, 0)
            };
            nav.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            if (isReader)
            {
                AddNav(nav, "Tra cứu sách", () => new ReaderCatalogControl(), "Tra cứu sách", firstActive: true);
                AddNav(nav, "Hồ sơ của tôi", () => new ReaderSelfServiceControl(), "Hồ sơ & lịch sử mượn trả");
            }
            else
            {
                AddNav(nav, "Tổng quan", () => new DashboardControl(), "Tổng quan", firstActive: true);
                if (isAdmin)
                {
                    // Admin chỉ quản lý tài khoản và duyệt yêu cầu khóa tài khoản.
                    AddCaption(nav, "QUẢN TRỊ");
                    AddNav(nav, "Duyệt khóa tài khoản", () => new YeuCauKhoaTaiKhoanControl(true), "Duyệt khóa tài khoản");
                    AddNav(nav, "Nhân viên", () => new NhanVienControl(), "Nhân viên");
                    AddNav(nav, "Tài khoản", () => new TaiKhoanControl(), "Tài khoản & Phân quyền");
                }
                else
                {
                    AddCaption(nav, "DANH MỤC");
                    AddNav(nav, "Kho sách", () => new SachControl(), "Kho sách");
                    AddNav(nav, "Tác giả & Thể loại", () => new TacGiaTheLoaiControl(), "Tác giả & Thể loại");
                    AddNav(nav, "Độc giả", () => new DocGiaControl(), "Độc giả");
                    AddCaption(nav, "NGHIỆP VỤ");
                    AddNav(nav, "Mượn / Trả sách", () => new MuonTraControl(_username), "Mượn / Trả sách");
                    AddNav(nav, "Duyệt yêu cầu mượn", () => new YeuCauMuonControl(), "Duyệt yêu cầu mượn");
                    AddNav(nav, "Yêu cầu khóa tài khoản", () => new YeuCauKhoaTaiKhoanControl(false), "Yêu cầu khóa tài khoản");
                    AddNav(nav, "Báo cáo & Thống kê", () => new BaoCaoControl(), "Báo cáo & Thống kê");
                }
            }

            // Hàng đệm cuối để các mục không bị kéo giãn lấp đầy chỗ trống.
            nav.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            nav.RowCount = nav.RowStyles.Count;

            var brand = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = UiTheme.Sidebar, Padding = new Padding(24, 0, 12, 0) };
            var brandLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = UiTheme.Sidebar,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            brandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            brandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            brandLayout.Controls.Add(new Label
            {
                Text = "THƯ VIỆN", Dock = DockStyle.Fill, Font = UiTheme.Font(15, FontStyle.Bold),
                ForeColor = Color.White, TextAlign = ContentAlignment.BottomLeft, Margin = Padding.Empty
            }, 0, 0);
            brandLayout.Controls.Add(new Label
            {
                Text = "Hệ thống quản lý", Dock = DockStyle.Fill, Font = UiTheme.Font(9.5f),
                ForeColor = UiTheme.SidebarText, TextAlign = ContentAlignment.TopLeft, Margin = Padding.Empty,
                Padding = new Padding(0, 2, 0, 0)
            }, 0, 1);
            brand.Controls.Add(brandLayout);

            var btnLogout = new NavButton("Đăng xuất") { Dock = DockStyle.Bottom, TextColor = Color.FromArgb(252, 165, 165) };
            btnLogout.Click += async (_, _) => await LogoutAsync();
            var btnChangePassword = new NavButton("Đổi mật khẩu") { Dock = DockStyle.Bottom };
            btnChangePassword.Click += async (_, _) => await ChangePasswordAsync();
            var footerLine = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = UiTheme.SidebarHover };

            sidebar.Controls.Add(nav);
            sidebar.Controls.Add(footerLine);
            sidebar.Controls.Add(btnChangePassword);
            sidebar.Controls.Add(btnLogout);
            // Control thêm sau cùng được dock trước: brand lên đỉnh, "Đăng xuất" xuống đáy, "Đổi mật khẩu" ngay trên nó.
            sidebar.Controls.Add(brand);

            // ----- Thanh tiêu đề -----
            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = UiTheme.Surface, Padding = new Padding(28, 0, 28, 0) };
            var headerLine = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = UiTheme.Border };
            lblHeaderTitle = new Label
            {
                Dock = DockStyle.Fill, UseMnemonic = false, Font = UiTheme.Title, ForeColor = UiTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
            };
            var userInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Right, Width = 280, ColumnCount = 1, RowCount = 2, BackColor = UiTheme.Surface,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            userInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            userInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            userInfo.Controls.Add(new Label
            {
                Text = _username, Dock = DockStyle.Fill, Font = UiTheme.BodyBold, ForeColor = UiTheme.Text,
                TextAlign = ContentAlignment.BottomRight, AutoEllipsis = true, Margin = Padding.Empty
            }, 0, 0);
            userInfo.Controls.Add(new Label
            {
                Text = RoleDisplayName, Dock = DockStyle.Fill, Font = UiTheme.Font(9.5f), ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.TopRight, Margin = Padding.Empty, Padding = new Padding(0, 1, 0, 0)
            }, 0, 1);
            header.Controls.Add(lblHeaderTitle);
            header.Controls.Add(userInfo);
            header.Controls.Add(headerLine);

            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.PageBg };

            Controls.Add(pnlContent);
            Controls.Add(header);
            Controls.Add(sidebar);
            Ui.ApplyDpi(this);
        }

        private void AddCaption(TableLayoutPanel nav, string text)
        {
            var caption = new Label
            {
                Text = text, AutoSize = true, Font = UiTheme.Font(9, FontStyle.Bold), ForeColor = UiTheme.SidebarCaption,
                Anchor = AnchorStyles.Left, Margin = new Padding(24, 18, 0, 6)
            };
            AddNavRow(nav, caption);
        }

        private NavButton AddNav(TableLayoutPanel nav, string text, Func<UserControl> factory, string title, bool firstActive = false)
        {
            var button = new NavButton(text) { Dock = DockStyle.Fill };
            button.Click += (_, _) => { SetActiveButton(button); LoadUserControl(factory(), title); };
            AddNavRow(nav, button);
            if (firstActive) SetActiveButton(button);
            return button;
        }

        private static void AddNavRow(TableLayoutPanel nav, Control control)
        {
            nav.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            nav.RowCount = nav.RowStyles.Count;
            nav.Controls.Add(control, 0, nav.RowCount - 1);
        }

        private void SetActiveButton(NavButton activeButton)
        {
            if (currentActiveButton != null) currentActiveButton.Active = false;
            currentActiveButton = activeButton;
            currentActiveButton.Active = true;
        }

        public void LoadUserControl(UserControl uc, string title)
        {
            lblHeaderTitle.Text = title;
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
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
                ClientSize = new Size(440, 330),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = UiTheme.Surface,
                Font = UiTheme.Body,
                Padding = new Padding(28, 24, 28, 16)
            };
            var current = PasswordBox();
            var next = PasswordBox();
            var confirm = PasswordBox();
            var form = new FormGrid(1);
            form.Add("Mật khẩu hiện tại", current);
            form.Add("Mật khẩu mới", next);
            form.Add("Nhập lại mật khẩu mới", confirm);
            var save = Ui.Button("Cập nhật", ButtonKind.Primary);
            save.DialogResult = DialogResult.OK;
            var cancel = Ui.Button("Hủy");
            cancel.DialogResult = DialogResult.Cancel;
            form.AddFull(Ui.ButtonBar(save, cancel));
            dialog.Controls.Add(form);
            dialog.AcceptButton = save;
            dialog.CancelButton = cancel;
            Ui.ApplyDpi(dialog);
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

        private static TextBox PasswordBox()
        {
            var box = Ui.TextBox();
            box.UseSystemPasswordChar = true;
            return box;
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
    }
}
