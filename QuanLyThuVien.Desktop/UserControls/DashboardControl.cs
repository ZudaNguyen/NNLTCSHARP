using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            InitializeComponent();
            SetupLibraryDashboardUI();
            UI.ScrollableView.Configure(this, new Size(1030, 480));
        }

        private void SetupLibraryDashboardUI()
        {
            Color paperCream = Color.FromArgb(247, 243, 233);
            Color cardBg = Color.FromArgb(255, 252, 245);
            Color woodDark = Color.FromArgb(44, 30, 22);

            this.Dock = DockStyle.Fill;
            this.BackColor = paperCream;
            this.Padding = new Padding(25);

            // 1. Khu vực 4 Thẻ Mục Lục (Library Index Cards)
            FlowLayoutPanel flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 145,
                AutoSize = false,
                WrapContents = false
            };

            flpCards.Controls.Add(CreateLibraryCard("📚 TỔNG ẤN PHẨM", "1,250", "Cuốn sách trong kho", Color.FromArgb(102, 51, 20)));
            flpCards.Controls.Add(CreateLibraryCard("📖 SÁCH ĐANG MƯỢN", "84", "Đang lưu hành", Color.FromArgb(45, 106, 79)));
            flpCards.Controls.Add(CreateLibraryCard("⏳ PHIẾU QUÁ HẠN", "12", "Cần thu hồi gấp", Color.FromArgb(158, 42, 43)));
            flpCards.Controls.Add(CreateLibraryCard("🪪 THẺ ĐỘC GIẢ", "340", "Độc giả đang hoạt động", Color.FromArgb(180, 120, 20)));

            // 2. Khu vực Sổ ghi chép mượn trả bên dưới
            Panel pnlLedger = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = cardBg,
                Padding = new Padding(18),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblLedgerTitle = new Label
            {
                Text = "✒️ Sổ theo dõi các phiếu mượn cần lưu ý (Sắp đến hạn & Quá hạn)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = woodDark,
                Dock = DockStyle.Top,
                Height = 38
            };

            DataGridView dgvRecent = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = cardBg,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 34 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            // Trang trí Header của bảng giống sổ sách
            dgvRecent.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvRecent.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvRecent.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Màu dòng khi click chọn
            dgvRecent.DefaultCellStyle.BackColor = cardBg;
            dgvRecent.DefaultCellStyle.ForeColor = woodDark;
            dgvRecent.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgvRecent.DefaultCellStyle.SelectionForeColor = woodDark;

            dgvRecent.Columns.Add("MaPhieu", "Mã Sổ Phiếu");
            dgvRecent.Columns.Add("DocGia", "Họ Tên Độc Giả");
            dgvRecent.Columns.Add("TenSach", "Ấn Phẩm Đang Giữ");
            dgvRecent.Columns.Add("NgayMuon", "Ngày Mượn");
            dgvRecent.Columns.Add("HanTra", "Hạn Hoàn Trả");
            dgvRecent.Columns.Add("TrangThai", "Tình Trạng");

            // TODO(API): thay dữ liệu mẫu (và 4 số liệu ở trên) bằng dữ liệu từ API.
            // Tình trạng được TÍNH từ hạn trả, không ghi cứng chữ "Quá hạn N ngày".
            var phieuMau = new (string Ma, string DG, string Sach, DateTime Muon, DateTime Han)[]
            {
                ("PM-001", "Nguyễn Văn A", "Lập trình C# Toàn tập", new DateTime(2026, 10, 1), new DateTime(2026, 10, 15)),
                ("PM-002", "Trần Thị B", "Cơ sở dữ liệu SQL Server", new DateTime(2026, 10, 10), new DateTime(2026, 10, 24)),
                ("PM-003", "Lê Hoàng C", "Thiết kế Kiến trúc Phần mềm", new DateTime(2026, 10, 5), new DateTime(2026, 10, 19))
            };
            foreach (var p in phieuMau)
            {
                int ngayTre = (DateTime.Today - p.Han.Date).Days;
                string tinhTrang = ngayTre > 0 ? $"⚠️ Quá hạn {ngayTre} ngày" : "📖 Đang đọc";
                dgvRecent.Rows.Add(p.Ma, p.DG, p.Sach, p.Muon.ToString("dd/MM/yyyy"), p.Han.ToString("dd/MM/yyyy"), tinhTrang);
            }

            pnlLedger.Controls.Add(dgvRecent);
            pnlLedger.Controls.Add(lblLedgerTitle);

            this.Controls.Add(pnlLedger);
            this.Controls.Add(flpCards);
        }

        private Panel CreateLibraryCard(string title, string value, string subtitle, Color spineColor)
        {
            Panel card = new Panel
            {
                Size = new Size(225, 120),
                BackColor = Color.FromArgb(255, 252, 245),
                Margin = new Padding(0, 0, 20, 0),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Gáy sách màu bên trái thẻ
            Panel bookSpine = new Panel
            {
                Dock = DockStyle.Left,
                Width = 8,
                BackColor = spineColor
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 70, 55),
                Location = new Point(18, 14),
                AutoSize = true
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = spineColor,
                Location = new Point(16, 40),
                AutoSize = true
            };

            Label lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(18, 88),
                AutoSize = true
            };

            card.Controls.Add(lblSub);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(bookSpine);

            return card;
        }
    }
}
