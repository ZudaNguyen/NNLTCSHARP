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

            // Dùng TableLayoutPanel chia đôi màn hình 50% - 50%
            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = PaperCream
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));

            // =========================================================
            // PHÂN KHU TRÁI: QUẢN LÝ THỂ LOẠI SÁCH (BẢNG TheLoai)
            // =========================================================
            Panel pnlTheLoai = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(12)
            };

            Panel pnlTLTop = new Panel { Dock = DockStyle.Top, Height = 282 };
            Label lblTLTitle = new Label
            {
                Text = "🔖 Danh Mục Thể Loại (Bảng TheLoai)",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(5, 5),
                AutoSize = true
            };
            pnlTLTop.Controls.Add(lblTLTitle);

            AddLabel(pnlTLTop, "Mã TL:", 5, 38);
            txtMaTheLoai = AddTextBox(pnlTLTop, 85, 35, 90);
            txtMaTheLoai.ReadOnly = true;
            txtMaTheLoai.BackColor = Color.FromArgb(235, 230, 220);
            txtMaTheLoai.PlaceholderText = "Tự động";

            AddLabel(pnlTLTop, "Tên TL (*):", 5, 74);
            txtTenTheLoai = AddTextBox(pnlTLTop, 85, 71, 280);

            AddLabel(pnlTLTop, "Mô tả:", 5, 110);
            txtMoTaTL = new TextBox
            {
                Location = new Point(85, 107),
                Size = new Size(280, 48),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlTLTop.Controls.Add(txtMoTaTL);

            FlowLayoutPanel flpTLBtns = new FlowLayoutPanel { Location = new Point(5, 165), Size = new Size(420, 40) };
            Button btnThemTL = CreateMiniButton("➕ Thêm", ForestGreen);
            Button btnSuaTL = CreateMiniButton("✏️ Sửa", Color.FromArgb(180, 120, 20));
            btnAnTheLoai = CreateMiniButton("🗃️ Ẩn (Xóa)", Burgundy);
            Button btnMoiTL = CreateMiniButton("🔄 Mới", Color.FromArgb(90, 75, 65));

            btnThemTL.Click += BtnThemTL_Click;
            btnSuaTL.Click += BtnSuaTL_Click;
            btnAnTheLoai.Click += BtnXoaTL_Click;
            btnMoiTL.Click += (_, _) => ClearTheLoai();

            flpTLBtns.Controls.AddRange(new Control[] { btnThemTL, btnSuaTL, btnAnTheLoai, btnMoiTL });
            pnlTLTop.Controls.Add(flpTLBtns);
            AddLabel(pnlTLTop, "Tìm:", 5, 216);
            txtTimTheLoai = AddTextBox(pnlTLTop, 85, 213, 280);
            txtTimTheLoai.PlaceholderText = "Tên thể loại hoặc mô tả";
            txtTimTheLoai.TextChanged += (_, _) => QueueSearch(true);
            AddLabel(pnlTLTop, "Trạng thái:", 5, 249);
            cboTrangThaiTheLoai = CreateStatusFilter(pnlTLTop, 85, 245, 220);
            cboTrangThaiTheLoai.SelectedIndexChanged += (_, _) => QueueSearch(true);

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

            Panel pnlTGTop = new Panel { Dock = DockStyle.Top, Height = 282 };
            Label lblTGTitle = new Label
            {
                Text = "✒️ Hồ Sơ Tác Giả (Bảng TacGia)",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(5, 5),
                AutoSize = true
            };
            pnlTGTop.Controls.Add(lblTGTitle);

            AddLabel(pnlTGTop, "Mã TG:", 5, 38);
            txtMaTacGia = AddTextBox(pnlTGTop, 90, 35, 80);
            txtMaTacGia.ReadOnly = true;
            txtMaTacGia.BackColor = Color.FromArgb(235, 230, 220);
            txtMaTacGia.PlaceholderText = "Tự động";

            AddLabel(pnlTGTop, "Quốc tịch:", 185, 38);
            txtQuocTich = AddTextBox(pnlTGTop, 260, 35, 160);

            AddLabel(pnlTGTop, "Tên TG (*):", 5, 74);
            txtTenTacGia = AddTextBox(pnlTGTop, 90, 71, 330);

            AddLabel(pnlTGTop, "Giới thiệu:", 5, 110);
            txtGioiThieuTG = new TextBox
            {
                Location = new Point(90, 107),
                Size = new Size(330, 48),
                Multiline = true,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlTGTop.Controls.Add(txtGioiThieuTG);

            FlowLayoutPanel flpTGBtns = new FlowLayoutPanel { Location = new Point(5, 165), Size = new Size(450, 40) };
            Button btnThemTG = CreateMiniButton("➕ Thêm", ForestGreen);
            Button btnSuaTG = CreateMiniButton("✏️ Sửa", Color.FromArgb(180, 120, 20));
            btnAnTacGia = CreateMiniButton("🗃️ Ẩn (Xóa)", Burgundy);
            Button btnMoiTG = CreateMiniButton("🔄 Mới", Color.FromArgb(90, 75, 65));

            btnThemTG.Click += BtnThemTG_Click;
            btnSuaTG.Click += BtnSuaTG_Click;
            btnAnTacGia.Click += BtnXoaTG_Click;
            btnMoiTG.Click += (_, _) => ClearTacGia();

            flpTGBtns.Controls.AddRange(new Control[] { btnThemTG, btnSuaTG, btnAnTacGia, btnMoiTG });
            pnlTGTop.Controls.Add(flpTGBtns);
            AddLabel(pnlTGTop, "Tìm:", 5, 216);
            txtTimTacGia = AddTextBox(pnlTGTop, 90, 213, 330);
            txtTimTacGia.PlaceholderText = "Tên tác giả, quốc tịch hoặc giới thiệu";
            txtTimTacGia.TextChanged += (_, _) => QueueSearch(false);
            AddLabel(pnlTGTop, "Trạng thái:", 5, 249);
            cboTrangThaiTacGia = CreateStatusFilter(pnlTGTop, 90, 245, 260);
            cboTrangThaiTacGia.SelectedIndexChanged += (_, _) => QueueSearch(false);

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
            tlpMain.Controls.Add(pnlTheLoai, 0, 0);
            tlpMain.Controls.Add(pnlTacGia, 1, 0);
            this.Controls.Add(tlpMain);
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

            dgv.DefaultCellStyle.BackColor = CardBg;
            dgv.DefaultCellStyle.ForeColor = WoodDark;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
            dgv.DefaultCellStyle.SelectionForeColor = WoodDark;
            return dgv;
        }

        private static ComboBox CreateStatusFilter(Control parent, int x, int y, int width)
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            combo.Items.AddRange(["Đang hoạt động", "Đã ẩn", "Tất cả"]);
            parent.Controls.Add(combo);
            return combo;
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
                Font = new Font("Georgia", 8.5f, FontStyle.Bold),
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
            var onlyDeleted = cboTrangThaiTheLoai.SelectedIndex == 1;
            var includeDeleted = cboTrangThaiTheLoai.SelectedIndex == 2;
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
            var onlyDeleted = cboTrangThaiTacGia.SelectedIndex == 1;
            var includeDeleted = cboTrangThaiTacGia.SelectedIndex == 2;
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
