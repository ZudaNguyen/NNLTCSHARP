using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class SachControl : UserControl
    {
        // Các Control nhập liệu (Bám sát bảng Sach & Sach_TacGia)
        private TextBox txtMaSach = null!;
        private TextBox txtTenSach = null!;
        private TextBox txtISBN = null!;
        private ComboBox cboTheLoai = null!;
        private CheckedListBox clbTacGia = null!;
        private NumericUpDown numNamXB = null!;
        private TextBox txtNhaXB = null!;
        private NumericUpDown numSoLuong = null!;
        private NumericUpDown numSoLuongCon = null!;
        private TextBox txtMoTa = null!;
        private PictureBox picBiaSach = null!;
        private string currentImagePath = "";

        // Các Control Tìm kiếm & Danh sách
        private TextBox txtTimKiem = null!;
        private ComboBox cboLocTheLoai = null!;
        private ComboBox cboTrangThaiSach = null!;
        private DataGridView dgvSach = null!;
        private AppButton? btnXoaSach;
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
            // ----- Biểu mẫu thông tin sách -----
            txtMaSach = Ui.ReadOnlyBox();
            txtISBN = Ui.TextBox();
            txtTenSach = Ui.TextBox();
            cboTheLoai = Ui.ComboBox();
            txtNhaXB = Ui.TextBox();
            numNamXB = Ui.Number(1900, DateTime.Now.Year + 1, 2026);
            numSoLuong = Ui.Number(0, 10000, 10);
            numSoLuongCon = Ui.Number(0, 10000, 10);
            numSoLuongCon.Enabled = false; // Chỉ thay đổi qua nghiệp vụ Mượn/Trả
            // Khi đổi tổng số lượng lúc thêm mới, tự cập nhật số lượng còn
            numSoLuong.ValueChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtMaSach.Text))
                    numSoLuongCon.Value = numSoLuong.Value;
            };
            clbTacGia = new CheckedListBox
            {
                Font = UiTheme.Font(9.5f), BackColor = UiTheme.Surface, ForeColor = UiTheme.Text,
                BorderStyle = BorderStyle.FixedSingle, CheckOnClick = true, IntegralHeight = false, Height = 104
            };
            txtMoTa = Ui.MultilineBox(104);

            var fields = new FormGrid(6);
            fields.Add("Mã sách", txtMaSach);
            fields.Add("ISBN", txtISBN, 2);
            fields.Add("Tên sách", txtTenSach, 3);
            fields.Add("Thể loại", cboTheLoai, 2);
            fields.Add("Nhà xuất bản", txtNhaXB, 2);
            fields.Add("Năm XB", numNamXB);
            fields.Add("Tổng số lượng", numSoLuong);
            fields.Add("Tác giả (chọn một hoặc nhiều)", clbTacGia, 3);
            fields.Add("Mô tả", txtMoTa, 2);
            fields.Add("Còn lại", numSoLuongCon);

            // Ảnh bìa nằm bên phải biểu mẫu
            picBiaSach = new PictureBox
            {
                Size = new Size(132, 176), BackColor = UiTheme.SurfaceMuted, BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 0, 0, 10)
            };
            var btnChonAnh = Ui.Button("Chọn ảnh bìa");
            btnChonAnh.MinimumSize = new Size(132, 34);
            btnChonAnh.Margin = Padding.Empty;
            btnChonAnh.Click += BtnChonAnh_Click;
            var cover = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, BackColor = UiTheme.Surface, Margin = new Padding(20, 0, 0, 0), Padding = Padding.Empty
            };
            cover.Controls.Add(Ui.FieldLabel("Ảnh bìa"));
            cover.Controls.Add(picBiaSach);
            cover.Controls.Add(btnChonAnh);

            var body = new TableLayoutPanel
            {
                ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = UiTheme.Surface,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fields.Dock = DockStyle.Fill;
            cover.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            body.Controls.Add(fields, 0, 0);
            body.Controls.Add(cover, 1, 0);

            var btnThem = Ui.Button("Thêm sách", ButtonKind.Primary);
            var btnSua = Ui.Button("Cập nhật");
            btnXoaSach = Ui.Button("Ẩn sách", ButtonKind.Danger);
            var btnLamMoi = Ui.Button("Làm mới");
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoaSach.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => ClearForm();

            var formCard = new CardPanel("Thông tin sách");
            formCard.AddRow(body);
            formCard.AddRow(Ui.ButtonBar(btnThem, btnSua, btnXoaSach, btnLamMoi));

            // ----- Tìm kiếm & danh sách -----
            txtTimKiem = Ui.TextBox("Tên sách, ISBN hoặc tác giả", 300);
            txtTimKiem.TextChanged += FilterChanged;
            cboLocTheLoai = Ui.ComboBox(width: 200);
            cboLocTheLoai.SelectedIndexChanged += FilterChanged;
            cboTrangThaiSach = Ui.ComboBox(new object[] { "Đang hoạt động", "Đã ẩn", "Tất cả" }, width: 170);
            cboTrangThaiSach.SelectedIndexChanged += FilterChanged;

            var toolbar = new ToolbarRow();
            toolbar.AddField("Tra cứu", txtTimKiem);
            toolbar.AddField("Thể loại", cboLocTheLoai);
            toolbar.AddField("Trạng thái", cboTrangThaiSach);

            dgvSach = Ui.Grid();
            dgvSach.Columns.Add("MaSach", "Mã sách");
            dgvSach.Columns["MaSach"].FillWeight = 45;
            dgvSach.Columns.Add("ISBN", "ISBN");
            dgvSach.Columns["ISBN"].FillWeight = 75;
            dgvSach.Columns.Add("TenSach", "Tên sách");
            dgvSach.Columns["TenSach"].FillWeight = 150;
            dgvSach.Columns.Add("TheLoai", "Thể loại");
            dgvSach.Columns.Add("TacGia", "Tác giả");
            dgvSach.Columns.Add("NamXB", "Năm XB");
            dgvSach.Columns["NamXB"].FillWeight = 50;
            dgvSach.Columns.Add("NhaXB", "Nhà xuất bản");
            dgvSach.Columns.Add("SoLuong", "Tổng SL");
            dgvSach.Columns["SoLuong"].FillWeight = 50;
            dgvSach.Columns.Add("SoLuongCon", "SL còn");
            dgvSach.Columns["SoLuongCon"].FillWeight = 50;
            dgvSach.Columns.Add("TrangThai", "Trạng thái");
            dgvSach.Columns["TrangThai"].FillWeight = 70;
            dgvSach.CellClick += DgvSach_CellClick;

            var listCard = new CardPanel("Danh sách sách", autoHeight: false);
            listCard.AddRow(toolbar);
            listCard.AddRow(dgvSach, fill: true);

            Ui.SetupPage(this, Ui.ScrollStack(300, formCard, listCard));
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
                    TenTheLoai = "Tất cả thể loại"
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
                MessageBox.Show("Tên sách không được để trống!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenSach.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                MessageBox.Show("Vui lòng nhập mã ISBN (dùng để tra cứu chuẩn quốc tế)!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Focus();
                return false;
            }

            if (authors.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một tác giả cho sách này!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async void BtnThem_Click(object? sender, EventArgs e)
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

        private async void BtnSua_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtMaSach.Text, out var id) || dgvSach.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một cuốn sách trong bảng trước khi cập nhật!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private async void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtMaSach.Text, out var id) || dgvSach.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn cuốn sách cần ẩn!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                $"Bạn có chắc muốn ẩn sách [{txtTenSach.Text}]?\nDữ liệu phiếu mượn cũ vẫn được bảo toàn và có thể khôi phục sau.",
                "Xác nhận ẩn sách", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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

        private void DgvSach_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvSach.Rows[e.RowIndex];
            if (row.Tag is not SachDto book) return;

            txtMaSach.Text = book.MaSach.ToString();
            SetHideButton(book.IsDeleted);
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

        private void SetHideButton(bool isDeleted)
        {
            if (btnXoaSach is null) return;
            btnXoaSach.Text = isDeleted ? "Khôi phục sách" : "Ẩn sách";
            btnXoaSach.Kind = isDeleted ? ButtonKind.Success : ButtonKind.Danger;
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

        private void BtnChonAnh_Click(object? sender, EventArgs e)
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
            SetHideButton(false);
        }
    }
}
