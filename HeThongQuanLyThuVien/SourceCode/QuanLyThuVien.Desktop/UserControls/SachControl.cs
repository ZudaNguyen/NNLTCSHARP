using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class SachControl : UserControl
    {
        // Bảng màu Thư viện cổ điển
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color GoldBorder = Color.FromArgb(205, 190, 165);

        // Các Control nhập liệu (Bám sát bảng Sach & Sach_TacGia)
        private TextBox txtMaSach;
        private TextBox txtTenSach;
        private TextBox txtISBN;
        private ComboBox cboTheLoai;
        private CheckedListBox clbTacGia;
        private NumericUpDown numNamXB;
        private TextBox txtNhaXB;
        private NumericUpDown numSoLuong;
        private NumericUpDown numSoLuongCon;
        private TextBox txtMoTa;
        private PictureBox picBiaSach;
        private string currentImagePath = "";

        // Các Control Tìm kiếm & Danh sách
        private TextBox txtTimKiem;
        private ComboBox cboLocTheLoai;
        private ComboBox cboTrangThaiSach;
        private DataGridView dgvSach;
        private Button? btnXoaSach;
        private readonly SachApiClient apiClient = new();
        private CancellationTokenSource? searchCancellation;
        private bool loadingLookups;
        private bool suppressFilterReload;

        public SachControl()
        {
            InitializeComponent();
            SetupSachUI();
            _ = LoadInitialDataAsync();
        }

        private void SetupSachUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(20);

            // ==========================================
            // 1. KHU VỰC NHẬP LIỆU CHI TIẾT SÁCH (TOP PANEL)
            // ==========================================
            Panel pnlForm = new Panel
            {
                Dock = DockStyle.Top,
                Height = 290,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                AutoScrollMinSize = new Size(820, 0)
            };

            Label lblFormTitle = new Label
            {
                Text = "✒️ Thông tin Ấn phẩm & Biên mục Sách",
                Font = new Font("Georgia", 11.5f, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlForm.Controls.Add(lblFormTitle);

            // --- CỘT 1: Mã sách, Tên sách, ISBN, Thể loại ---
            AddLabel(pnlForm, "Mã sách:", 15, 42);
            txtMaSach = AddTextBox(pnlForm, 105, 39, 90);
            txtMaSach.ReadOnly = true;
            txtMaSach.BackColor = Color.FromArgb(235, 230, 220);
            txtMaSach.PlaceholderText = "Tự động";

            AddLabel(pnlForm, "ISBN:", 205, 42);
            txtISBN = AddTextBox(pnlForm, 270, 39, 130);

            AddLabel(pnlForm, "Tên sách:", 15, 80);
            txtTenSach = AddTextBox(pnlForm, 105, 77, 295);

            AddLabel(pnlForm, "Thể loại:", 15, 118);
            cboTheLoai = new ComboBox
            {
                Location = new Point(105, 115),
                Width = 295,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            pnlForm.Controls.Add(cboTheLoai);

            AddLabel(pnlForm, "Nhà XB:", 15, 156);
            txtNhaXB = AddTextBox(pnlForm, 105, 153, 165);

            AddLabel(pnlForm, "Năm XB:", 280, 156);
            numNamXB = new NumericUpDown
            {
                Location = new Point(340, 153),
                Width = 60,
                Font = new Font("Segoe UI", 10),
                Minimum = 1900,
                Maximum = DateTime.Now.Year + 1,
                Value = 2026
            };
            pnlForm.Controls.Add(numNamXB);

            AddLabel(pnlForm, "Tổng SL:", 15, 194);
            numSoLuong = new NumericUpDown
            {
                Location = new Point(105, 191),
                Width = 85,
                Font = new Font("Segoe UI", 10),
                Minimum = 0,
                Maximum = 10000,
                Value = 10
            };
            // Khi đổi tổng số lượng lúc thêm mới, tự cập nhật số lượng còn
            numSoLuong.ValueChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtMaSach.Text))
                    numSoLuongCon.Value = numSoLuong.Value;
            };
            pnlForm.Controls.Add(numSoLuong);

            AddLabel(pnlForm, "SL Còn:", 210, 194);
            numSoLuongCon = new NumericUpDown
            {
                Location = new Point(275, 191),
                Width = 125,
                Font = new Font("Segoe UI", 10),
                Minimum = 0,
                Maximum = 10000,
                Value = 10,
                Enabled = false // Khóa cứng theo quy tắc nghiệp vụ, chỉ thay đổi qua Mượn/Trả
            };
            pnlForm.Controls.Add(numSoLuongCon);

            // --- CỘT 2: Chọn nhiều Tác giả (Bảng Sach_TacGia) & Mô tả ---
            AddLabel(pnlForm, "Tác giả (Chọn 1 hoặc nhiều):", 425, 42);
            clbTacGia = new CheckedListBox
            {
                Location = new Point(425, 65),
                Size = new Size(230, 95),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };
            pnlForm.Controls.Add(clbTacGia);

            AddLabel(pnlForm, "Mô tả chi tiết:", 425, 168);
            txtMoTa = new TextBox
            {
                Location = new Point(425, 191),
                Size = new Size(230, 42),
                Font = new Font("Segoe UI", 9.5f),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlForm.Controls.Add(txtMoTa);

            // --- CỘT 3: Ảnh bìa sách (HinhAnh) ---
            AddLabel(pnlForm, "Ảnh bìa:", 680, 42);
            picBiaSach = new PictureBox
            {
                Location = new Point(680, 65),
                Size = new Size(115, 130),
                BackColor = Color.FromArgb(238, 230, 212),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pnlForm.Controls.Add(picBiaSach);

            Button btnChonAnh = new Button
            {
                Text = "🖼️ Chọn bìa...",
                Location = new Point(680, 200),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(238, 230, 212),
                ForeColor = WoodDark,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChonAnh.FlatAppearance.BorderColor = GoldBorder;
            btnChonAnh.Click += BtnChonAnh_Click;
            pnlForm.Controls.Add(btnChonAnh);

            // --- THANH NÚT HÀNH ĐỘNG (CRUD BUTTONS) ---
            FlowLayoutPanel flpButtons = new FlowLayoutPanel
            {
                Location = new Point(15, 240),
                Size = new Size(780, 40),
                WrapContents = false
            };

            Button btnThem = CreateActionButton("➕ Ghi Sổ Mới (Thêm)", Color.FromArgb(45, 106, 79));
            Button btnSua = CreateActionButton("✏️ Tu Chỉnh (Sửa)", Color.FromArgb(180, 120, 20));
            btnXoaSach = CreateActionButton("🗃️ Lưu Kho Ẩn (Xóa mềm)", Burgundy);
            Button btnLamMoi = CreateActionButton("🔄 Làm Mới Form", Color.FromArgb(90, 75, 65));

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoaSach.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            flpButtons.Controls.Add(btnThem);
            flpButtons.Controls.Add(btnSua);
            flpButtons.Controls.Add(btnXoaSach);
            flpButtons.Controls.Add(btnLamMoi);
            pnlForm.Controls.Add(flpButtons);

            // ==========================================
            // 2. THANH TÌM KIẾM & BỘ LỌC (MIDDLE PANEL)
            // ==========================================
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = PaperCream,
                Padding = new Padding(0, 10, 0, 5)
            };

            Label lblTim = new Label
            {
                Text = "🔍 Tra cứu (Tên / ISBN / Tác giả):",
                Font = new Font("Georgia", 10, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(5, 18),
                AutoSize = true
            };

            txtTimKiem = new TextBox
            {
                Location = new Point(245, 15),
                Width = 220,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            txtTimKiem.TextChanged += FilterChanged;

            Label lblLocTL = new Label
            {
                Text = "Thể loại:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(480, 18),
                AutoSize = true
            };

            cboLocTheLoai = new ComboBox
            {
                Location = new Point(545, 15),
                Width = 170,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLocTheLoai.SelectedIndexChanged += FilterChanged;

            Label lblTrangThaiSach = new Label
            {
                Text = "Trạng thái:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                Location = new Point(5, 57),
                AutoSize = true
            };
            cboTrangThaiSach = new ComboBox
            {
                Location = new Point(85, 52),
                Width = 200,
                Font = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTrangThaiSach.Items.AddRange(["Đang hoạt động", "Đã ẩn", "Tất cả"]);
            cboTrangThaiSach.SelectedIndexChanged += FilterChanged;

            pnlFilter.Controls.Add(cboTrangThaiSach);
            pnlFilter.Controls.Add(lblTrangThaiSach);
            pnlFilter.Controls.Add(cboLocTheLoai);
            pnlFilter.Controls.Add(lblLocTL);
            pnlFilter.Controls.Add(txtTimKiem);
            pnlFilter.Controls.Add(lblTim);

            // ==========================================
            // 3. BẢNG DANH SÁCH ẤN PHẨM (DATAGRIDVIEW)
            // ==========================================
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            dgvSach = new DataGridView
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
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgvSach.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgvSach.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgvSach.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
            QuanLyThuVien.Desktop.Services.DataGridViewStyleHelper.ApplyLibraryStyle(dgvSach);

            dgvSach.Columns.Add("MaSach", "Mã Sách");
            dgvSach.Columns["MaSach"].FillWeight = 45;
            dgvSach.Columns.Add("ISBN", "Mã ISBN");
            dgvSach.Columns["ISBN"].FillWeight = 75;
            dgvSach.Columns.Add("TenSach", "Tên Ấn Phẩm");
            dgvSach.Columns["TenSach"].FillWeight = 150;
            dgvSach.Columns.Add("TheLoai", "Thể Loại");
            dgvSach.Columns.Add("TacGia", "Tác Giả");
            dgvSach.Columns.Add("NamXB", "Năm XB");
            dgvSach.Columns["NamXB"].FillWeight = 50;
            dgvSach.Columns.Add("NhaXB", "Nhà Xuất Bản");
            dgvSach.Columns.Add("SoLuong", "Tổng SL");
            dgvSach.Columns["SoLuong"].FillWeight = 50;
            dgvSach.Columns.Add("SoLuongCon", "SL Còn");
            dgvSach.Columns["SoLuongCon"].FillWeight = 50;
            dgvSach.Columns.Add("TrangThai", "Trạng Thái");
            dgvSach.Columns["TrangThai"].FillWeight = 70;

            dgvSach.CellClick += DgvSach_CellClick;

            pnlGrid.Controls.Add(dgvSach);

            var pageLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = PaperCream, Margin = Padding.Empty, Padding = Padding.Empty
            };
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 290));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnlForm.Dock = DockStyle.Fill;
            pnlFilter.Dock = DockStyle.Fill;
            pnlGrid.Dock = DockStyle.Fill;
            pageLayout.Controls.Add(pnlForm, 0, 0);
            pageLayout.Controls.Add(pnlFilter, 0, 1);
            pageLayout.Controls.Add(pnlGrid, 0, 2);
            this.Controls.Add(pageLayout);
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ TẠO UI & VALIDATION NGHIỆP VỤ
        // ==========================================
        private void AddLabel(Control parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = WoodDark,
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private TextBox AddTextBox(Control parent, int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateActionButton(string text, Color bgColor)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(180, 35),
                Margin = new Padding(0, 0, 12, 0),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = bgColor,
                ForeColor = Color.FromArgb(255, 248, 231),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async Task LoadInitialDataAsync()
        {
            loadingLookups = true;
            try
            {
                var lookups = await apiClient.GetLookupsAsync(CancellationToken.None);

                cboTheLoai.DisplayMember = nameof(TheLoaiOptionDto.TenTheLoai);
                cboTheLoai.Items.Clear();
                foreach (var category in lookups.TheLoai)
                    cboTheLoai.Items.Add(category);
                if (cboTheLoai.Items.Count > 0)
                    cboTheLoai.SelectedIndex = 0;

                cboLocTheLoai.DisplayMember = nameof(TheLoaiOptionDto.TenTheLoai);
                cboLocTheLoai.Items.Clear();
                cboLocTheLoai.Items.Add(new TheLoaiOptionDto
                {
                    MaTheLoai = 0,
                    TenTheLoai = "-- Tất cả thể loại --"
                });
                foreach (var category in lookups.TheLoai)
                    cboLocTheLoai.Items.Add(category);
                cboLocTheLoai.SelectedIndex = 0;

                cboTrangThaiSach.SelectedIndex = 0;

                clbTacGia.DisplayMember = nameof(TacGiaOptionDto.TenTacGia);
                clbTacGia.Items.Clear();
                foreach (var author in lookups.TacGia)
                    clbTacGia.Items.Add(author);
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
            finally
            {
                loadingLookups = false;
            }

            await LoadBooksAsync();
        }

        private async void FilterChanged(object? sender, EventArgs e)
        {
            if (!loadingLookups && !suppressFilterReload)
                await LoadBooksAsync();
        }

        private async Task LoadBooksAsync()
        {
            searchCancellation?.Cancel();
            searchCancellation?.Dispose();
            searchCancellation = new CancellationTokenSource();
            var cancellationToken = searchCancellation.Token;

            var selectedCategory = cboLocTheLoai.SelectedItem as TheLoaiOptionDto;
            int? categoryId = selectedCategory is { MaTheLoai: > 0 }
                ? selectedCategory.MaTheLoai
                : null;
            var onlyDeleted = cboTrangThaiSach.SelectedIndex == 1;
            var includeDeleted = cboTrangThaiSach.SelectedIndex == 2;

            try
            {
                var books = await apiClient.GetBooksAsync(
                    txtTimKiem.Text,
                    categoryId,
                    includeDeleted,
                    onlyDeleted,
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested || IsDisposed)
                    return;

                ResetBookSelection();
                dgvSach.Rows.Clear();
                foreach (var book in books)
                {
                    var rowIndex = dgvSach.Rows.Add(
                        book.MaSach,
                        book.ISBN,
                        book.TenSach,
                        book.TenTheLoai,
                        string.Join(", ", book.TenTacGia),
                        book.NamXB,
                        book.NhaXB ?? string.Empty,
                        book.SoLuong,
                        book.SoLuongCon,
                        book.IsDeleted ? "Đã ẩn" : "Hoạt động");
                    dgvSach.Rows[rowIndex].Tag = book;
                }
            }
            catch (OperationCanceledException)
            {
                // A newer search request superseded this one.
            }
            catch (Exception exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                    ShowApiError(exception);
            }
        }

        private static void ShowApiError(Exception exception)
        {
            var message = exception is HttpRequestException
                ? exception.Message
                : $"Không thể hoàn tất thao tác sách: {exception.Message}";
            MessageBox.Show(message, "Lỗi API sách", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool ValidateInput(out string selectedAuthors)
        {
            selectedAuthors = "";
            List<string> authors = new List<string>();
            foreach (var item in clbTacGia.CheckedItems)
                authors.Add(item.ToString() ?? string.Empty);

            selectedAuthors = string.Join(", ", authors);

            if (string.IsNullOrWhiteSpace(txtTenSach.Text))
            {
                MessageBox.Show("Tên ấn phẩm không được để trống!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenSach.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                MessageBox.Show("Vui lòng nhập mã ISBN (Dùng để tra cứu chuẩn quốc tế)!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Focus();
                return false;
            }

            if (authors.Count == 0)
            {
                MessageBox.Show("Vui lòng tích chọn ít nhất 1 Tác giả cho ấn phẩm này!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out _)) return;

            try
            {
                var created = await apiClient.CreateAsync(BuildWriteRequest(), CancellationToken.None);
                MessageBox.Show($"Đã thêm sách mã {created.MaSach}.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                await LoadBooksAsync();
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
        }

        private async void BtnSua_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaSach.Text, out var id) || dgvSach.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một ấn phẩm dưới bảng trước khi tu chỉnh!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvSach.CurrentRow.Tag is SachDto selectedBook && selectedBook.IsDeleted)
            {
                MessageBox.Show("Sách đang ẩn không thể chỉnh sửa. Hãy khôi phục sách trước.", "Sách đã ẩn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out _)) return;

            try
            {
                await apiClient.UpdateAsync(id, BuildWriteRequest(), CancellationToken.None);
                MessageBox.Show("Đã cập nhật thông tin sách.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadBooksAsync();
            }
            catch (Exception exception)
            {
                ShowApiError(exception);
            }
        }

        private async void BtnXoa_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaSach.Text, out var id) || dgvSach.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn ấn phẩm cần lưu kho ẩn!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvSach.CurrentRow.Tag is SachDto selectedBook && selectedBook.IsDeleted)
            {
                var restoreConfirm = MessageBox.Show(
                    $"Khôi phục sách [{selectedBook.TenSach}] về trạng thái hoạt động?",
                    "Xác nhận khôi phục", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (restoreConfirm != DialogResult.Yes) return;

                try
                {
                    await apiClient.RestoreAsync(id, CancellationToken.None);
                    ClearForm();
                    suppressFilterReload = true;
                    try { cboTrangThaiSach.SelectedIndex = 0; }
                    finally { suppressFilterReload = false; }
                    await LoadBooksAsync();
                    MessageBox.Show("Đã khôi phục sách về trạng thái hoạt động.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    ShowApiError(exception);
                }
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc muốn chuyển sách [{txtTenSach.Text}] sang trạng thái Lưu kho ẩn (Soft Delete)?\nDữ liệu phiếu mượn cũ vẫn sẽ được bảo toàn.",
                "Xác nhận xóa mềm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    await apiClient.DeleteAsync(id, CancellationToken.None);
                    ClearForm();
                    cboTrangThaiSach.SelectedIndex = 1;
                }
                catch (Exception exception)
                {
                    ShowApiError(exception);
                }
            }
        }

        private SachWriteDto BuildWriteRequest()
        {
            var category = cboTheLoai.SelectedItem as TheLoaiOptionDto;
            return new SachWriteDto
            {
                ISBN = txtISBN.Text.Trim(),
                TenSach = txtTenSach.Text.Trim(),
                MaTheLoai = category?.MaTheLoai ?? 0,
                NamXB = (int)numNamXB.Value,
                NhaXB = txtNhaXB.Text.Trim(),
                SoLuong = (int)numSoLuong.Value,
                MoTa = txtMoTa.Text.Trim(),
                HinhAnh = currentImagePath,
                MaTacGia = clbTacGia.CheckedItems
                    .Cast<TacGiaOptionDto>()
                    .Select(author => author.MaTacGia)
                    .ToList()
            };
        }

        private void DgvSach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvSach.Rows[e.RowIndex];
            if (row.Tag is not SachDto book) return;

            txtMaSach.Text = book.MaSach.ToString();
            btnXoaSach!.Text = book.IsDeleted ? "↩ Khôi phục sách" : "🗃️ Lưu Kho Ẩn (Xóa mềm)";
            btnXoaSach.BackColor = book.IsDeleted ? Color.FromArgb(45, 106, 79) : Burgundy;
            txtISBN.Text = book.ISBN;
            txtTenSach.Text = book.TenSach;
            cboTheLoai.SelectedItem = cboTheLoai.Items
                .Cast<TheLoaiOptionDto>()
                .FirstOrDefault(category => category.MaTheLoai == book.MaTheLoai);
            numNamXB.Value = Math.Clamp(book.NamXB, (int)numNamXB.Minimum, (int)numNamXB.Maximum);
            txtNhaXB.Text = book.NhaXB ?? string.Empty;
            numSoLuong.Value = Math.Clamp(book.SoLuong, (int)numSoLuong.Minimum, (int)numSoLuong.Maximum);
            numSoLuongCon.Value = Math.Clamp(book.SoLuongCon, (int)numSoLuongCon.Minimum, (int)numSoLuongCon.Maximum);
            txtMoTa.Text = book.MoTa ?? string.Empty;
            currentImagePath = book.HinhAnh ?? string.Empty;
            DisplayCover(currentImagePath);

            for (int i = 0; i < clbTacGia.Items.Count; i++)
            {
                if (clbTacGia.Items[i] is TacGiaOptionDto author)
                    clbTacGia.SetItemChecked(i, book.MaTacGia.Contains(author.MaTacGia));
            }
        }

        private void DisplayCover(string imagePath)
        {
            picBiaSach.Image?.Dispose();
            picBiaSach.Image = null;
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return;

            try
            {
                using var imageStream = new MemoryStream(File.ReadAllBytes(imagePath));
                using var image = Image.FromStream(imageStream);
                picBiaSach.Image = new Bitmap(image);
            }
            catch (IOException)
            {
                // Missing or inaccessible cover images don't prevent book editing.
            }
            catch (ArgumentException)
            {
                // Ignore files that aren't valid image data.
            }
        }

        private void BtnChonAnh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentImagePath = ofd.FileName;
                    DisplayCover(currentImagePath);
                }
            }
        }

        private void ClearForm()
        {
            ResetBookSelection();
            txtMaSach.Clear();
            txtISBN.Clear();
            txtTenSach.Clear();
            txtNhaXB.Clear();
            txtMoTa.Clear();
            numNamXB.Value = 2026;
            numSoLuong.Value = 10;
            numSoLuongCon.Value = 10;
            picBiaSach.Image?.Dispose();
            picBiaSach.Image = null;
            currentImagePath = string.Empty;
            for (int i = 0; i < clbTacGia.Items.Count; i++)
                clbTacGia.SetItemChecked(i, false);
            txtTenSach.Focus();
        }

        private void ResetBookSelection()
        {
            txtMaSach.Clear();
            dgvSach.ClearSelection();
            if (btnXoaSach is not null)
            {
                btnXoaSach.Text = "🗃️ Lưu Kho Ẩn (Xóa mềm)";
                btnXoaSach.BackColor = Burgundy;
            }
        }
    }
}
