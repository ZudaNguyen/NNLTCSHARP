using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class MuonTraControl : UserControl
    {
        // Bảng màu Thư viện cổ điển
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        // Controls Lập Phiếu Mượn Mới (Bảng PhieuMuon)
        private ComboBox cboDocGia;
        private DateTimePicker dtpNgayMuon;
        private DateTimePicker dtpHanTra;
        private CheckedListBox clbSachMuon;
        private TextBox txtTimPhieu;
        private ComboBox cboLocTrangThai;

        // 2 Bảng Master - Detail
        private DataGridView dgvPhieuMuon;
        private DataGridView dgvChiTiet;

        // Controls Xác Nhận Trả Sách (Bảng ChiTiet_PhieuMuon)
        private Label lblSachDangChon;
        private Label lblTienPhat;
        private DateTimePicker dtpNgayTra;
        private NumericUpDown numTienPhat;
        private NumericUpDown numSoLuongTra;
        private ComboBox cboTinhTrangTra;
        private Button btnXacNhanTra;

        private readonly MuonTraApiClient _api = new();
        private readonly string _nguoiLap;
        private readonly List<SachMuonOptionDto> _sachConHang = [];
        private readonly List<MuonTraDto> _phieuMuon = [];
        private readonly Dictionary<int, MuonTraChiTietDto> _chiTietPhieu = [];
        private decimal? _mucPhatMoiNgay;
        private int? _selectedLineId;

        public MuonTraControl(string nguoiLap = "")
        {
            _nguoiLap = nguoiLap;
            InitializeComponent();
            SetupMuonTraUI();
            Load += async (_, _) => await LoadInitialDataAsync();
        }

        private void SetupMuonTraUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(18);

            // =========================================================
            // 1. KHU VỰC LẬP PHIẾU MƯỢN MỚI (TOP PANEL)
            // =========================================================
            Panel pnlCreateLoan = new Panel
            {
                Dock = DockStyle.Top,
                Height = 185,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitleCreate = new Label
            {
                Text = "✒️ Lập Phiếu Mượn Sách Mới (Ghi Sổ Mượn)",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlCreateLoan.Controls.Add(lblTitleCreate);

            // Cột trái: Độc giả, Ngày mượn, Hạn trả
            AddLabel(pnlCreateLoan, "Thẻ Độc giả (*):", 15, 42);
            cboDocGia = new ComboBox
            {
                Location = new Point(130, 39),
                Width = 270,
                Font = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlCreateLoan.Controls.Add(cboDocGia);

            AddLabel(pnlCreateLoan, "Ngày mượn:", 15, 80);
            dtpNgayMuon = new DateTimePicker
            {
                Location = new Point(130, 77),
                Width = 125,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlCreateLoan.Controls.Add(dtpNgayMuon);

            AddLabel(pnlCreateLoan, "Hạn trả (*):", 15, 118);
            dtpHanTra = new DateTimePicker
            {
                Location = new Point(130, 115),
                Width = 125,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(14) // Mặc định cho mượn 14 ngày
            };
            pnlCreateLoan.Controls.Add(dtpHanTra);

            Button btnLapPhieu = new Button
            {
                Text = "📜 Lập Phiếu Mượn",
                Location = new Point(265, 77),
                Size = new Size(135, 65),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLapPhieu.FlatAppearance.BorderSize = 0;
            btnLapPhieu.Click += ApiBtnLapPhieu_Click;
            pnlCreateLoan.Controls.Add(btnLapPhieu);

            // Cột phải: Chọn danh sách sách muốn mượn
            AddLabel(pnlCreateLoan, "Chọn ấn phẩm mượn (Chỉ hiển thị sách còn tồn kho > 0):", 425, 15);
            clbSachMuon = new CheckedListBox
            {
                Location = new Point(425, 39),
                Size = new Size(520, 130),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };
            pnlCreateLoan.Controls.Add(clbSachMuon);

            // =========================================================
            // 2. KHU VỰC DANH SÁCH PHIẾU MƯỢN - MASTER (MIDDLE PANEL)
            // =========================================================
            Panel pnlMaster = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                BackColor = PaperCream,
                Padding = new Padding(0, 10, 0, 10)
            };

            Panel pnlMasterBox = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            // Thanh tìm kiếm phiếu mượn
            Panel pnlMasterTop = new Panel { Dock = DockStyle.Top, Height = 36 };
            Label lblMasterTitle = new Label
            {
                Text = "📚 Danh sách Sổ Phiếu Mượn",
                Font = new Font("Georgia", 10.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(0, 6),
                AutoSize = true
            };

            Label lblTim = new Label { Text = "🔍 Tìm Mã phiếu / Độc giả:", Location = new Point(260, 8), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtTimPhieu = new TextBox { Location = new Point(430, 5), Width = 170, Font = new Font("Segoe UI", 9) };
            txtTimPhieu.TextChanged += (s, e) => ApiFilterMasterGrid();

            Label lblLoc = new Label { Text = "Trạng thái:", Location = new Point(615, 8), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cboLocTrangThai = new ComboBox { Location = new Point(685, 5), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cboLocTrangThai.Items.AddRange(new object[] { "-- Tất cả --", "Đang mượn", "Quá hạn", "Đã hoàn tất" });
            cboLocTrangThai.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndexChanged += (s, e) => ApiFilterMasterGrid();

            pnlMasterTop.Controls.AddRange(new Control[] { lblMasterTitle, lblTim, txtTimPhieu, lblLoc, cboLocTrangThai });

            dgvPhieuMuon = CreateStyledGrid();
            dgvPhieuMuon.Columns.Add("MaPhieu", "Mã Phiếu");
            dgvPhieuMuon.Columns["MaPhieu"].FillWeight = 55;
            dgvPhieuMuon.Columns.Add("DocGia", "Họ Tên Độc Giả");
            dgvPhieuMuon.Columns["DocGia"].FillWeight = 130;
            dgvPhieuMuon.Columns.Add("NhanVien", "Thủ Thư Lập");
            dgvPhieuMuon.Columns.Add("NgayMuon", "Ngày Mượn");
            dgvPhieuMuon.Columns.Add("HanTra", "Hạn Trả");
            dgvPhieuMuon.Columns.Add("TrangThai", "Trạng Thái Phiếu");
            dgvPhieuMuon.SelectionChanged += ApiDgvPhieuMuon_SelectionChanged;

            pnlMasterBox.Controls.Add(dgvPhieuMuon);
            pnlMasterBox.Controls.Add(pnlMasterTop);
            pnlMaster.Controls.Add(pnlMasterBox);

            // =========================================================
            // 3. CHI TIẾT PHIẾU & XÁC NHẬN TRẢ SÁCH - DETAIL (BOTTOM FILL)
            // =========================================================
            Panel pnlDetail = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            // Khu vực thao tác Trả sách bên phải
            Panel pnlReturnAction = new Panel
            {
                Dock = DockStyle.Right,
                Width = 310,
                BackColor = Color.FromArgb(245, 238, 225),
                Padding = new Padding(12),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblReturnTitle = new Label
            {
                Text = "⚖️ Nghiệp vụ Nhận Trả & Tính Phạt",
                Font = new Font("Georgia", 10, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(10, 8),
                AutoSize = true
            };

            lblSachDangChon = new Label
            {
                Text = "Ấn phẩm: (Chọn sách bên trái)",
                Font = new Font("Segoe UI", 9, FontStyle.Italic | FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(10, 32),
                Size = new Size(285, 22),
                AutoEllipsis = true
            };

            AddLabel(pnlReturnAction, "Ngày trả:", 10, 62);
            dtpNgayTra = new DateTimePicker
            {
                Location = new Point(105, 59),
                Width = 185,
                Font = new Font("Segoe UI", 9),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            dtpNgayTra.ValueChanged += (s, e) => ApiTinhTienPhatTuDong();

            AddLabel(pnlReturnAction, "Số lượng trả:", 10, 95);
            numSoLuongTra = new NumericUpDown { Location = new Point(105, 92), Width = 185, Minimum = 1, Maximum = 1000, Value = 1 };
            pnlReturnAction.Controls.Add(numSoLuongTra);

            AddLabel(pnlReturnAction, "Tình trạng:", 10, 128);
            cboTinhTrangTra = new ComboBox
            {
                Location = new Point(105, 125),
                Width = 185,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTinhTrangTra.Items.AddRange(new object[] { "Nguyên vẹn", "Rách hoặc bẩn nhẹ", "Hỏng nặng", "Mất" });
            cboTinhTrangTra.SelectedIndex = 0;
            cboTinhTrangTra.SelectedIndexChanged += (s, e) => ApiTinhTienPhatTuDong();

            lblTienPhat = new Label { Text = "Tiền phạt (đ):", Location = new Point(10, 161), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = WoodDark };
            pnlReturnAction.Controls.Add(lblTienPhat);
            numTienPhat = new NumericUpDown
            {
                Location = new Point(105, 158),
                Width = 185,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                ThousandsSeparator = true
            };

            btnXacNhanTra = new Button
            {
                Text = "✔️ Xác Nhận Trả Sách & Cộng Kho",
                Location = new Point(10, 191),
                Size = new Size(280, 36),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = Burgundy,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnXacNhanTra.FlatAppearance.BorderSize = 0;
            btnXacNhanTra.Click += ApiBtnXacNhanTra_Click;

            pnlReturnAction.Controls.AddRange(new Control[] {
                lblReturnTitle, lblSachDangChon, dtpNgayTra, cboTinhTrangTra, numTienPhat, btnXacNhanTra
            });
            numTienPhat.ReadOnly = true;
            numTienPhat.Maximum = decimal.MaxValue;
            var btnCauHinhPhat = new Button { Text = "⚙ Cấu hình mức phạt/ngày", Location = new Point(10, 202), Size = new Size(280, 30), BackColor = Color.FromArgb(238, 230, 212), ForeColor = WoodDark, FlatStyle = FlatStyle.Flat };
            btnCauHinhPhat.Click += ApiBtnCauHinhPhat_Click;
            pnlReturnAction.Controls.Add(btnCauHinhPhat);
            btnCauHinhPhat.Location = new Point(10, 233);
            var btnThuTienPhat = new Button { Text = "Thu tiền phạt", Location = new Point(10, 267), Size = new Size(280, 30), BackColor = ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnThuTienPhat.Click += ApiBtnThuTienPhat_Click;
            pnlReturnAction.Controls.Add(btnThuTienPhat);
            var btnLichSuThu = new Button { Text = "Xem lịch sử thanh toán", Location = new Point(10, 301), Size = new Size(280, 30), BackColor = CardBg, ForeColor = WoodDark, FlatStyle = FlatStyle.Flat };
            btnLichSuThu.Click += ApiBtnLichSuThu_Click;
            pnlReturnAction.Controls.Add(btnLichSuThu);
            // Bảng Chi tiết sách mượn bên trái
            Panel pnlDetailLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 10, 0) };
            Label lblDetailTitle = new Label
            {
                Text = "📖 Chi tiết các ấn phẩm trong phiếu đang chọn (Bảng ChiTiet_PhieuMuon)",
                Font = new Font("Georgia", 10.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Dock = DockStyle.Top,
                Height = 28
            };

            dgvChiTiet = CreateStyledGrid();
            dgvChiTiet.Columns.Add("MaSach", "Mã Sách");
            dgvChiTiet.Columns["MaSach"].FillWeight = 50;
            dgvChiTiet.Columns.Add("TenSach", "Tên Ấn Phẩm");
            dgvChiTiet.Columns["TenSach"].FillWeight = 150;
            dgvChiTiet.Columns.Add("SoLuong", "SL");
            dgvChiTiet.Columns["SoLuong"].FillWeight = 35;
            dgvChiTiet.Columns.Add("NgayTra", "Ngày Trả Thực Tế");
            dgvChiTiet.Columns.Add("TienPhat", "Tiền Phạt (VNĐ)");
            dgvChiTiet.Columns.Add("TinhTrang", "Tình Trạng Sách");
            dgvChiTiet.CellClick += ApiDgvChiTiet_CellClick;

            pnlDetailLeft.Controls.Add(dgvChiTiet);
            pnlDetailLeft.Controls.Add(lblDetailTitle);

            pnlDetail.Controls.Add(pnlDetailLeft);
            pnlDetail.Controls.Add(pnlReturnAction);

            // Add theo thứ tự ngược cho Dock
            this.Controls.Add(pnlDetail);
            this.Controls.Add(pnlMaster);
            this.Controls.Add(pnlCreateLoan);
        }

        // =========================================================
        // HÀM HỖ TRỢ & XỬ LÝ NGHIỆP VỤ FRONTEND
        // =========================================================
        private DataGridView CreateStyledGrid()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 30 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9, FontStyle.Bold);

            dgv.DefaultCellStyle.BackColor = CardBg;
            dgv.DefaultCellStyle.ForeColor = WoodDark;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgv.DefaultCellStyle.SelectionForeColor = WoodDark;
            return dgv;
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = WoodDark,
                AutoSize = true
            });
        }

    }
}
