using System.Drawing;
using System.Net;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls;

public partial class DashboardControl : UserControl
{
    private readonly ThongKeApiClient _api = new();
    private readonly List<Label> _cardValues = [];
    private readonly List<Label> _cardSubtitles = [];
    private DataGridView _activityGrid = null!;
    private DataGridView _topBooksGrid = null!;
    private Label _activityStatus = null!;
    private Label _topBooksStatus = null!;
    private Button _refresh = null!;
    private ComboBox _topBooksPeriod = null!;

    public DashboardControl()
    {
        InitializeComponent();
        SetupLibraryDashboardUI();
        Load += async (_, _) => await LoadDashboardAsync();
    }

    private void SetupLibraryDashboardUI()
    {
        // ----- Các thẻ số liệu -----
        var cards = new TableLayoutPanel
        {
            ColumnCount = 5, RowCount = 1, BackColor = UiTheme.PageBg, AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = Padding.Empty, Padding = Padding.Empty
        };
        for (var i = 0; i < 5; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        cards.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        AddCard(cards, 0, "Đầu sách", "Đang hoạt động", UiTheme.Primary);
        AddCard(cards, 1, "Tổng số bản", "Bản còn trong kho", UiTheme.Primary);
        AddCard(cards, 2, "Đang mượn", "Số bản chưa trả", UiTheme.Warning);
        AddCard(cards, 3, "Phiếu quá hạn", "Cần xử lý", UiTheme.Danger);
        AddCard(cards, 4, "Độc giả hoạt động", "Thẻ còn hiệu lực", UiTheme.Success);

        // ----- Khu theo dõi -----
        _refresh = Ui.Button("Làm mới");
        _refresh.Margin = Padding.Empty;
        _refresh.Click += async (_, _) => await LoadDashboardAsync();
        var title = new Label
        {
            Text = "Theo dõi sách đang lưu hành", AutoSize = true, Font = UiTheme.Heading, ForeColor = UiTheme.Text,
            Anchor = AnchorStyles.Left, Margin = Padding.Empty
        };
        var header = new TableLayoutPanel
        {
            ColumnCount = 2, RowCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiTheme.Surface, Margin = new Padding(0, 0, 0, 8), Padding = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(_refresh, 1, 0);

        _activityGrid = Ui.Grid();
        AddActivityColumns(_activityGrid);
        _activityStatus = CreateStatusLabel();
        var activityPage = StackPage(_activityGrid, _activityStatus);

        _topBooksPeriod = Ui.ComboBox(new object[] { "Tháng này", "Toàn thời gian" }, width: 170);
        _topBooksPeriod.AccessibleName = "Khoảng thời gian Top 5";
        _topBooksPeriod.SelectedIndexChanged += async (_, _) => await LoadTopBooksAsync();
        var periodBar = new ToolbarRow { Dock = DockStyle.Fill };
        periodBar.AddField("Thống kê theo", _topBooksPeriod);

        _topBooksGrid = Ui.Grid();
        _topBooksGrid.Columns.Add("Rank", "Hạng");
        _topBooksGrid.Columns.Add("MaSach", "Mã sách");
        _topBooksGrid.Columns.Add("TenSach", "Tên sách");
        _topBooksGrid.Columns.Add("SoLuotMuon", "Số lượt mượn");
        _topBooksGrid.Columns[0].FillWeight = 45;
        _topBooksGrid.Columns[1].FillWeight = 60;
        _topBooksGrid.Columns[2].FillWeight = 220;
        _topBooksGrid.Columns[3].FillWeight = 85;
        _topBooksGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _topBooksGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _topBooksGrid.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _topBooksGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _topBooksStatus = CreateStatusLabel();
        var topBooksPage = StackPage(_topBooksGrid, _topBooksStatus, periodBar);

        var tabs = new PageTabs(UiTheme.Surface);
        tabs.AddPage("Đang mượn / Quá hạn", activityPage);
        tabs.AddPage("Top 5 sách mượn nhiều", topBooksPage);

        var workspace = new CardPanel(null, autoHeight: false);
        workspace.AddRow(header);
        workspace.AddRow(tabs, fill: true);

        Ui.SetupPage(this, Ui.Stack(cards, workspace));
    }

    /// <summary>Trang gồm (tùy chọn) thanh phía trên, bảng ở giữa và dòng trạng thái ở dưới.</summary>
    private static Control StackPage(DataGridView grid, Label status, Control? top = null)
    {
        var page = new TableLayoutPanel
        {
            ColumnCount = 1, BackColor = UiTheme.Surface, Margin = Padding.Empty, Padding = Padding.Empty
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        if (top != null)
        {
            page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            page.Controls.Add(top, 0, page.RowStyles.Count - 1);
        }
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.Controls.Add(grid, 0, page.RowStyles.Count - 1);
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        page.Controls.Add(status, 0, page.RowStyles.Count - 1);
        page.RowCount = page.RowStyles.Count;
        return page;
    }

    private static void AddActivityColumns(DataGridView grid)
    {
        grid.Columns.Add("MaPhieu", "Mã phiếu");
        grid.Columns.Add("DocGia", "Độc giả");
        grid.Columns.Add("TenSach", "Sách đang giữ");
        grid.Columns.Add("SoLuong", "SL còn mượn");
        grid.Columns.Add("NgayMuon", "Ngày mượn");
        grid.Columns.Add("HanTra", "Hạn trả");
        grid.Columns.Add("TrangThai", "Tình trạng");
        grid.Columns[0].FillWeight = 62;
        grid.Columns[1].FillWeight = 105;
        grid.Columns[2].FillWeight = 160;
        grid.Columns[3].FillWeight = 75;
        grid.Columns[4].FillWeight = 80;
        grid.Columns[5].FillWeight = 80;
        grid.Columns[6].FillWeight = 100;
        grid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
    }

    private static Label CreateStatusLabel() => new()
    {
        Dock = DockStyle.Fill, AutoSize = false, MinimumSize = new Size(0, 30), TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = UiTheme.TextMuted, Font = UiTheme.Font(9.5f), AutoEllipsis = true, Margin = new Padding(0, 6, 0, 0)
    };

    /// <summary>Thẻ số liệu: vạch màu bên trái, nhãn nhỏ, số lớn, ghi chú.</summary>
    private void AddCard(TableLayoutPanel parent, int column, string title, string subtitle, Color accent)
    {
        var card = new CardPanel(null, autoHeight: true)
        {
            Dock = DockStyle.Fill, Padding = new Padding(18, 14, 12, 12),
            Margin = new Padding(0, 0, column < 4 ? 14 : 0, 0)
        };
        var titleLabel = new Label
        {
            Text = title.ToUpperInvariant(), AutoSize = true, Font = UiTheme.Font(9, FontStyle.Bold),
            ForeColor = UiTheme.TextMuted, Margin = new Padding(0, 0, 0, 2)
        };
        var value = new Label
        {
            Text = "—", AutoSize = true, Font = UiTheme.Font(18, FontStyle.Bold), ForeColor = accent,
            Margin = new Padding(0, 0, 0, 2)
        };
        var sub = new Label
        {
            Text = subtitle, AutoSize = true, Font = UiTheme.Font(9), ForeColor = UiTheme.TextMuted, Margin = Padding.Empty
        };
        card.AddRow(titleLabel);
        card.AddRow(value);
        card.AddRow(sub);
        _cardValues.Add(value);
        _cardSubtitles.Add(sub);

        // Vạch màu bên trái, vẽ trên chính thẻ
        card.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(accent);
            e.Graphics.FillRectangle(brush, 0, 0, Ui.Px(4), card.Height);
        };
        parent.Controls.Add(card, column, 0);
    }

    private async Task LoadDashboardAsync()
    {
        if (_refresh is null || _refresh.IsDisposed) return;
        _refresh.Enabled = false;
        _activityStatus.Text = "Đang tải số liệu từ API...";
        try
        {
            var summaryTask = _api.GetDashboardTongQuanAsync(CancellationToken.None);
            var activityTask = _api.GetDashboardHoatDongAsync(CancellationToken.None);
            var topBooksTask = GetTopBooksRangeAsync(CancellationToken.None);
            await Task.WhenAll(summaryTask, activityTask, topBooksTask);
            var summary = await summaryTask;
            var activity = await activityTask;
            var topBooks = await topBooksTask;

            _cardValues[0].Text = summary.SoDauSach.ToString("N0");
            _cardValues[1].Text = summary.TongSoBanSach.ToString("N0");
            _cardSubtitles[1].Text = $"{summary.SoBanConTrong:N0} bản còn trong kho";
            _cardValues[2].Text = summary.SoBanDangMuon.ToString("N0");
            _cardValues[3].Text = summary.SoPhieuQuaHan.ToString("N0");
            _cardValues[4].Text = summary.SoDocGiaHoatDong.ToString("N0");

            _activityGrid.Rows.Clear();
            foreach (var item in activity)
                _activityGrid.Rows.Add($"PM-{item.MaPhieu:D6}", item.TenDocGia, item.TenSach, item.SoLuongConMuon,
                    item.NgayMuon.ToString("dd/MM/yyyy"), item.HanTra.ToString("dd/MM/yyyy"), item.TrangThai);
            _activityStatus.Text = activity.Count == 0
                ? $"Đang mượn: {summary.SoPhieuDangMuon:N0} phiếu • Lượt mượn trong tháng: {summary.SoLuotMuonTrongThang:N0} • Hiện không có sách đang mượn hoặc quá hạn."
                : $"Đang mượn: {summary.SoPhieuDangMuon:N0} phiếu • Lượt mượn trong tháng: {summary.SoLuotMuonTrongThang:N0} • Hiển thị {activity.Count:N0} dòng gần hạn/quá hạn.";
            PopulateTopBooks(topBooks);
        }
        catch (HttpRequestException ex)
        {
            _activityStatus.Text = ex.Message;
            _topBooksStatus.Text = ex.Message;
            MessageBox.Show(ex.Message, "Lỗi tải Dashboard", MessageBoxButtons.OK, ex.StatusCode == HttpStatusCode.Forbidden ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            _activityStatus.Text = "Không thể tải Dashboard.";
            _topBooksStatus.Text = "Không thể tải Top 5 sách.";
            MessageBox.Show($"Không thể tải số liệu Dashboard: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { if (!_refresh.IsDisposed) _refresh.Enabled = true; }
    }

    private async Task LoadTopBooksAsync()
    {
        if (_topBooksPeriod is null || _topBooksGrid is null || !_topBooksPeriod.IsHandleCreated) return;
        _topBooksPeriod.Enabled = false;
        _topBooksStatus.Text = "Đang tải Top 5 từ API...";
        try { PopulateTopBooks(await GetTopBooksRangeAsync(CancellationToken.None)); }
        catch (HttpRequestException ex)
        {
            _topBooksStatus.Text = ex.Message;
            MessageBox.Show(ex.Message, "Lỗi tải Top 5", MessageBoxButtons.OK, ex.StatusCode == HttpStatusCode.Forbidden ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            _topBooksStatus.Text = "Không thể tải Top 5 sách.";
            MessageBox.Show($"Không thể tải Top 5 sách: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { if (!_topBooksPeriod.IsDisposed) _topBooksPeriod.Enabled = true; }
    }

    private Task<IReadOnlyList<SachMuonNhieuDto>> GetTopBooksRangeAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var from = _topBooksPeriod?.SelectedIndex == 1 ? DateOnly.MinValue : new DateOnly(today.Year, today.Month, 1);
        return _api.GetSachMuonNhieuAsync(from, today, 5, ct);
    }

    private void PopulateTopBooks(IReadOnlyList<SachMuonNhieuDto> books)
    {
        _topBooksGrid.Rows.Clear();
        foreach (var book in books.OrderBy(x => x.XepHang))
            _topBooksGrid.Rows.Add(book.XepHang, book.MaSach, book.TenSach, book.SoLuotMuon);

        var period = _topBooksPeriod.SelectedIndex == 1 ? "toàn thời gian" : "tháng này";
        _topBooksStatus.Text = books.Count == 0
            ? $"Chưa có dữ liệu mượn {period}."
            : $"Xếp theo số lượt mượn • Bản mượn: tổng số bản trong các lượt • Thời gian: {period}.";
    }
}
