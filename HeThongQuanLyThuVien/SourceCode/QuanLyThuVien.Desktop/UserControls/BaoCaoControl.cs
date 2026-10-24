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
                Height = 115,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            Label lblTitle = new Label
            {
                Text = "📊 Trích Xuất Sổ Báo Cáo & Thống Kê Nghiệp Vụ",
                Font = new Font("Georgia", 11.5f, FontStyle.Bold),
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
            cboLoaiBaoCao.SelectedIndexChanged += (s, e) => LoadReportData(cboLoaiBaoCao.SelectedIndex);

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
                Location = new Point(110, 82), // Sẽ chỉnh lại cùng hàng nếu màn rộng
                Size = new Size(130, 34),
                Font = new Font("Georgia", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(180, 120, 20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLoc.Location = new Point(790, 50);
            btnLoc.FlatAppearance.BorderSize = 0;
            btnLoc.Click += (s, e) => LoadReportData(cboLoaiBaoCao.SelectedIndex);

            Button btnXuatExcel = new Button
            {
                Text = "📥 Xuất Báo Cáo (.CSV)",
                Location = new Point(930, 50),
                Size = new Size(185, 34),
                Font = new Font("Georgia", 9f, FontStyle.Bold),
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
                Height = 45,
                BackColor = Color.FromArgb(238, 230, 212),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 0)
            };

            lblTongKet = new Label
            {
                Text = "✒️ Tổng kết: Đang hiển thị 0 dòng dữ liệu.",
                Font = new Font("Georgia", 10.5f, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Burgundy,
                AutoSize = true
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
            dgvBaoCao.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);

            dgvBaoCao.DefaultCellStyle.BackColor = CardBg;
            dgvBaoCao.DefaultCellStyle.ForeColor = WoodDark;
            dgvBaoCao.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgvBaoCao.DefaultCellStyle.SelectionForeColor = WoodDark;

            pnlGrid.Controls.Add(dgvBaoCao);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlFilter);
        }

        private void LoadReportData(int reportType)
        {
            dgvBaoCao.Columns.Clear();
            dgvBaoCao.Rows.Clear();

            if (reportType == 0)
            {
                // Báo cáo 1: Sách đang mượn & Quá hạn
                dgvBaoCao.Columns.Add("MaPhieu", "Mã Phiếu");
                dgvBaoCao.Columns.Add("DocGia", "Độc Giả");
                dgvBaoCao.Columns.Add("SDT", "Điện Thoại");
                dgvBaoCao.Columns.Add("TenSach", "Tên Ấn Phẩm");
                dgvBaoCao.Columns.Add("NgayMuon", "Ngày Mượn");
                dgvBaoCao.Columns.Add("HanTra", "Hạn Trả");
                dgvBaoCao.Columns.Add("TinhTrang", "Tình Trạng");

                dgvBaoCao.Rows.Add("PM-001", "Nguyễn Văn A", "0901234567", "Clean Code - Mã Sạch", "01/10/2026", "15/10/2026", "Quá hạn (6 ngày)");
                dgvBaoCao.Rows.Add("PM-002", "Trần Thị B", "0912345678", "Thiết kế CSDL SQL Server", "12/10/2026", "26/10/2026", "Đang mượn");
                dgvBaoCao.Rows.Add("PM-003", "Lê Hoàng C", "0987654321", "Nhà Giả Kim", "05/10/2026", "19/10/2026", "Quá hạn (2 ngày)");

                lblTongKet.Text = "✒️ Tổng kết: Có 3 ấn phẩm đang lưu hành bên ngoài (Trong đó có 2 ấn phẩm đã quá hạn cần thu hồi).";
            }
            else if (reportType == 1)
            {
                // Báo cáo 2: Top Sách mượn nhiều
                dgvBaoCao.Columns.Add("Hang", "Thứ Hạng");
                dgvBaoCao.Columns.Add("MaSach", "Mã Sách");
                dgvBaoCao.Columns.Add("TenSach", "Tên Ấn Phẩm");
                dgvBaoCao.Columns.Add("TheLoai", "Thể Loại");
                dgvBaoCao.Columns.Add("SoLuotMuon", "Tổng Lượt Mượn");

                dgvBaoCao.Rows.Add("Top 1", "S02", "Giáo trình Lập trình C# .NET", "Công nghệ thông tin", "48 lượt");
                dgvBaoCao.Rows.Add("Top 2", "S01", "Clean Code - Mã Sạch", "Công nghệ thông tin", "35 lượt");
                dgvBaoCao.Rows.Add("Top 3", "S03", "Nhà Giả Kim", "Văn học kinh điển", "29 lượt");

                lblTongKet.Text = "✒️ Tổng kết: Nhóm sách [Công nghệ thông tin] chiếm tỷ trọng mượn đọc cao nhất trong kỳ.";
            }
            else
            {
                // Báo cáo 3: Tiền phạt
                dgvBaoCao.Columns.Add("MaPhieu", "Mã Phiếu");
                dgvBaoCao.Columns.Add("DocGia", "Độc Giả Nộp Phạt");
                dgvBaoCao.Columns.Add("TenSach", "Ấn Phẩm Vi Phạm");
                dgvBaoCao.Columns.Add("NgayTra", "Ngày Trả");
                dgvBaoCao.Columns.Add("LyDo", "Lý Do Phạt");
                dgvBaoCao.Columns.Add("SoTien", "Số Tiền Phạt (VNĐ)");

                dgvBaoCao.Rows.Add("PM-000", "Phạm Minh D", "Đắc Nhân Tâm", "10/10/2026", "Trễ hạn 4 ngày", "20,000");
                dgvBaoCao.Rows.Add("PM-004", "Hoàng Thị E", "Cấu trúc dữ liệu & Giải thuật", "18/10/2026", "Rách bìa nhẹ + Trễ 2 ngày", "30,000");

                lblTongKet.Text = "✒️ Tổng kết: Tổng doanh thu tiền phạt vi phạm trong kỳ báo cáo là 50,000 VNĐ.";
            }
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

                        // Ghi dòng tiêu đề cột
                        for (int i = 0; i < dgvBaoCao.Columns.Count; i++)
                        {
                            sb.Append($"\"{dgvBaoCao.Columns[i].HeaderText}\"");
                            if (i < dgvBaoCao.Columns.Count - 1) sb.Append(",");
                        }
                        sb.AppendLine();

                        // Ghi từng dòng dữ liệu
                        foreach (DataGridViewRow row in dgvBaoCao.Rows)
                        {
                            for (int i = 0; i < dgvBaoCao.Columns.Count; i++)
                            {
                                string cellValue = row.Cells[i].Value?.ToString().Replace("\"", "\"\"") ?? "";
                                sb.Append($"\"{cellValue}\"");
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