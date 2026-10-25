using System.Drawing;
using System.Net;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

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
        var paperCream = Color.FromArgb(247, 243, 233);
        var cardBg = Color.FromArgb(255, 252, 245);
        var woodDark = Color.FromArgb(44, 30, 22);
        Dock = DockStyle.Fill;
        BackColor = paperCream;
        Padding = new Padding(20);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = paperCream,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = paperCream,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        for (var i = 0; i < 5; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cards.Controls.Add(CreateLibraryCard("ĐẦU SÁCH", "Đang hoạt động", Color.FromArgb(102, 51, 20)), 0, 0);
        cards.Controls.Add(CreateLibraryCard("TỔNG BẢN", "Bản còn trong kho", Color.FromArgb(180, 120, 20)), 1, 0);
        cards.Controls.Add(CreateLibraryCard("ĐANG MƯỢN", "Số bản chưa trả", Color.FromArgb(45, 106, 79)), 2, 0);
        cards.Controls.Add(CreateLibraryCard("PHIẾU QUÁ HẠN", "Đang cần xử lý", Color.FromArgb(158, 42, 43)), 3, 0);
        cards.Controls.Add(CreateLibraryCard("ĐỘC GIẢ HOẠT ĐỘNG", "Thẻ còn hiệu lực", Color.FromArgb(91, 83, 133)), 4, 0);

        var workspace = new Panel { Dock = DockStyle.Fill, BackColor = cardBg, Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle };
        var header = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = cardBg };
        var title = new Label
        {
            Text = "Sổ theo dõi sách đang lưu hành, ưu tiên sắp đến hạn và quá hạn",
            Font = new Font("Georgia", 11f, FontStyle.Bold), ForeColor = woodDark,
            Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft
        };
        _refresh = new Button
        {
            Text = "Làm mới", Dock = DockStyle.Right, Width = 105,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = Color.FromArgb(45, 106, 79),
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        _refresh.FlatAppearance.BorderSize = 0;
        _refresh.Click += async (_, _) => await LoadDashboardAsync();
        header.Controls.Add(title);
        header.Controls.Add(_refresh);

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f), Padding = new Point(12, 5) };
        var activityPage = new TabPage("Đang mượn / Quá hạn") { BackColor = cardBg, Padding = new Padding(5) };
        var topBooksPage = new TabPage("Top 5 sách mượn nhiều") { BackColor = cardBg, Padding = new Padding(5) };
        _activityGrid = CreateLibraryGrid(cardBg);
        _activityGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        AddActivityColumns(_activityGrid);
        _activityStatus = CreateStatusLabel();
        activityPage.Controls.Add(_activityGrid);
        activityPage.Controls.Add(_activityStatus);

        var topHeader = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = cardBg, Padding = new Padding(4, 0, 4, 0) };
        var topTitle = new Label
        {
            Text = "TOP 5 SÁCH ĐƯỢC MƯỢN NHIỀU NHẤT",
            Font = new Font("Georgia", 10.5f, FontStyle.Bold), ForeColor = woodDark,
            Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft
        };
        _topBooksPeriod = new ComboBox
        {
            Dock = DockStyle.Right, Width = 155, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5f), AccessibleName = "Khoảng thời gian Top 5"
        };
        _topBooksPeriod.Items.AddRange(["Tháng này", "Toàn thời gian"]);
        _topBooksPeriod.SelectedIndex = 0;
        _topBooksPeriod.SelectedIndexChanged += async (_, _) => await LoadTopBooksAsync();
        topHeader.Controls.Add(topTitle);
        topHeader.Controls.Add(_topBooksPeriod);

        _topBooksGrid = CreateLibraryGrid(cardBg);
        _topBooksGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
        topBooksPage.Controls.Add(_topBooksGrid);
        topBooksPage.Controls.Add(_topBooksStatus);
        topBooksPage.Controls.Add(topHeader);

        tabs.TabPages.Add(activityPage);
        tabs.TabPages.Add(topBooksPage);
        workspace.Controls.Add(tabs);
        workspace.Controls.Add(header);
        layout.Controls.Add(cards, 0, 0);
        layout.Controls.Add(workspace, 0, 1);
        Controls.Add(layout);
    }

    private static DataGridView CreateLibraryGrid(Color cardBg)
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, BackgroundColor = cardBg, BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
            EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 42,
            RowTemplate = { Height = 34 }, GridColor = Color.FromArgb(225, 215, 195)
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
        DataGridViewStyleHelper.ApplyLibraryStyle(grid);
        return grid;
    }

    private static void AddActivityColumns(DataGridView grid)
    {
        grid.Columns.Add("MaPhieu", "Mã Phiếu");
        grid.Columns.Add("DocGia", "Độc Giả");
        grid.Columns.Add("TenSach", "Ấn Phẩm Đang Giữ");
        grid.Columns.Add("SoLuong", "SL Còn Mượn");
        grid.Columns.Add("NgayMuon", "Ngày Mượn");
        grid.Columns.Add("HanTra", "Hạn Trả");
        grid.Columns.Add("TrangThai", "Tình Trạng");
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
        Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft,
        ForeColor = Color.FromArgb(122, 40, 34), Font = new Font("Segoe UI", 9.5f), AutoEllipsis = true
    };

    private Panel CreateLibraryCard(string title, string subtitle, Color spineColor)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(255, 252, 245), Margin = new Padding(3, 2, 5, 7), BorderStyle = BorderStyle.FixedSingle };
        var spine = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = spineColor };
        var titleLabel = new Label
        {
            Text = title, Font = new Font("Georgia", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(90, 70, 55),
            Location = new Point(14, 8), Size = new Size(150, 28), AutoEllipsis = true
        };
        var value = new Label { Text = "—", Font = new Font("Georgia", 19, FontStyle.Bold), ForeColor = spineColor, Location = new Point(14, 39), AutoSize = true };
        var sub = new Label
        {
            Text = subtitle, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(90, 85, 78),
            Location = new Point(14, 91), Size = new Size(150, 20), AutoEllipsis = true
        };
        card.SizeChanged += (_, _) =>
        {
            var width = Math.Max(30, card.ClientSize.Width - 28);
            titleLabel.Width = width;
            sub.Width = width;
            value.MaximumSize = new Size(width, 36);
        };
        _cardValues.Add(value);
        _cardSubtitles.Add(sub);
        card.Controls.Add(sub);
        card.Controls.Add(value);
        card.Controls.Add(titleLabel);
        card.Controls.Add(spine);
        return card;
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
