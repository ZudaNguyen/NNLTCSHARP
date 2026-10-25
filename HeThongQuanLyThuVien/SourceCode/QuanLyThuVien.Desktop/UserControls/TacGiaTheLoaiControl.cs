using System;
using System.Drawing;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class TacGiaTheLoaiControl : UserControl
    {
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        // Nửa trái: Bảng TheLoai
        private TextBox txtMaTheLoai, txtTenTheLoai, txtMoTaTL;
        private DataGridView dgvTheLoai;
        private TextBox txtTimTheLoai;
        private ComboBox cboTrangThaiTheLoai;
        private Button btnAnTheLoai;

        // Nửa phải: Bảng TacGia
        private TextBox txtMaTacGia, txtTenTacGia, txtQuocTich, txtGioiThieuTG;
        private DataGridView dgvTacGia;
        private TextBox txtTimTacGia;
        private ComboBox cboTrangThaiTacGia;
        private Button btnAnTacGia;
        private readonly CatalogApiClient _api = new();
        private CancellationTokenSource? _searchTheLoaiCts;
        private CancellationTokenSource? _searchTacGiaCts;
        private bool _loadingInitialData;

        public TacGiaTheLoaiControl()
        {
            InitializeComponent();
            SetupSplitUI();
            _ = LoadInitialDataAsync();
        }

        private void SetupSplitUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(18);

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = PaperCream,
                Font = new Font("Georgia", 10, FontStyle.Bold),
                Padding = new Point(14, 6)
            };
            var tabTheLoai = new TabPage("  Quản lý Thể loại  ") { BackColor = PaperCream, Padding = new Padding(8) };
            var tabTacGia = new TabPage("  Quản lý Tác giả  ") { BackColor = PaperCream, Padding = new Padding(8) };
            tabs.TabPages.AddRange([tabTheLoai, tabTacGia]);
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.ItemSize = new Size(180, 38);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.DrawItem += (_, e) =>
            {
                var selected = e.Index == tabs.SelectedIndex;
                using var background = new SolidBrush(selected ? Burgundy : Color.FromArgb(224, 213, 191));
                e.Graphics.FillRectangle(background, e.Bounds);
                TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text.Trim(), tabs.Font,
                    e.Bounds, selected ? Color.White : WoodDark,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // =========================================================
            // PHÂN KHU TRÁI: QUẢN LÝ THỂ LOẠI SÁCH (BẢNG TheLoai)
            // =========================================================
            Panel pnlTheLoai = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = Padding.Empty,
                Padding = new Padding(12)
            };

            var pnlTLTop = new TableLayoutPanel { Dock = DockStyle.Top, Height = 242, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty, Padding = Padding.Empty };
            pnlTLTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlTLTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
            pnlTLTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var pnlTLFields = new Panel { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new Size(710, 0) };
            Label lblTLTitle = new Label
            {
                Text = "🔖 Danh Mục Thể Loại (Bảng TheLoai)",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(5, 5),
                AutoSize = true
            };
            pnlTLFields.Controls.Add(lblTLTitle);

            AddLabel(pnlTLFields, "Mã TL:", 5, 40);
            txtMaTheLoai = AddTextBox(pnlTLFields, 62, 37, 82);
            txtMaTheLoai.ReadOnly = true;
            txtMaTheLoai.BackColor = Color.FromArgb(235, 230, 220);
            txtMaTheLoai.PlaceholderText = "Tự động";

            AddLabel(pnlTLFields, "Tên thể loại:", 158, 40);
            txtTenTheLoai = AddTextBox(pnlTLFields, 245, 37, 280);

            AddLabel(pnlTLFields, "Mô tả:", 5, 85);
            txtMoTaTL = new TextBox
            {
                Location = new Point(85, 80),
                Size = new Size(620, 44),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtMoTaTL.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlTLFields.Controls.Add(txtMoTaTL);

            FlowLayoutPanel flpTLBtns = new FlowLayoutPanel { Location = new Point(5, 128), Size = new Size(420, 36), WrapContents = false };
            Button btnThemTL = CreateMiniButton("➕ Thêm", ForestGreen);
            Button btnSuaTL = CreateMiniButton("✏️ Sửa", Color.FromArgb(180, 120, 20));
            btnAnTheLoai = CreateMiniButton("🗃️ Ẩn (Xóa)", Burgundy);
            Button btnMoiTL = CreateMiniButton("🔄 Mới", Color.FromArgb(90, 75, 65));

            btnThemTL.Click += BtnThemTL_Click;
            btnSuaTL.Click += BtnSuaTL_Click;
            btnAnTheLoai.Click += BtnXoaTL_Click;
            btnMoiTL.Click += (_, _) => ClearTheLoai();

            flpTLBtns.Controls.AddRange(new Control[] { btnThemTL, btnSuaTL, btnAnTheLoai, btnMoiTL });
            pnlTLFields.Controls.Add(flpTLBtns);
            txtTimTheLoai = new TextBox { Width = 250, Font = new Font("Segoe UI", 9.5f), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Tên thể loại hoặc mô tả" };
            txtTimTheLoai.PlaceholderText = "Tên thể loại hoặc mô tả";
            txtTimTheLoai.TextChanged += (_, _) => QueueSearch(true);
            cboTrangThaiTheLoai = CreateStatusFilter("Tất cả");
            cboTrangThaiTheLoai.SelectedIndexChanged += (_, _) => QueueSearch(true);
            var tlFilter = CreateCatalogFilterBar(txtTimTheLoai, cboTrangThaiTheLoai);
            pnlTLTop.Controls.Add(pnlTLFields, 0, 0);
            pnlTLTop.Controls.Add(tlFilter, 0, 1);
            pnlTLFields.SizeChanged += (_, _) =>
            {
                var width = Math.Max(220, pnlTLFields.ClientSize.Width - 265);
                txtTenTheLoai.Width = width;
                txtMoTaTL.Width = Math.Max(250, pnlTLFields.ClientSize.Width - txtMoTaTL.Left - 15);
            };

            dgvTheLoai = CreateStyledGrid();
            dgvTheLoai.Columns.Add("MaTL", "Mã TL");
            dgvTheLoai.Columns["MaTL"].FillWeight = 45;
            dgvTheLoai.Columns.Add("TenTL", "Tên Thể Loại");
            dgvTheLoai.Columns["TenTL"].FillWeight = 120;
            dgvTheLoai.Columns.Add("MoTa", "Mô Tả");
            dgvTheLoai.Columns["MoTa"].FillWeight = 150;
            dgvTheLoai.Columns.Add("TrangThai", "Trạng Thái");
            dgvTheLoai.Columns["TrangThai"].FillWeight = 70;
            dgvTheLoai.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgvTheLoai.Rows[e.RowIndex];
                var item = row.Tag as TheLoaiDto;
                txtMaTheLoai.Text = row.Cells["MaTL"].Value?.ToString();
                txtTenTheLoai.Text = row.Cells["TenTL"].Value?.ToString();
                txtMoTaTL.Text = row.Cells["MoTa"].Value?.ToString();
                SetRestoreButton(btnAnTheLoai, item?.IsDeleted == true);
            };

            pnlTheLoai.Controls.Add(dgvTheLoai);
            pnlTheLoai.Controls.Add(pnlTLTop);

            // =========================================================
            // PHÂN KHU PHẢI: QUẢN LÝ TÁC GIẢ (BẢNG TacGia)
            // =========================================================
            Panel pnlTacGia = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10, 0, 0, 0),
                Padding = new Padding(12)
            };

            var pnlTGTop = new TableLayoutPanel { Dock = DockStyle.Top, Height = 242, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty, Padding = Padding.Empty };
            pnlTGTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlTGTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
            pnlTGTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var pnlTGFields = new Panel { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new Size(710, 0) };
            Label lblTGTitle = new Label
            {
                Text = "✒️ Hồ Sơ Tác Giả (Bảng TacGia)",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(5, 5),
                AutoSize = true
            };
            pnlTGFields.Controls.Add(lblTGTitle);

            AddLabel(pnlTGFields, "Mã TG:", 5, 40);
            txtMaTacGia = AddTextBox(pnlTGFields, 62, 37, 82);
            txtMaTacGia.ReadOnly = true;
            txtMaTacGia.BackColor = Color.FromArgb(235, 230, 220);
            txtMaTacGia.PlaceholderText = "Tự động";

            AddLabel(pnlTGFields, "Tên tác giả:", 158, 40);
            txtTenTacGia = AddTextBox(pnlTGFields, 245, 37, 270);

            var lblQuocTich = new Label
            {
                Text = "Quốc tịch:", Location = new Point(520, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = WoodDark, AutoSize = true
            };
            pnlTGFields.Controls.Add(lblQuocTich);
            txtQuocTich = AddTextBox(pnlTGFields, 605, 37, 140);

            AddLabel(pnlTGFields, "Giới thiệu:", 5, 85);
            txtGioiThieuTG = new TextBox
            {
                Location = new Point(90, 80),
                Size = new Size(620, 44),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtGioiThieuTG.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlTGFields.Controls.Add(txtGioiThieuTG);

            FlowLayoutPanel flpTGBtns = new FlowLayoutPanel { Location = new Point(5, 128), Size = new Size(420, 36), WrapContents = false };
            Button btnThemTG = CreateMiniButton("➕ Thêm", ForestGreen);
            Button btnSuaTG = CreateMiniButton("✏️ Sửa", Color.FromArgb(180, 120, 20));
            btnAnTacGia = CreateMiniButton("🗃️ Ẩn (Xóa)", Burgundy);
            Button btnMoiTG = CreateMiniButton("🔄 Mới", Color.FromArgb(90, 75, 65));

            btnThemTG.Click += BtnThemTG_Click;
            btnSuaTG.Click += BtnSuaTG_Click;
            btnAnTacGia.Click += BtnXoaTG_Click;
            btnMoiTG.Click += (_, _) => ClearTacGia();

            flpTGBtns.Controls.AddRange(new Control[] { btnThemTG, btnSuaTG, btnAnTacGia, btnMoiTG });
            pnlTGFields.Controls.Add(flpTGBtns);
            txtTimTacGia = new TextBox { Width = 250, Font = new Font("Segoe UI", 9.5f), BackColor = Color.FromArgb(255, 253, 248), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Tên tác giả, quốc tịch hoặc giới thiệu" };
            txtTimTacGia.TextChanged += (_, _) => QueueSearch(false);
            cboTrangThaiTacGia = CreateStatusFilter("Tất cả");
            cboTrangThaiTacGia.SelectedIndexChanged += (_, _) => QueueSearch(false);
            var tgFilter = CreateCatalogFilterBar(txtTimTacGia, cboTrangThaiTacGia);
            pnlTGTop.Controls.Add(pnlTGFields, 0, 0);
            pnlTGTop.Controls.Add(tgFilter, 0, 1);
            pnlTGFields.SizeChanged += (_, _) =>
            {
                var countryLabelLeft = Math.Max(520, pnlTGFields.ClientSize.Width - 230);
                lblQuocTich.Left = countryLabelLeft;
                txtTenTacGia.Width = Math.Max(180, countryLabelLeft - txtTenTacGia.Left - 15);
                txtQuocTich.Left = countryLabelLeft + 78;
                txtQuocTich.Width = Math.Max(100, pnlTGFields.ClientSize.Width - txtQuocTich.Left - 10);
                txtGioiThieuTG.Width = Math.Max(250, pnlTGFields.ClientSize.Width - txtGioiThieuTG.Left - 15);
            };

            dgvTacGia = CreateStyledGrid();
            dgvTacGia.Columns.Add("MaTG", "Mã TG");
            dgvTacGia.Columns["MaTG"].FillWeight = 45;
            dgvTacGia.Columns.Add("TenTG", "Tên Tác Giả");
            dgvTacGia.Columns["TenTG"].FillWeight = 120;
            dgvTacGia.Columns.Add("QuocTich", "Quốc Tịch");
            dgvTacGia.Columns["QuocTich"].FillWeight = 80;
            dgvTacGia.Columns.Add("GioiThieu", "Tiểu Sử / Giới Thiệu");
            dgvTacGia.Columns["GioiThieu"].FillWeight = 140;
            dgvTacGia.Columns.Add("TrangThai", "Trạng Thái");
            dgvTacGia.Columns["TrangThai"].FillWeight = 70;
            dgvTacGia.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgvTacGia.Rows[e.RowIndex];
                var item = row.Tag as TacGiaDto;
                txtMaTacGia.Text = row.Cells["MaTG"].Value?.ToString();
                txtTenTacGia.Text = row.Cells["TenTG"].Value?.ToString();
                txtQuocTich.Text = row.Cells["QuocTich"].Value?.ToString();
                txtGioiThieuTG.Text = row.Cells["GioiThieu"].Value?.ToString();
                SetRestoreButton(btnAnTacGia, item?.IsDeleted == true);
            };

            pnlTacGia.Controls.Add(dgvTacGia);
            pnlTacGia.Controls.Add(pnlTGTop);

            // Ghép 2 phân khu vào TableLayoutPanel
            pnlTheLoai.Dock = DockStyle.Fill;
            pnlTacGia.Dock = DockStyle.Fill;
            tabTheLoai.Controls.Add(pnlTheLoai);
            tabTacGia.Controls.Add(pnlTacGia);
            this.Controls.Add(tabs);
        }

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
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 30 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9, FontStyle.Bold);
            DataGridViewStyleHelper.ApplyLibraryStyle(dgv);
            return dgv;
        }

        private static ComboBox CreateStatusFilter(string defaultValue)
        {
            var combo = new ComboBox
            {
                Width = 180,
                Font = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(3, 4, 8, 3)
            };
            combo.Items.AddRange(["Tất cả", "Đang hoạt động", "Đã ẩn"]);
            combo.SelectedItem = defaultValue;
            return combo;
        }

        private static FlowLayoutPanel CreateCatalogFilterBar(TextBox search, ComboBox status)
        {
            var bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = true, AutoScroll = false,
                FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(3, 5, 3, 2),
                Margin = Padding.Empty, BackColor = Color.FromArgb(247, 243, 233)
            };
            var searchLabel = new Label
            {
                Text = "Tìm kiếm:", Width = 82, Height = 30, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 30, 22),
                Margin = new Padding(3, 3, 2, 2)
            };
            var statusLabel = new Label
            {
                Text = "Trạng thái:", Width = 82, Height = 30, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 30, 22),
                Margin = new Padding(10, 3, 2, 2)
            };
            search.Width = 250;
            search.Margin = new Padding(3, 4, 8, 2);
            bar.Controls.AddRange([searchLabel, search, statusLabel, status]);
            return bar;
        }

        private static void SetRestoreButton(Button button, bool isDeleted)
        {
            button.Text = isDeleted ? "↩ Hiện lại" : "🗃️ Ẩn (Xóa)";
            button.BackColor = isDeleted ? Color.FromArgb(45, 106, 79) : Color.FromArgb(122, 40, 34);
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

        private TextBox AddTextBox(Control parent, int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateMiniButton(string text, Color bg)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(92, 32),
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async Task LoadInitialDataAsync()
        {
            _loadingInitialData = true;
            cboTrangThaiTheLoai.SelectedIndex = 0;
            cboTrangThaiTacGia.SelectedIndex = 0;
            _loadingInitialData = false;
            try { await RefreshTheLoaiAsync(CancellationToken.None); await RefreshTacGiaAsync(CancellationToken.None); }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private void QueueSearch(bool category)
        {
            if (_loadingInitialData) return;
            if (category) { _searchTheLoaiCts?.Cancel(); _searchTheLoaiCts?.Dispose(); _searchTheLoaiCts = new(); _ = SearchTheLoaiAfterDelayAsync(_searchTheLoaiCts.Token); }
            else { _searchTacGiaCts?.Cancel(); _searchTacGiaCts?.Dispose(); _searchTacGiaCts = new(); _ = SearchTacGiaAfterDelayAsync(_searchTacGiaCts.Token); }
        }

        private async Task SearchTheLoaiAfterDelayAsync(CancellationToken token)
        {
            try { await Task.Delay(300, token); await RefreshTheLoaiAsync(token); }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!token.IsCancellationRequested) ShowApiError(ex); }
        }

        private async Task SearchTacGiaAfterDelayAsync(CancellationToken token)
        {
            try { await Task.Delay(300, token); await RefreshTacGiaAsync(token); }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!token.IsCancellationRequested) ShowApiError(ex); }
        }

        private async Task RefreshTheLoaiAsync(CancellationToken token)
        {
            var onlyDeleted = cboTrangThaiTheLoai.SelectedIndex == 2;
            var includeDeleted = cboTrangThaiTheLoai.SelectedIndex != 1;
            var items = await _api.GetTheLoaiAsync(txtTimTheLoai.Text, includeDeleted, onlyDeleted, token);
            ClearTheLoai();
            dgvTheLoai.Rows.Clear();
            foreach (var item in items)
            {
                var index = dgvTheLoai.Rows.Add(item.MaTheLoai, item.TenTheLoai, item.MoTa ?? string.Empty, item.IsDeleted ? "Đã ẩn" : "Hoạt động");
                dgvTheLoai.Rows[index].Tag = item;
            }
        }

        private async Task RefreshTacGiaAsync(CancellationToken token)
        {
            var onlyDeleted = cboTrangThaiTacGia.SelectedIndex == 2;
            var includeDeleted = cboTrangThaiTacGia.SelectedIndex != 1;
            var items = await _api.GetTacGiaAsync(txtTimTacGia.Text, includeDeleted, onlyDeleted, token);
            ClearTacGia();
            dgvTacGia.Rows.Clear();
            foreach (var item in items)
            {
                var index = dgvTacGia.Rows.Add(item.MaTacGia, item.TenTacGia, item.QuocTich ?? string.Empty, item.GioiThieu ?? string.Empty, item.IsDeleted ? "Đã ẩn" : "Hoạt động");
                dgvTacGia.Rows[index].Tag = item;
            }
        }

        private async void BtnThemTL_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTheLoai.Text)) { WarnRequired("Tên thể loại"); return; }
            try
            {
                await _api.CreateTheLoaiAsync(new TheLoaiWriteDto { TenTheLoai = txtTenTheLoai.Text, MoTa = txtMoTaTL.Text }, CancellationToken.None);
                ClearTheLoai(); await RefreshTheLoaiAsync(CancellationToken.None); ShowSuccess("Đã thêm thể loại.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnSuaTL_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaTheLoai.Text, out var id)) { WarnSelect("thể loại"); return; }
            if (dgvTheLoai.CurrentRow?.Tag is TheLoaiDto { IsDeleted: true }) { WarnDeleted("thể loại"); return; }
            if (string.IsNullOrWhiteSpace(txtTenTheLoai.Text)) { WarnRequired("Tên thể loại"); return; }
            try
            {
                await _api.UpdateTheLoaiAsync(id, new TheLoaiWriteDto { TenTheLoai = txtTenTheLoai.Text, MoTa = txtMoTaTL.Text }, CancellationToken.None);
                await RefreshTheLoaiAsync(CancellationToken.None); ShowSuccess("Đã cập nhật thể loại.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnXoaTL_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaTheLoai.Text, out var id)) { WarnSelect("thể loại"); return; }
            var isDeleted = dgvTheLoai.CurrentRow?.Tag is TheLoaiDto { IsDeleted: true };
            if (isDeleted)
            {
                if (MessageBox.Show("Khôi phục thể loại đã chọn về trạng thái hoạt động?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                try
                {
                    await _api.RestoreTheLoaiAsync(id, CancellationToken.None);
                    ClearTheLoai(); cboTrangThaiTheLoai.SelectedIndex = 0;
                    await RefreshTheLoaiAsync(CancellationToken.None); ShowSuccess("Đã khôi phục thể loại.");
                }
                catch (Exception ex) { ShowApiError(ex); }
                return;
            }
            if (MessageBox.Show("Ẩn thể loại đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await _api.DeleteTheLoaiAsync(id, CancellationToken.None); ClearTheLoai();
                await RefreshTheLoaiAsync(CancellationToken.None); ShowSuccess("Đã ẩn thể loại.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnThemTG_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTacGia.Text)) { WarnRequired("Tên tác giả"); return; }
            try
            {
                await _api.CreateTacGiaAsync(new TacGiaWriteDto { TenTacGia = txtTenTacGia.Text, QuocTich = txtQuocTich.Text, GioiThieu = txtGioiThieuTG.Text }, CancellationToken.None);
                ClearTacGia(); await RefreshTacGiaAsync(CancellationToken.None); ShowSuccess("Đã thêm tác giả.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnSuaTG_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaTacGia.Text, out var id)) { WarnSelect("tác giả"); return; }
            if (dgvTacGia.CurrentRow?.Tag is TacGiaDto { IsDeleted: true }) { WarnDeleted("tác giả"); return; }
            if (string.IsNullOrWhiteSpace(txtTenTacGia.Text)) { WarnRequired("Tên tác giả"); return; }
            try
            {
                await _api.UpdateTacGiaAsync(id, new TacGiaWriteDto { TenTacGia = txtTenTacGia.Text, QuocTich = txtQuocTich.Text, GioiThieu = txtGioiThieuTG.Text }, CancellationToken.None);
                await RefreshTacGiaAsync(CancellationToken.None); ShowSuccess("Đã cập nhật tác giả.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnXoaTG_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaTacGia.Text, out var id)) { WarnSelect("tác giả"); return; }
            var isDeleted = dgvTacGia.CurrentRow?.Tag is TacGiaDto { IsDeleted: true };
            if (isDeleted)
            {
                if (MessageBox.Show("Khôi phục tác giả đã chọn về trạng thái hoạt động?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                try
                {
                    await _api.RestoreTacGiaAsync(id, CancellationToken.None);
                    ClearTacGia(); cboTrangThaiTacGia.SelectedIndex = 0;
                    await RefreshTacGiaAsync(CancellationToken.None); ShowSuccess("Đã khôi phục tác giả.");
                }
                catch (Exception ex) { ShowApiError(ex); }
                return;
            }
            if (MessageBox.Show("Ẩn tác giả đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await _api.DeleteTacGiaAsync(id, CancellationToken.None); ClearTacGia();
                await RefreshTacGiaAsync(CancellationToken.None); ShowSuccess("Đã ẩn tác giả.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private void ClearTheLoai()
        {
            txtMaTheLoai.Clear(); txtTenTheLoai.Clear(); txtMoTaTL.Clear(); dgvTheLoai.ClearSelection();
            SetRestoreButton(btnAnTheLoai, false);
        }

        private void ClearTacGia()
        {
            txtMaTacGia.Clear(); txtTenTacGia.Clear(); txtQuocTich.Clear(); txtGioiThieuTG.Clear(); dgvTacGia.ClearSelection();
            SetRestoreButton(btnAnTacGia, false);
        }
        private static void WarnDeleted(string entity) => MessageBox.Show($"Không thể sửa {entity} đang ẩn. Hãy khôi phục trước.", "Dữ liệu đã ẩn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private static void WarnRequired(string field) => MessageBox.Show($"{field} không được để trống.", "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private static void WarnSelect(string entity) => MessageBox.Show($"Vui lòng chọn {entity} trong danh sách.", "Chưa chọn dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private static void ShowSuccess(string message) => MessageBox.Show(message, "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private static void ShowApiError(Exception exception)
        {
            var message = exception is HttpRequestException http && http.StatusCode is null
                ? "Không kết nối được API. Hãy kiểm tra API đang chạy tại localhost:5058."
                : exception.Message;
            MessageBox.Show(message, "Lỗi API danh mục", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
