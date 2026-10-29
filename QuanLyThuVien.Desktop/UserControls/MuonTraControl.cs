using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyThuVien.Desktop.Models;

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
        private DateTimePicker dtpNgayTra;
        private NumericUpDown numTienPhat;
        private ComboBox cboTinhTrangTra;
        private Button btnXacNhanTra;

        // Lưu dữ liệu giả lập của Chi Tiết Phiếu Mượn theo Mã Phiếu
        private class ChiTietItem
        {
            public string MaSach { get; set; }
            public string TenSach { get; set; }
            public int SoLuong { get; set; }
            public string NgayTra { get; set; }
            public decimal TienPhat { get; set; }
            public string TinhTrang { get; set; }
        }
        private Dictionary<string, List<ChiTietItem>> mockChiTietData = new Dictionary<string, List<ChiTietItem>>();

        // Thông tin phiếu gắn vào Tag của dòng (không phải đọc/ghép lại từ chuỗi hiển thị)
        private class PhieuItem
        {
            public string Ma { get; set; } = "";
            public DateTime HanTra { get; set; }
        }

        private const string ChuaTra = "Chưa trả";

        private readonly string _currentUser;      // thủ thư đang đăng nhập (ghi vào phiếu)
        private ChiTietItem? _selectedChiTiet;
        private bool _loadingChiTiet;
        private string? _loadedMaPhieu;
        private int _soPhieuTiepTheo = 1;
        private List<SachItem> _kho = new List<SachItem>();
        private List<DocGiaItem> _docGias = new List<DocGiaItem>();

        public MuonTraControl() : this(string.Empty) { }

        public MuonTraControl(string currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            SetupMuonTraUI();
            LoadMockData();
            UI.ScrollableView.Configure(this, new Size(1040, 700));
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
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlCreateLoan.Controls.Add(lblTitleCreate);

            // Cột trái: Độc giả, Ngày mượn, Hạn trả
            AddLabel(pnlCreateLoan, "Độc giả (*):", 15, 42);
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
                Value = DateTime.Today,
                Enabled = false // Ngày mượn = ngày hiện tại (khi có API sẽ do server quyết định)
            };
            pnlCreateLoan.Controls.Add(dtpNgayMuon);

            AddLabel(pnlCreateLoan, "Hạn trả (*):", 15, 118);
            dtpHanTra = new DateTimePicker
            {
                Location = new Point(130, 115),
                Width = 125,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(BusinessRules.SoNgayMuonMacDinh)
            };
            pnlCreateLoan.Controls.Add(dtpHanTra);

            Button btnLapPhieu = new Button
            {
                Text = "📜 Lập Phiếu Mượn",
                Location = new Point(265, 77),
                Size = new Size(135, 65),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLapPhieu.FlatAppearance.BorderSize = 0;
            btnLapPhieu.Click += BtnLapPhieu_Click;
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
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(0, 6),
                AutoSize = true
            };

            Label lblTim = new Label { Text = "🔍 Tìm Mã phiếu / Độc giả:", Location = new Point(260, 8), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtTimPhieu = new TextBox { Location = new Point(430, 5), Width = 170, Font = new Font("Segoe UI", 9) };
            txtTimPhieu.TextChanged += (s, e) => FilterMasterGrid();

            Label lblLoc = new Label { Text = "Trạng thái:", Location = new Point(615, 8), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cboLocTrangThai = new ComboBox { Location = new Point(685, 5), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cboLocTrangThai.Items.AddRange(new object[] { "-- Tất cả --", "Đang mượn", "Quá hạn", "Đã hoàn tất" });
            cboLocTrangThai.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndexChanged += (s, e) => FilterMasterGrid();

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
            dgvPhieuMuon.CurrentCellChanged += DgvPhieuMuon_CurrentCellChanged;

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
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
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
                Value = DateTime.Today,
                Enabled = false // Ngày trả = hôm nay, không cho chỉnh tay để tránh lùi ngày né phạt
            };

            AddLabel(pnlReturnAction, "Tình trạng:", 10, 95);
            cboTinhTrangTra = new ComboBox
            {
                Location = new Point(105, 92),
                Width = 185,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTinhTrangTra.Items.AddRange(new object[] {
                "Nguyên vẹn (Tốt)",
                $"Rách / Bẩn nhẹ (+{BusinessRules.PhiSachRach:N0}đ)",
                $"Hư hỏng nặng / Mất (+{BusinessRules.PhiSachHuHong:N0}đ)"
            });
            cboTinhTrangTra.SelectedIndex = 0;
            cboTinhTrangTra.SelectedIndexChanged += (s, e) => TinhTienPhatTuDong();

            AddLabel(pnlReturnAction, "Tiền phạt (đ):", 10, 128);
            numTienPhat = new NumericUpDown
            {
                Location = new Point(105, 125),
                Width = 185,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Maximum = BusinessRules.TienPhatToiDa,
                ThousandsSeparator = true,
                Enabled = false // Tiền phạt do hệ thống tính; không cho sửa tay
            };

            btnXacNhanTra = new Button
            {
                Text = "✔️ Xác Nhận Trả Sách & Cộng Kho",
                Location = new Point(10, 158),
                Size = new Size(280, 36),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Burgundy,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false // chỉ bật sau khi chọn một ấn phẩm chưa trả
            };
            btnXacNhanTra.FlatAppearance.BorderSize = 0;
            btnXacNhanTra.Click += BtnXacNhanTra_Click;

            pnlReturnAction.Controls.AddRange(new Control[] {
                lblReturnTitle, lblSachDangChon, dtpNgayTra, cboTinhTrangTra, numTienPhat, btnXacNhanTra
            });

            // Bảng Chi tiết sách mượn bên trái
            Panel pnlDetailLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 10, 0) };
            Label lblDetailTitle = new Label
            {
                Text = "📖 Chi tiết các ấn phẩm trong phiếu đang chọn (Bảng ChiTiet_PhieuMuon)",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
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
            dgvChiTiet.CurrentCellChanged += DgvChiTiet_CurrentCellChanged;

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
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

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

        private void LoadMockData()
        {
            // Độc giả (mô phỏng dữ liệu từ API). Chỉ độc giả đang hoạt động mới xuất hiện để lập phiếu.
            _docGias = new List<DocGiaItem>
            {
                new DocGiaItem { MaDocGia = "DG001", HoTen = "Nguyễn Văn A", HoatDong = true },
                new DocGiaItem { MaDocGia = "DG002", HoTen = "Trần Thị B", HoatDong = true },
                new DocGiaItem { MaDocGia = "DG003", HoTen = "Lê Hoàng C", HoatDong = false }
            };
            cboDocGia.Items.Clear();
            foreach (var dg in _docGias)
            {
                if (dg.HoatDong) cboDocGia.Items.Add(dg);
            }
            if (cboDocGia.Items.Count > 0) cboDocGia.SelectedIndex = 0;

            // Kho sách (mô phỏng). Số lượng còn được trừ/cộng khi mượn/trả.
            _kho = new List<SachItem>
            {
                new SachItem { MaSach = "S01", TenSach = "Clean Code - Mã Sạch", SoLuongCon = 15 },
                new SachItem { MaSach = "S02", TenSach = "Giáo trình Lập trình C# .NET", SoLuongCon = 42 },
                new SachItem { MaSach = "S04", TenSach = "Thiết kế Cơ sở dữ liệu SQL Server", SoLuongCon = 8 },
                new SachItem { MaSach = "S05", TenSach = "Kiến trúc Phần mềm Hiện đại", SoLuongCon = 5 }
            };
            RefreshSachList();

            // Chi tiết phiếu trước, rồi mới thêm dòng phiếu (CurrentCellChanged sẽ đọc chi tiết)
            mockChiTietData["PM-001"] = new List<ChiTietItem>
            {
                new ChiTietItem { MaSach = "S01", TenSach = "Clean Code - Mã Sạch", SoLuong = 1, NgayTra = ChuaTra, TienPhat = 0, TinhTrang = "Đang mượn" },
                new ChiTietItem { MaSach = "S02", TenSach = "Giáo trình Lập trình C# .NET", SoLuong = 1, NgayTra = "14/10/2026", TienPhat = 0, TinhTrang = "Nguyên vẹn (Tốt)" }
            };
            mockChiTietData["PM-002"] = new List<ChiTietItem>
            {
                new ChiTietItem { MaSach = "S04", TenSach = "Thiết kế Cơ sở dữ liệu SQL Server", SoLuong = 1, NgayTra = ChuaTra, TienPhat = 0, TinhTrang = "Đang mượn" }
            };

            ThemDongPhieu("PM-001", "Nguyễn Văn A", "thuthu01", new DateTime(2026, 10, 1), new DateTime(2026, 10, 15));
            ThemDongPhieu("PM-002", "Trần Thị B", "admin", new DateTime(2026, 10, 12), new DateTime(2026, 10, 26));
            _soPhieuTiepTheo = 3;

            CapNhatTrangThaiTatCa();

            if (dgvPhieuMuon.Rows.Count > 0)
            {
                dgvPhieuMuon.CurrentCell = dgvPhieuMuon.Rows[0].Cells[0];
                LoadChiTietByMaPhieu("PM-001");
            }
        }

        private DataGridViewRow ThemDongPhieu(string ma, string docGia, string nhanVien, DateTime ngayMuon, DateTime hanTra)
        {
            int idx = dgvPhieuMuon.Rows.Add(ma, docGia, nhanVien,
                ngayMuon.ToString("dd/MM/yyyy"), hanTra.ToString("dd/MM/yyyy"), "Đang mượn");
            var row = dgvPhieuMuon.Rows[idx];
            row.Tag = new PhieuItem { Ma = ma, HanTra = hanTra }; // dữ liệu thật nằm ở Tag, không phải parse từ chữ hiển thị
            return row;
        }

        private void RefreshSachList()
        {
            clbSachMuon.Items.Clear();
            foreach (var s in _kho)
            {
                if (s.SoLuongCon > 0) clbSachMuon.Items.Add(s);
            }
        }

        // Trạng thái phiếu được TÍNH từ chi tiết + hạn trả, không lưu cứng nên không bị lỗi thời
        private string TinhTrangPhieu(string ma, DateTime hanTra)
        {
            if (mockChiTietData.TryGetValue(ma, out var ds) && ds.Count > 0 && ds.TrueForAll(ct => ct.NgayTra != ChuaTra))
                return "Đã hoàn tất";
            return hanTra.Date < DateTime.Today ? "Quá hạn" : "Đang mượn";
        }

        private void CapNhatTrangThai(DataGridViewRow row)
        {
            if (row.Tag is PhieuItem p)
                row.Cells["TrangThai"].Value = TinhTrangPhieu(p.Ma, p.HanTra);
        }

        private void CapNhatTrangThaiTatCa()
        {
            foreach (DataGridViewRow row in dgvPhieuMuon.Rows)
                CapNhatTrangThai(row);
        }

        private void DgvPhieuMuon_CurrentCellChanged(object? sender, EventArgs e)
        {
            string maPhieu = dgvPhieuMuon.CurrentRow?.Cells["MaPhieu"].Value?.ToString() ?? "";
            if (maPhieu != _loadedMaPhieu)
                LoadChiTietByMaPhieu(maPhieu);
        }

        private void LoadChiTietByMaPhieu(string maPhieu)
        {
            _loadingChiTiet = true;
            try
            {
                _loadedMaPhieu = maPhieu;
                dgvChiTiet.Rows.Clear();
                ResetReturnSelection();
                if (mockChiTietData.TryGetValue(maPhieu, out var items))
                {
                    foreach (var item in items)
                        dgvChiTiet.Rows.Add(item.MaSach, item.TenSach, item.SoLuong, item.NgayTra, item.TienPhat.ToString("N0"), item.TinhTrang);
                }
                UI.GridSelection.Clear(dgvChiTiet);
            }
            finally { _loadingChiTiet = false; }
        }

        private void ResetReturnSelection()
        {
            _selectedChiTiet = null;
            btnXacNhanTra.Enabled = false;
            cboTinhTrangTra.Enabled = false;
            cboTinhTrangTra.SelectedIndex = 0;
            lblSachDangChon.Text = "Ấn phẩm: (Chọn 1 dòng bên trái)";
            dtpNgayTra.Value = DateTime.Today;
            numTienPhat.Value = 0;
        }

        // Lấy chi tiết theo Mã sách (không dùng chỉ số dòng, vì lưới có thể bị sắp xếp lại khi bấm tiêu đề cột)
        private ChiTietItem? LayChiTietDangChon(int rowIndex)
        {
            if (dgvPhieuMuon.CurrentRow == null || rowIndex < 0 || rowIndex >= dgvChiTiet.Rows.Count) return null;
            string maPhieu = dgvPhieuMuon.CurrentRow.Cells["MaPhieu"].Value?.ToString() ?? "";
            string maSach = dgvChiTiet.Rows[rowIndex].Cells["MaSach"].Value?.ToString() ?? "";
            if (!mockChiTietData.TryGetValue(maPhieu, out var ds)) return null;
            return ds.Find(c => c.MaSach == maSach);
        }

        private void DgvChiTiet_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (_loadingChiTiet) return;
            var item = LayChiTietDangChon(dgvChiTiet.CurrentRow?.Index ?? -1);
            if (item == null)
            {
                ResetReturnSelection();
                return;
            }
            if (ReferenceEquals(item, _selectedChiTiet)) return;

            _selectedChiTiet = item;
            lblSachDangChon.Text = $"Ấn phẩm: {item.TenSach}";

            bool chuaTra = item.NgayTra == ChuaTra;
            btnXacNhanTra.Enabled = chuaTra;
            cboTinhTrangTra.Enabled = chuaTra;

            if (chuaTra)
            {
                cboTinhTrangTra.SelectedIndex = 0;
                TinhTienPhatTuDong();
            }
            else
            {
                cboTinhTrangTra.SelectedItem = item.TinhTrang;
                numTienPhat.Value = Math.Min(BusinessRules.TienPhatToiDa, item.TienPhat);
            }
        }

        // Quy tắc: phạt = số ngày quá hạn x mức phạt/ngày + phí hư hỏng. Các mức lấy từ BusinessRules (sau này từ API cấu hình).
        private void TinhTienPhatTuDong()
        {
            if (_selectedChiTiet == null || _selectedChiTiet.NgayTra != ChuaTra
                || dgvPhieuMuon.CurrentRow?.Tag is not PhieuItem p) return;
            dtpNgayTra.Value = DateTime.Today;

            decimal phatQuaHan = 0;
            int soNgayTre = (DateTime.Today - p.HanTra.Date).Days;
            if (soNgayTre > 0)
                phatQuaHan = soNgayTre * BusinessRules.MucPhatMoiNgay;

            decimal phatHuHong = 0;
            if (cboTinhTrangTra.SelectedIndex == 1) phatHuHong = BusinessRules.PhiSachRach;
            else if (cboTinhTrangTra.SelectedIndex == 2) phatHuHong = BusinessRules.PhiSachHuHong;

            // Chặn trần để không bao giờ vượt Maximum của NumericUpDown (tránh ArgumentOutOfRangeException)
            numTienPhat.Value = Math.Min(BusinessRules.TienPhatToiDa, phatQuaHan + phatHuHong);
        }

        private void BtnLapPhieu_Click(object? sender, EventArgs e)
        {
            // 1. Độc giả phải được chọn và đang hoạt động
            if (cboDocGia.SelectedItem is not DocGiaItem docGia)
            {
                MessageBox.Show("Vui lòng chọn độc giả!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!docGia.HoatDong)
            {
                MessageBox.Show("Độc giả này đang bị khóa thẻ! Chỉ độc giả trạng thái Hoạt động mới được mượn sách.",
                                "Từ chối lập phiếu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2. Kiểm tra ngày
            DateTime ngayMuon = dtpNgayMuon.Value.Date;
            DateTime hanTra = dtpHanTra.Value.Date;
            if (hanTra < ngayMuon)
            {
                MessageBox.Show("Ngày hẹn trả không được nhỏ hơn ngày mượn sách!",
                                "Lỗi ngày tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if ((hanTra - ngayMuon).Days > BusinessRules.SoNgayMuonToiDa)
            {
                MessageBox.Show($"Thời hạn mượn tối đa là {BusinessRules.SoNgayMuonToiDa} ngày!",
                                "Vượt thời hạn mượn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Kiểm tra số sách
            if (clbSachMuon.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng tích chọn ít nhất 1 ấn phẩm để mượn!",
                                "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (clbSachMuon.CheckedItems.Count > BusinessRules.SoSachToiDaMoiPhieu)
            {
                MessageBox.Show($"Mỗi phiếu chỉ được mượn tối đa {BusinessRules.SoSachToiDaMoiPhieu} ấn phẩm!",
                                "Vượt số sách tối đa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var chon = clbSachMuon.CheckedItems.Cast<SachItem>().ToList();
            foreach (var s in chon)
            {
                if (s.SoLuongCon <= 0)
                {
                    MessageBox.Show($"Ấn phẩm [{s.TenSach}] đã hết trong kho!", "Hết sách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RefreshSachList();
                    return;
                }
            }

            // 4. Ghi phiếu + trừ tồn kho (khi có API: cùng một transaction ở server)
            string maPhieu = $"PM-{_soPhieuTiepTheo:000}";
            _soPhieuTiepTheo++;

            var chiTietList = new List<ChiTietItem>();
            foreach (var s in chon)
            {
                chiTietList.Add(new ChiTietItem
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    SoLuong = 1,
                    NgayTra = ChuaTra,
                    TienPhat = 0,
                    TinhTrang = "Đang mượn"
                });
                s.SoLuongCon -= 1;
            }
            mockChiTietData[maPhieu] = chiTietList;

            string nguoiLap = string.IsNullOrEmpty(_currentUser) ? "(không rõ)" : _currentUser;
            var row = ThemDongPhieu(maPhieu, docGia.HoTen, nguoiLap, ngayMuon, hanTra);
            CapNhatTrangThai(row);

            RefreshSachList(); // làm mới danh sách (đồng thời bỏ tích chọn)

            MessageBox.Show($"Đã lập phiếu mượn [{maPhieu}] và trừ tồn kho {chon.Count} ấn phẩm.",
                            "Ghi sổ thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXacNhanTra_Click(object? sender, EventArgs e)
        {
            if (_selectedChiTiet == null || dgvPhieuMuon.CurrentRow == null || dgvChiTiet.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách trong bảng Chi tiết bên trái để xác nhận trả!",
                                "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string maPhieu = dgvPhieuMuon.CurrentRow.Cells["MaPhieu"].Value?.ToString() ?? "";
            var item = LayChiTietDangChon(dgvChiTiet.CurrentRow.Index);
            if (item == null || !ReferenceEquals(item, _selectedChiTiet)) return;

            if (item.NgayTra != ChuaTra)
            {
                MessageBox.Show("Ấn phẩm này đã được trả trước đó rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TinhTienPhatTuDong();
            if (MessageBox.Show($"Xác nhận nhận trả ấn phẩm [{item.TenSach}] với tiền phạt {numTienPhat.Value:N0}đ?",
                    "Xác nhận trả sách", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            bool matHuNang = cboTinhTrangTra.SelectedIndex == 2;

            // Cập nhật chi tiết trả sách (ngày trả = hôm nay)
            item.NgayTra = DateTime.Today.ToString("dd/MM/yyyy");
            item.TienPhat = numTienPhat.Value;
            item.TinhTrang = cboTinhTrangTra.SelectedItem?.ToString() ?? "";

            // Cộng lại kho, trừ trường hợp hư hỏng nặng / mất (không nhập lại kho)
            if (!matHuNang)
            {
                var s = _kho.Find(x => x.MaSach == item.MaSach);
                if (s != null) s.SoLuongCon += item.SoLuong;
                RefreshSachList();
            }

            LoadChiTietByMaPhieu(maPhieu);
            CapNhatTrangThai(dgvPhieuMuon.CurrentRow);

            string ghiChuKho = matHuNang ? "Sách hư hỏng nặng/mất nên không nhập lại kho." : "Đã cộng lại số lượng tồn kho.";
            if (dgvPhieuMuon.CurrentRow.Cells["TrangThai"].Value?.ToString() == "Đã hoàn tất")
            {
                MessageBox.Show($"Đã nhận trả sách! Toàn bộ sách trong phiếu [{maPhieu}] đã được trả đủ -> Phiếu chuyển sang [Đã hoàn tất].\n{ghiChuKho}",
                                "Hoàn tất phiếu mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Đã ghi nhận trả cuốn sách này. {ghiChuKho}",
                                "Trả sách thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FilterMasterGrid()
        {
            string kw = txtTimPhieu.Text.Trim().ToLower();
            string status = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem?.ToString() ?? "" : "";

            CapNhatTrangThaiTatCa();

            // Không thể ẩn dòng đang là dòng hiện hành (ném InvalidOperationException) -> bỏ chọn trước
            dgvPhieuMuon.CurrentCell = null;

            foreach (DataGridViewRow row in dgvPhieuMuon.Rows)
            {
                string ma = row.Cells["MaPhieu"].Value?.ToString()?.ToLower() ?? "";
                string dg = row.Cells["DocGia"].Value?.ToString()?.ToLower() ?? "";
                string tt = row.Cells["TrangThai"].Value?.ToString() ?? "";

                bool matchKw = string.IsNullOrEmpty(kw) || ma.Contains(kw) || dg.Contains(kw);
                bool matchSt = string.IsNullOrEmpty(status) || tt == status;

                row.Visible = matchKw && matchSt;
            }

            // Chọn lại dòng hiển thị đầu tiên để bảng chi tiết khớp với bảng phiếu
            var first = dgvPhieuMuon.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Visible);
            if (first != null)
                dgvPhieuMuon.CurrentCell = first.Cells[0];
            else
                LoadChiTietByMaPhieu("");
        }
    }
}
