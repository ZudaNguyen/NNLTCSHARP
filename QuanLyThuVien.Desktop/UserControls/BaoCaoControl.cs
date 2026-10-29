using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class BaoCaoControl : UserControl
    {
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        private ComboBox cboLoaiBaoCao;
        private DateTimePicker dtpTuNgay;
        private DateTimePicker dtpDenNgay;
        private DataGridView dgvBaoCao;
        private Label lblTongKet;

        public BaoCaoControl()
        {
            InitializeComponent();
            SetupBaoCaoUI();
            LoadReportData(0);
            UI.ScrollableView.Configure(this, new Size(1000, 500));
        }

        private void SetupBaoCaoUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(20);

            // ==========================================
            // 1. THANH CÔNG CỤ LỌC & XUẤT BÁO CÁO (TOP PANEL)
            // ==========================================
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 135,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            Label lblTitle = new Label
            {
                Text = "📊 Trích Xuất Sổ Báo Cáo & Thống Kê Nghiệp Vụ",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 12),
                AutoSize = true
            };
            pnlFilter.Controls.Add(lblTitle);

            Label lblLoai = new Label
            {
                Text = "Loại báo cáo:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(15, 55),
                AutoSize = true
            };

            cboLoaiBaoCao = new ComboBox
            {
                Location = new Point(110, 52),
                Width = 260,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLoaiBaoCao.Items.AddRange(new object[] {
                "1. Danh sách Sách đang mượn & Quá hạn",
                "2. Thống kê Sách mượn nhiều nhất (Top Ấn phẩm)",
                "3. Báo cáo Thu tiền phạt quá hạn / hư hỏng"
            });
            cboLoaiBaoCao.SelectedIndex = 0;
            cboLoaiBaoCao.SelectedIndexChanged += (s, e) => { if (KiemTraKhoangNgay()) LoadReportData(cboLoaiBaoCao.SelectedIndex); };

            Label lblTu = new Label
            {
                Text = "Từ ngày:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(390, 55),
                AutoSize = true
            };

            dtpTuNgay = new DateTimePicker
            {
                Location = new Point(455, 52),
                Width = 115,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 10, 1)
            };

            Label lblDen = new Label
            {
                Text = "Đến ngày:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(585, 55),
                AutoSize = true
            };

            dtpDenNgay = new DateTimePicker
            {
                Location = new Point(660, 52),
                Width = 115,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            Button btnLoc = new Button
            {
                Text = "🔍 Lọc Dữ Liệu",
                Location = new Point(110, 90), // Hàng thứ 2 để không bị tràn khỏi panel khi cửa sổ hẹp
                Size = new Size(130, 34),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(180, 120, 20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLoc.FlatAppearance.BorderSize = 0;
            btnLoc.Click += (s, e) => { if (KiemTraKhoangNgay()) LoadReportData(cboLoaiBaoCao.SelectedIndex); };

            Button btnXuatExcel = new Button
            {
                Text = "📥 Xuất Báo Cáo (.CSV)",
                Location = new Point(255, 90),
                Size = new Size(185, 34),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnXuatExcel.FlatAppearance.BorderSize = 0;
            btnXuatExcel.Click += BtnXuatExcel_Click;

            pnlFilter.Controls.AddRange(new Control[] {
                lblLoai, cboLoaiBaoCao, lblTu, dtpTuNgay, lblDen, dtpDenNgay, btnLoc, btnXuatExcel
            });

            // ==========================================
            // 2. THANH TỔNG KẾT SỐ LIỆU (BOTTOM BAR)
            // ==========================================
            Panel pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(238, 230, 212),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 0)
            };

            lblTongKet = new Label
            {
                Text = "✒️ Tổng kết: Đang hiển thị 0 dòng dữ liệu.",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Burgundy,
                AutoSize = false,
                Dock = DockStyle.Fill
            };
            pnlFooter.Controls.Add(lblTongKet);

            // ==========================================
            // 3. BẢNG HIỂN THỊ BÁO CÁO (MIDDLE FILL)
            // ==========================================
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12)
            };

            dgvBaoCao = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgvBaoCao.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvBaoCao.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvBaoCao.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            dgvBaoCao.DefaultCellStyle.BackColor = CardBg;
            dgvBaoCao.DefaultCellStyle.ForeColor = WoodDark;
            dgvBaoCao.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgvBaoCao.DefaultCellStyle.SelectionForeColor = WoodDark;

            pnlGrid.Controls.Add(dgvBaoCao);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlFilter);
        }

        // Từ ngày không được lớn hơn Đến ngày
        private bool KiemTraKhoangNgay()
        {
            if (dtpTuNgay.Value.Date > dtpDenNgay.Value.Date)
            {
                MessageBox.Show("\"Từ ngày\" không được lớn hơn \"Đến ngày\"!", "Khoảng ngày không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTuNgay.Focus();
                return false;
            }
            return true;
        }

        private bool TrongKhoang(DateTime d) => d.Date >= dtpTuNgay.Value.Date && d.Date <= dtpDenNgay.Value.Date;

        private void LoadReportData(int reportType)
        {
            dgvBaoCao.Columns.Clear();
            dgvBaoCao.Rows.Clear();

            // TODO(API): thay dữ liệu mẫu dưới đây bằng dữ liệu từ API, truyền Từ ngày/Đến ngày lên server để lọc.
            if (reportType == 0)
            {
                // Báo cáo 1: Sách đang mượn & Quá hạn (lọc theo Ngày mượn)
                dgvBaoCao.Columns.Add("MaPhieu", "Mã Phiếu");
                dgvBaoCao.Columns.Add("DocGia", "Độc Giả");
                dgvBaoCao.Columns.Add("SDT", "Điện Thoại");
                dgvBaoCao.Columns.Add("TenSach", "Tên Ấn Phẩm");
                dgvBaoCao.Columns.Add("NgayMuon", "Ngày Mượn");
                dgvBaoCao.Columns.Add("HanTra", "Hạn Trả");
                dgvBaoCao.Columns.Add("TinhTrang", "Tình Trạng");

                var mau = new (string Ma, string DG, string Sdt, string Sach, DateTime Muon, DateTime Han)[]
                {
                    ("PM-001", "Nguyễn Văn A", "0901234567", "Clean Code - Mã Sạch", new DateTime(2026, 10, 1), new DateTime(2026, 10, 15)),
                    ("PM-002", "Trần Thị B", "0912345678", "Thiết kế CSDL SQL Server", new DateTime(2026, 10, 12), new DateTime(2026, 10, 26)),
                    ("PM-003", "Lê Hoàng C", "0987654321", "Nhà Giả Kim", new DateTime(2026, 10, 5), new DateTime(2026, 10, 19))
                };

                int tong = 0, quaHan = 0;
                foreach (var m in mau)
                {
                    if (!TrongKhoang(m.Muon)) continue;
                    int ngayTre = (DateTime.Today - m.Han.Date).Days; // tính từ hạn trả, không ghi cứng
                    string tinhTrang = ngayTre > 0 ? $"Quá hạn ({ngayTre} ngày)" : "Đang mượn";
                    dgvBaoCao.Rows.Add(m.Ma, m.DG, m.Sdt, m.Sach, m.Muon.ToString("dd/MM/yyyy"), m.Han.ToString("dd/MM/yyyy"), tinhTrang);
                    tong++;
                    if (ngayTre > 0) quaHan++;
                }

                lblTongKet.Text = $"✒️ Tổng kết: Có {tong} ấn phẩm đang lưu hành bên ngoài (Trong đó có {quaHan} ấn phẩm đã quá hạn cần thu hồi).";
            }
            else if (reportType == 1)
            {
                // Báo cáo 2: Top Sách mượn nhiều (dữ liệu mẫu, chưa lọc theo ngày)
                dgvBaoCao.Columns.Add("Hang", "Thứ Hạng");
                dgvBaoCao.Columns.Add("MaSach", "Mã Sách");
                dgvBaoCao.Columns.Add("TenSach", "Tên Ấn Phẩm");
                dgvBaoCao.Columns.Add("TheLoai", "Thể Loại");
                dgvBaoCao.Columns.Add("SoLuotMuon", "Tổng Lượt Mượn");

                dgvBaoCao.Rows.Add("Top 1", "S02", "Giáo trình Lập trình C# .NET", "Công nghệ thông tin", "48 lượt");
                dgvBaoCao.Rows.Add("Top 2", "S01", "Clean Code - Mã Sạch", "Công nghệ thông tin", "35 lượt");
                dgvBaoCao.Rows.Add("Top 3", "S03", "Nhà Giả Kim", "Văn học kinh điển", "29 lượt");

                lblTongKet.Text = "✒️ Tổng kết: Nhóm sách [Công nghệ thông tin] chiếm tỷ trọng mượn đọc cao nhất (dữ liệu mẫu, chưa lọc theo ngày).";
            }
            else
            {
                // Báo cáo 3: Tiền phạt (lọc theo Ngày trả)
                dgvBaoCao.Columns.Add("MaPhieu", "Mã Phiếu");
                dgvBaoCao.Columns.Add("DocGia", "Độc Giả Nộp Phạt");
                dgvBaoCao.Columns.Add("TenSach", "Ấn Phẩm Vi Phạm");
                dgvBaoCao.Columns.Add("NgayTra", "Ngày Trả");
                dgvBaoCao.Columns.Add("LyDo", "Lý Do Phạt");
                dgvBaoCao.Columns.Add("SoTien", "Số Tiền Phạt (VNĐ)");

                var mau = new (string Ma, string DG, string Sach, DateTime Tra, string LyDo, decimal Tien)[]
                {
                    ("PM-000", "Phạm Minh D", "Đắc Nhân Tâm", new DateTime(2026, 10, 10), "Trễ hạn 4 ngày", 20000m),
                    ("PM-004", "Hoàng Thị E", "Cấu trúc dữ liệu & Giải thuật", new DateTime(2026, 10, 18), "Rách bìa nhẹ + Trễ 2 ngày", 30000m)
                };

                decimal tongTien = 0;
                foreach (var m in mau)
                {
                    if (!TrongKhoang(m.Tra)) continue;
                    dgvBaoCao.Rows.Add(m.Ma, m.DG, m.Sach, m.Tra.ToString("dd/MM/yyyy"), m.LyDo, m.Tien.ToString("N0"));
                    tongTien += m.Tien;
                }

                lblTongKet.Text = $"✒️ Tổng kết: Tổng doanh thu tiền phạt vi phạm trong kỳ báo cáo là {tongTien:N0} VNĐ.";
            }
        }

        // Chống CSV/Formula injection: ô bắt đầu bằng = + - @ (hoặc tab/CR) sẽ bị Excel chạy như công thức.
        // Dữ liệu như tên độc giả có thể do người dùng Web nhập, nên phải vô hiệu hóa bằng dấu nháy đơn.
        private static string CsvCell(string? value)
        {
            string v = value ?? "";
            if (v.Length > 0 && "=+-@\t\r".IndexOf(v[0]) >= 0)
                v = "'" + v;
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        }

        // Chức năng mở rộng ăn điểm cộng: Xuất trực tiếp DataGridView ra file CSV (Đọc bằng Excel có tiếng Việt UTF-8)
        private void BtnXuatExcel_Click(object sender, EventArgs e)
        {
            if (dgvBaoCao.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất báo cáo!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Excel File (*.csv)|*.csv";
                sfd.FileName = $"BaoCaoThuVien_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();

                        // Dòng "sep=," để Excel luôn tách cột bằng dấu phẩy, bất kể cài đặt vùng (vi-VN thường dùng dấu ;)
                        sb.AppendLine("sep=,");

                        // Ghi dòng tiêu đề cột
                        for (int i = 0; i < dgvBaoCao.Columns.Count; i++)
                        {
                            sb.Append(CsvCell(dgvBaoCao.Columns[i].HeaderText));
                            if (i < dgvBaoCao.Columns.Count - 1) sb.Append(",");
                        }
                        sb.AppendLine();

                        // Ghi từng dòng dữ liệu
                        foreach (DataGridViewRow row in dgvBaoCao.Rows)
                        {
                            for (int i = 0; i < dgvBaoCao.Columns.Count; i++)
                            {
                                sb.Append(CsvCell(row.Cells[i].Value?.ToString()));
                                if (i < dgvBaoCao.Columns.Count - 1) sb.Append(",");
                            }
                            sb.AppendLine();
                        }

                        // Lưu với UTF8-BOM để mở bằng Microsoft Excel không bị lỗi font tiếng Việt
                        File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));

                        MessageBox.Show($"Đã trích xuất báo cáo thành công tại:\n{sfd.FileName}",
                                        "Xuất Báo Cáo Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
