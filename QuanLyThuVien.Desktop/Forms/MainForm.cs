using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyThuVien.Desktop.UserControls;

namespace QuanLyThuVien.Desktop.Forms
{
    public partial class MainForm : Form
    {
        private string _role;
        private string _username;

        private Panel pnlSidebar = null!;
        private Panel pnlHeader = null!;
        private Panel pnlContent = null!;
        private Label lblHeaderTitle = null!;
        private Button? currentActiveButton;
        private readonly Dictionary<Type, UserControl> _views = new();

        // Bảng màu Thư viện cổ điển
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);       // Nâu gỗ đậm
        private readonly Color WoodDarker = Color.FromArgb(30, 20, 14);     // Nâu đen
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);      // Đỏ đô bìa sách
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);  // Màu giấy cũ
        private readonly Color PaperHeader = Color.FromArgb(238, 230, 212); // Màu giấy đậm hơn cho Header
        private readonly Color GoldText = Color.FromArgb(218, 176, 63);     // Vàng đồng

        public MainForm(string role, string username)
        {
            InitializeComponent();
            _role = role;
            _username = username;

            SetupMainLayout();
            LoadUserControl(() => new DashboardControl(), "📜 Tổng quan Sổ sách Thư viện");
        }

        private void SetupMainLayout()
        {
            this.Text = $"📖 Hệ thống Quản lý Thư viện - Người trực: {_username} ({Models.Roles.ToDisplay(_role)})";
            this.Size = new Size(1280, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            // Nội dung có thanh cuộn riêng khi không đủ chỗ cho biểu mẫu.
            this.MinimumSize = new Size(1000, 620);

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
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = GoldText,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = WoodDarker
            };

            // Các nút menu mang biểu tượng sách, bút lông, thẻ đọc
            Button btnLogout = CreateMenuButton("🚪 Đóng sổ (Đăng xuất)");
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.BackColor = Color.FromArgb(90, 25, 20);
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Application.Restart();
            };

            Button btnBaoCao = CreateMenuButton("📊 Thống kê & Báo cáo");
            Button btnNhanVien = CreateMenuButton("🔑 Quản lý Thủ thư");
            Button btnPhieuMuon = CreateMenuButton("📝 Sổ Mượn / Trả sách");
            Button btnDocGia = CreateMenuButton("🪪 Thẻ Độc giả");
            Button btnTacGiaTheLoai = CreateMenuButton("✒️ Tác giả & Thể loại");
            Button btnSach = CreateMenuButton("📚 Kho Sách & Ấn phẩm");
            Button btnDashboard = CreateMenuButton("📜 Tổng quan Thư viện");

            // Phân quyền: Ẩn Quản lý Thủ thư nếu không phải Admin
            // (Chỉ là lớp giao diện — API vẫn phải kiểm tra quyền từng endpoint)
            bool laAdmin = _role == Models.Roles.Admin;
            if (!laAdmin)
            {
                btnNhanVien.Visible = false;
            }

            // Gắn sự kiện click
            btnDashboard.Click += (s, e) => { SetActiveButton(btnDashboard); LoadUserControl(() => new DashboardControl(), "📜 Tổng quan Sổ sách Thư viện"); };
            btnSach.Click += (s, e) => { SetActiveButton(btnSach); LoadUserControl(() => new SachControl(), "📚 Kho Sách & Ấn phẩm"); };
            btnTacGiaTheLoai.Click += (s, e) => { SetActiveButton(btnTacGiaTheLoai); LoadUserControl(() => new TacGiaTheLoaiControl(), "✒️ Danh mục Tác giả & Thể loại"); };
            btnDocGia.Click += (s, e) => { SetActiveButton(btnDocGia); LoadUserControl(() => new DocGiaControl(), "🪪 Hồ sơ Thẻ Độc giả"); };
            btnPhieuMuon.Click += (s, e) => { SetActiveButton(btnPhieuMuon); LoadUserControl(() => new MuonTraControl(_username), "📝 Sổ theo dõi Mượn / Trả sách"); };
            btnNhanVien.Click += (s, e) =>
            {
                if (!laAdmin) return; // phòng thủ thêm, dù nút đã bị ẩn
                SetActiveButton(btnNhanVien);
                LoadUserControl(() => new NhanVienControl(_username), "🔑 Hồ sơ Nhân viên & Phân quyền");
            };
            btnBaoCao.Click += (s, e) => { SetActiveButton(btnBaoCao); LoadUserControl(() => new BaoCaoControl(), "📊 Báo cáo & Thống kê Thư viện"); };

            pnlSidebar.Controls.Add(btnBaoCao);
            pnlSidebar.Controls.Add(btnNhanVien);
            pnlSidebar.Controls.Add(btnPhieuMuon);
            pnlSidebar.Controls.Add(btnDocGia);
            pnlSidebar.Controls.Add(btnTacGiaTheLoai);
            pnlSidebar.Controls.Add(btnSach);
            pnlSidebar.Controls.Add(btnDashboard);
            pnlSidebar.Controls.Add(lblBrand);
            pnlSidebar.Controls.Add(btnLogout);

            SetActiveButton(btnDashboard);

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
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = WoodDark,
                Dock = DockStyle.Fill,
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };

            string chucDanh = laAdmin ? "Quản trị (Admin)" : "Thủ thư trực";
            Label lblUserInfo = new Label
            {
                Text = $"🪶 Người trực: {_username}  |  Cấp bậc: {chucDanh}",
                Font = new Font("Segoe UI", 10, FontStyle.Italic | FontStyle.Bold),
                ForeColor = Burgundy,
                Dock = DockStyle.Fill,
                AutoSize = false,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty
            };
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            headerLayout.Controls.Add(lblHeaderTitle, 0, 0);
            headerLayout.Controls.Add(lblUserInfo, 0, 1);
            pnlHeader.Controls.Add(headerLayout);
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

        private void LoadUserControl<T>(Func<T> createView, string title) where T : UserControl
        {
            if (!_views.TryGetValue(typeof(T), out var view))
            {
                view = createView();
                view.Dock = DockStyle.Fill;
                view.Visible = false;
                _views.Add(typeof(T), view);
                pnlContent.Controls.Add(view);
            }

            // Giữ cả dữ liệu giả lập, bộ lọc và nội dung nhập dở trong phiên làm việc.
            foreach (var cached in _views.Values)
                if (cached != view) cached.Visible = false;
            lblHeaderTitle.Text = title;
            view.Visible = true;
            view.BringToFront();
            // Các màn hình vẫn là con của pnlContent và được Dispose khi đóng MainForm.
        }
    }
}
