using System;
using System.Drawing;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class TacGiaTheLoaiControl : UserControl
    {
        // Thể loại
        private TextBox txtMaTheLoai = null!, txtTenTheLoai = null!, txtMoTaTL = null!;
        private DataGridView dgvTheLoai = null!;
        private TextBox txtTimTheLoai = null!;
        private ComboBox cboTrangThaiTheLoai = null!;
        private AppButton btnAnTheLoai = null!;

        // Tác giả
        private TextBox txtMaTacGia = null!, txtTenTacGia = null!, txtQuocTich = null!, txtGioiThieuTG = null!;
        private DataGridView dgvTacGia = null!;
        private TextBox txtTimTacGia = null!;
        private ComboBox cboTrangThaiTacGia = null!;
        private AppButton btnAnTacGia = null!;
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
            var tabs = new PageTabs();
            tabs.AddPage("Thể loại", BuildTheLoaiPage());
            tabs.AddPage("Tác giả", BuildTacGiaPage());
            Ui.SetupPage(this, tabs);
        }

        private Control BuildTheLoaiPage()
        {
            txtMaTheLoai = Ui.ReadOnlyBox();
            txtTenTheLoai = Ui.TextBox();
            txtMoTaTL = Ui.MultilineBox(64);

            var fields = new FormGrid(4);
            fields.Add("Mã thể loại", txtMaTheLoai);
            fields.Add("Tên thể loại", txtTenTheLoai, 3);
            fields.Add("Mô tả", txtMoTaTL, 4);

            var btnThem = Ui.Button("Thêm thể loại", ButtonKind.Primary);
            var btnSua = Ui.Button("Cập nhật");
            btnAnTheLoai = Ui.Button("Ẩn thể loại", ButtonKind.Danger);
            var btnMoi = Ui.Button("Làm mới");
            btnThem.Click += BtnThemTL_Click;
            btnSua.Click += BtnSuaTL_Click;
            btnAnTheLoai.Click += BtnXoaTL_Click;
            btnMoi.Click += (_, _) => ClearTheLoai();

            var formCard = new CardPanel("Thông tin thể loại");
            formCard.AddRow(fields);
            formCard.AddRow(Ui.ButtonBar(btnThem, btnSua, btnAnTheLoai, btnMoi));

            txtTimTheLoai = Ui.TextBox("Tên thể loại hoặc mô tả", 300);
            txtTimTheLoai.TextChanged += (_, _) => QueueSearch(true);
            cboTrangThaiTheLoai = CreateStatusFilter();
            cboTrangThaiTheLoai.SelectedIndexChanged += (_, _) => QueueSearch(true);
            var toolbar = new ToolbarRow();
            toolbar.AddField("Tìm kiếm", txtTimTheLoai);
            toolbar.AddField("Trạng thái", cboTrangThaiTheLoai);

            dgvTheLoai = Ui.Grid();
            dgvTheLoai.Columns.Add("MaTL", "Mã");
            dgvTheLoai.Columns["MaTL"].FillWeight = 45;
            dgvTheLoai.Columns.Add("TenTL", "Tên thể loại");
            dgvTheLoai.Columns["TenTL"].FillWeight = 120;
            dgvTheLoai.Columns.Add("MoTa", "Mô tả");
            dgvTheLoai.Columns["MoTa"].FillWeight = 150;
            dgvTheLoai.Columns.Add("TrangThai", "Trạng thái");
            dgvTheLoai.Columns["TrangThai"].FillWeight = 70;
            dgvTheLoai.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgvTheLoai.Rows[e.RowIndex];
                var item = row.Tag as TheLoaiDto;
                txtMaTheLoai.Text = row.Cells["MaTL"].Value?.ToString();
                txtTenTheLoai.Text = row.Cells["TenTL"].Value?.ToString();
                txtMoTaTL.Text = row.Cells["MoTa"].Value?.ToString();
                SetRestoreButton(btnAnTheLoai, item?.IsDeleted == true, "thể loại");
            };

            var listCard = new CardPanel("Danh sách thể loại", autoHeight: false);
            listCard.AddRow(toolbar);
            listCard.AddRow(dgvTheLoai, fill: true);
            return Ui.ScrollStack(280, formCard, listCard);
        }

        private Control BuildTacGiaPage()
        {
            txtMaTacGia = Ui.ReadOnlyBox();
            txtTenTacGia = Ui.TextBox();
            txtQuocTich = Ui.TextBox();
            txtGioiThieuTG = Ui.MultilineBox(64);

            var fields = new FormGrid(4);
            fields.Add("Mã tác giả", txtMaTacGia);
            fields.Add("Tên tác giả", txtTenTacGia, 2);
            fields.Add("Quốc tịch", txtQuocTich);
            fields.Add("Giới thiệu", txtGioiThieuTG, 4);

            var btnThem = Ui.Button("Thêm tác giả", ButtonKind.Primary);
            var btnSua = Ui.Button("Cập nhật");
            btnAnTacGia = Ui.Button("Ẩn tác giả", ButtonKind.Danger);
            var btnMoi = Ui.Button("Làm mới");
            btnThem.Click += BtnThemTG_Click;
            btnSua.Click += BtnSuaTG_Click;
            btnAnTacGia.Click += BtnXoaTG_Click;
            btnMoi.Click += (_, _) => ClearTacGia();

            var formCard = new CardPanel("Thông tin tác giả");
            formCard.AddRow(fields);
            formCard.AddRow(Ui.ButtonBar(btnThem, btnSua, btnAnTacGia, btnMoi));

            txtTimTacGia = Ui.TextBox("Tên tác giả, quốc tịch hoặc giới thiệu", 300);
            txtTimTacGia.TextChanged += (_, _) => QueueSearch(false);
            cboTrangThaiTacGia = CreateStatusFilter();
            cboTrangThaiTacGia.SelectedIndexChanged += (_, _) => QueueSearch(false);
            var toolbar = new ToolbarRow();
            toolbar.AddField("Tìm kiếm", txtTimTacGia);
            toolbar.AddField("Trạng thái", cboTrangThaiTacGia);

            dgvTacGia = Ui.Grid();
            dgvTacGia.Columns.Add("MaTG", "Mã");
            dgvTacGia.Columns["MaTG"].FillWeight = 45;
            dgvTacGia.Columns.Add("TenTG", "Tên tác giả");
            dgvTacGia.Columns["TenTG"].FillWeight = 120;
            dgvTacGia.Columns.Add("QuocTich", "Quốc tịch");
            dgvTacGia.Columns["QuocTich"].FillWeight = 80;
            dgvTacGia.Columns.Add("GioiThieu", "Giới thiệu");
            dgvTacGia.Columns["GioiThieu"].FillWeight = 140;
            dgvTacGia.Columns.Add("TrangThai", "Trạng thái");
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
                SetRestoreButton(btnAnTacGia, item?.IsDeleted == true, "tác giả");
            };

            var listCard = new CardPanel("Danh sách tác giả", autoHeight: false);
            listCard.AddRow(toolbar);
            listCard.AddRow(dgvTacGia, fill: true);
            return Ui.ScrollStack(280, formCard, listCard);
        }

        private static ComboBox CreateStatusFilter() =>
            Ui.ComboBox(new object[] { "Tất cả", "Đang hoạt động", "Đã ẩn" }, width: 180);

        private static void SetRestoreButton(AppButton button, bool isDeleted, string entity)
        {
            button.Text = isDeleted ? $"Khôi phục {entity}" : $"Ẩn {entity}";
            button.Kind = isDeleted ? ButtonKind.Success : ButtonKind.Danger;
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

        private async void BtnThemTL_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTheLoai.Text)) { WarnRequired("Tên thể loại"); return; }
            try
            {
                await _api.CreateTheLoaiAsync(new TheLoaiWriteDto { TenTheLoai = txtTenTheLoai.Text, MoTa = txtMoTaTL.Text }, CancellationToken.None);
                ClearTheLoai(); await RefreshTheLoaiAsync(CancellationToken.None); ShowSuccess("Đã thêm thể loại.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnSuaTL_Click(object? sender, EventArgs e)
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

        private async void BtnXoaTL_Click(object? sender, EventArgs e)
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

        private async void BtnThemTG_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenTacGia.Text)) { WarnRequired("Tên tác giả"); return; }
            try
            {
                await _api.CreateTacGiaAsync(new TacGiaWriteDto { TenTacGia = txtTenTacGia.Text, QuocTich = txtQuocTich.Text, GioiThieu = txtGioiThieuTG.Text }, CancellationToken.None);
                ClearTacGia(); await RefreshTacGiaAsync(CancellationToken.None); ShowSuccess("Đã thêm tác giả.");
            }
            catch (Exception ex) { ShowApiError(ex); }
        }

        private async void BtnSuaTG_Click(object? sender, EventArgs e)
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

        private async void BtnXoaTG_Click(object? sender, EventArgs e)
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
            SetRestoreButton(btnAnTheLoai, false, "thể loại");
        }

        private void ClearTacGia()
        {
            txtMaTacGia.Clear(); txtTenTacGia.Clear(); txtQuocTich.Clear(); txtGioiThieuTG.Clear(); dgvTacGia.ClearSelection();
            SetRestoreButton(btnAnTacGia, false, "tác giả");
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
