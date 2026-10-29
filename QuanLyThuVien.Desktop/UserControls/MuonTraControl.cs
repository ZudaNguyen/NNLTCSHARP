using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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

        public MuonTraControl()
        {
            InitializeComponent();
            SetupMuonTraUI();
            LoadMockData();
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
                Font = new Font("Georgia", 10.5f, FontStyle.Bold),
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
            dgvPhieuMuon.SelectionChanged += DgvPhieuMuon_SelectionChanged;

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
            dtpNgayTra.ValueChanged += (s, e) => TinhTienPhatTuDong();

            AddLabel(pnlReturnAction, "Tình trạng:", 10, 95);
            cboTinhTrangTra = new ComboBox
            {
                Location = new Point(105, 92),
                Width = 185,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTinhTrangTra.Items.AddRange(new object[] { "Nguyên vẹn (Tốt)", "Rách / Bẩn nhẹ (+20.000đ)", "Hư hỏng nặng / Mất (+100.000đ)" });
            cboTinhTrangTra.SelectedIndex = 0;
            cboTinhTrangTra.SelectedIndexChanged += (s, e) => TinhTienPhatTuDong();

            AddLabel(pnlReturnAction, "Tiền phạt (đ):", 10, 128);
            numTienPhat = new NumericUpDown
            {
                Location = new Point(105, 125),
                Width = 185,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Maximum = 5000000,
                Increment = 5000,
                ThousandsSeparator = true
            };

            btnXacNhanTra = new Button
            {
                Text = "✔️ Xác Nhận Trả Sách & Cộng Kho",
                Location = new Point(10, 158),
                Size = new Size(280, 36),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = Burgundy,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
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
            dgvChiTiet.CellClick += DgvChiTiet_CellClick;

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

        private void LoadMockData()
        {
            // Danh sách độc giả (Có người hoạt động, có người bị khóa để test quy tắc nghiệp vụ)
            cboDocGia.Items.AddRange(new object[] {
                "DG001 - Nguyễn Văn A (Hoạt động)",
                "DG002 - Trần Thị B (Hoạt động)",
                "DG003 - Lê Hoàng C (Đang khóa thẻ)"
            });
            cboDocGia.SelectedIndex = 0;

            // Danh sách sách còn trong kho
            clbSachMuon.Items.AddRange(new object[] {
                "S01 - Clean Code - Mã Sạch (Còn: 15)",
                "S02 - Giáo trình Lập trình C# .NET (Còn: 42)",
                "S04 - Thiết kế Cơ sở dữ liệu SQL Server (Còn: 8)",
                "S05 - Kiến trúc Phần mềm Hiện đại (Còn: 5)"
            });

            // Dữ liệu mẫu Bảng Master (PhieuMuon)
            dgvPhieuMuon.Rows.Add("PM-001", "Nguyễn Văn A", "thuthu01", "01/10/2026", "15/10/2026", "Quá hạn");
            dgvPhieuMuon.Rows.Add("PM-002", "Trần Thị B", "admin", "12/10/2026", "26/10/2026", "Đang mượn");

            // Dữ liệu mẫu Bảng Detail (ChiTiet_PhieuMuon)
            mockChiTietData["PM-001"] = new List<ChiTietItem>
            {
                new ChiTietItem { MaSach = "S01", TenSach = "Clean Code - Mã Sạch", SoLuong = 1, NgayTra = "Chưa trả", TienPhat = 0, TinhTrang = "Đang mượn" },
                new ChiTietItem { MaSach = "S02", TenSach = "Giáo trình Lập trình C# .NET", SoLuong = 1, NgayTra = "14/10/2026", TienPhat = 0, TinhTrang = "Nguyên vẹn (Tốt)" }
            };

            mockChiTietData["PM-002"] = new List<ChiTietItem>
            {
                new ChiTietItem { MaSach = "S04", TenSach = "Thiết kế Cơ sở dữ liệu SQL Server", SoLuong = 1, NgayTra = "Chưa trả", TienPhat = 0, TinhTrang = "Đang mượn" }
            };

            if (dgvPhieuMuon.Rows.Count > 0)
                LoadChiTietByMaPhieu("PM-001");
        }

        private void DgvPhieuMuon_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPhieuMuon.CurrentRow == null) return;
            string maPhieu = dgvPhieuMuon.CurrentRow.Cells["MaPhieu"].Value?.ToString();
            if (!string.IsNullOrEmpty(maPhieu))
                LoadChiTietByMaPhieu(maPhieu);
        }

        private void LoadChiTietByMaPhieu(string maPhieu)
        {
            dgvChiTiet.Rows.Clear();
            if (!mockChiTietData.ContainsKey(maPhieu)) return;

            foreach (var item in mockChiTietData[maPhieu])
            {
                dgvChiTiet.Rows.Add(item.MaSach, item.TenSach, item.SoLuong, item.NgayTra, item.TienPhat.ToString("N0"), item.TinhTrang);
            }
            lblSachDangChon.Text = "Ấn phẩm: (Chọn 1 dòng bên trái)";
            numTienPhat.Value = 0;
        }

        private void DgvChiTiet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvChiTiet.CurrentRow == null) return;
            string tenSach = dgvChiTiet.CurrentRow.Cells["TenSach"].Value?.ToString();
            string ngayTra = dgvChiTiet.CurrentRow.Cells["NgayTra"].Value?.ToString();

            lblSachDangChon.Text = $"Ấn phẩm: {tenSach}";
            btnXacNhanTra.Enabled = (ngayTra == "Chưa trả");

            TinhTienPhatTuDong();
        }

        // Quy tắc nghiệp vụ: Tự động tính phạt theo số ngày quá hạn (5.000đ/ngày) + phí hư hỏng
        private void TinhTienPhatTuDong()
        {
            if (dgvPhieuMuon.CurrentRow == null || dgvChiTiet.CurrentRow == null) return;

            string hanTraStr = dgvPhieuMuon.CurrentRow.Cells["HanTra"].Value?.ToString();
            decimal phatQuaHan = 0;

            if (DateTime.TryParseExact(hanTraStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime hanTra))
            {
                int soNgayTre = (dtpNgayTra.Value.Date - hanTra.Date).Days;
                if (soNgayTre > 0)
                    phatQuaHan = soNgayTre * 5000; // Cấu hình mẫu: 5.000 VNĐ / ngày trễ
            }

            decimal phatHuHong = 0;
            if (cboTinhTrangTra.SelectedIndex == 1) phatHuHong = 20000;
            else if (cboTinhTrangTra.SelectedIndex == 2) phatHuHong = 100000;

            numTienPhat.Value = phatQuaHan + phatHuHong;
        }

        private void BtnLapPhieu_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra độc giả có đang hoạt động không
            string docGiaInfo = cboDocGia.SelectedItem?.ToString() ?? "";
            if (docGiaInfo.Contains("Đang khóa thẻ"))
            {
                MessageBox.Show("Độc giả này đang bị khóa thẻ! Chỉ độc giả trạng thái Hoạt động mới được mượn sách.",
                                "Từ chối lập phiếu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2. Kiểm tra Hạn trả không được trước Ngày mượn
            if (dtpHanTra.Value.Date < dtpNgayMuon.Value.Date)
            {
                MessageBox.Show("Ngày hẹn trả không được nhỏ hơn ngày mượn sách!",
                                "Lỗi ngày tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Kiểm tra đã chọn sách chưa
            if (clbSachMuon.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng tích chọn ít nhất 1 ấn phẩm để mượn!",
                                "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tạo mã phiếu mới
            string newMaPhieu = $"PM-00{dgvPhieuMuon.Rows.Count + 1}";
            string tenDocGia = docGiaInfo.Split('-')[1].Replace("(Hoạt động)", "").Trim();

            var chiTietList = new List<ChiTietItem>();
            foreach (var item in clbSachMuon.CheckedItems)
            {
                string[] parts = item.ToString().Split('-');
                string maSach = parts[0].Trim();
                string tenSach = parts[1].Trim();

                chiTietList.Add(new ChiTietItem
                {
                    MaSach = maSach,
                    TenSach = tenSach,
                    SoLuong = 1,
                    NgayTra = "Chưa trả",
                    TienPhat = 0,
                    TinhTrang = "Đang mượn"
                });
            }

            mockChiTietData[newMaPhieu] = chiTietList;

            dgvPhieuMuon.Rows.Add(
                newMaPhieu,
                tenDocGia,
                "admin",
                dtpNgayMuon.Value.ToString("dd/MM/yyyy"),
                dtpHanTra.Value.ToString("dd/MM/yyyy"),
                "Đang mượn"
            );

            // Bỏ tích chọn sau khi lập xong
            for (int i = 0; i < clbSachMuon.Items.Count; i++)
                clbSachMuon.SetItemChecked(i, false);

            MessageBox.Show($"Đã lập phiếu mượn [{newMaPhieu}] thành công và trừ tồn kho tương ứng!",
                            "Ghi sổ thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnXacNhanTra_Click(object sender, EventArgs e)
        {
            if (dgvPhieuMuon.CurrentRow == null || dgvChiTiet.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách trong bảng Chi tiết bên trái để xác nhận trả!",
                                "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string maPhieu = dgvPhieuMuon.CurrentRow.Cells["MaPhieu"].Value.ToString();
            int detailIndex = dgvChiTiet.CurrentRow.Index;
            var item = mockChiTietData[maPhieu][detailIndex];

            if (item.NgayTra != "Chưa trả")
            {
                MessageBox.Show("Ấn phẩm này đã được trả trước đó rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Cập nhật chi tiết trả sách
            item.NgayTra = dtpNgayTra.Value.ToString("dd/MM/yyyy");
            item.TienPhat = numTienPhat.Value;
            item.TinhTrang = cboTinhTrangTra.SelectedItem.ToString();

            LoadChiTietByMaPhieu(maPhieu);

            // Quy tắc nghiệp vụ: Một phiếu chỉ hoàn tất khi MỌI chi tiết đều đã trả
            bool tatCaDaTra = true;
            foreach (var ct in mockChiTietData[maPhieu])
            {
                if (ct.NgayTra == "Chưa trả")
                {
                    tatCaDaTra = false;
                    break;
                }
            }

            if (tatCaDaTra)
            {
                dgvPhieuMuon.CurrentRow.Cells["TrangThai"].Value = "Đã hoàn tất";
                MessageBox.Show($"Đã nhận trả sách! Toàn bộ sách trong phiếu [{maPhieu}] đã được trả đủ -> Phiếu chuyển sang [Đã hoàn tất].",
                                "Hoàn tất phiếu mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Đã ghi nhận trả cuốn sách này và cộng lại số lượng tồn kho!",
                                "Trả sách thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FilterMasterGrid()
        {
            string kw = txtTimPhieu.Text.Trim().ToLower();
            string status = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem.ToString() : "";

            foreach (DataGridViewRow row in dgvPhieuMuon.Rows)
            {
                string ma = row.Cells["MaPhieu"].Value?.ToString().ToLower() ?? "";
                string dg = row.Cells["DocGia"].Value?.ToString().ToLower() ?? "";
                string tt = row.Cells["TrangThai"].Value?.ToString() ?? "";

                bool matchKw = string.IsNullOrEmpty(kw) || ma.Contains(kw) || dg.Contains(kw);
                bool matchSt = string.IsNullOrEmpty(status) || tt == status;

                row.Visible = matchKw && matchSt;
            }
        }
    }
}