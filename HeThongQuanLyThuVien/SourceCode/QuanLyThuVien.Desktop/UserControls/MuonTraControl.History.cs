using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Forms;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls;

public partial class MuonTraControl
{
    private readonly List<HistoryReaderOption> _historyReaders = [];
    private ComboBox _historyReader = null!;
    private TextBox _historySearch = null!;
    private ComboBox _historyStatus = null!;
    private DateTimePicker _historyFrom = null!;
    private DateTimePicker _historyTo = null!;
    private DataGridView _historyGrid = null!;
    private Label _historyCount = null!;
    private bool _historyLoaded;
    private bool _loadingHistory;

    private Control CreateHistoryView()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
            BackColor = PaperCream, Padding = new Padding(2),
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 136));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        var title = new Label
        {
            Text = "LỊCH SỬ MƯỢN SÁCH", Dock = DockStyle.Fill,
            Font = new Font("Georgia", 12, FontStyle.Bold), ForeColor = Burgundy,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
        };
        layout.Controls.Add(title, 0, 0);

        var filters = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
            BackColor = CardBg, Padding = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle
        };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < 3; row++) filters.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
        var firstLine = CreateFilterLine();
        _historyReader = new ComboBox
        {
            Width = 250, DropDownStyle = ComboBoxStyle.DropDown,
            Font = new Font("Segoe UI", 9.5f), Margin = new Padding(4, 3, 12, 3)
        };
        _historyReader.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _historyReader.AutoCompleteSource = AutoCompleteSource.ListItems;
        _historyReader.DisplayMember = nameof(HistoryReaderOption.Label);
        _historyReader.ValueMember = nameof(HistoryReaderOption.Id);
        firstLine.Controls.Add(CreateFilterLabel("Độc giả:", 62));
        firstLine.Controls.Add(_historyReader);
        _historyStatus = new ComboBox
        {
            Width = 150, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5f), Margin = new Padding(4, 3, 4, 3)
        };
        _historyStatus.Items.AddRange(["Tất cả", "Đang mượn", "Đã trả", "Quá hạn"]);
        _historyStatus.SelectedIndex = 0;
        firstLine.Controls.Add(CreateFilterLabel("Trạng thái:", 82));
        firstLine.Controls.Add(_historyStatus);
        filters.Controls.Add(firstLine, 0, 0);

        var secondLine = CreateFilterLine();
        _historySearch = new TextBox
        {
            Width = 170, Font = new Font("Segoe UI", 9.5f),
            Margin = new Padding(4, 5, 12, 3), PlaceholderText = "PM-000001"
        };
        secondLine.Controls.Add(CreateFilterLabel("Mã phiếu:", 75));
        secondLine.Controls.Add(_historySearch);
        _historyFrom = CreateDateFilter();
        _historyTo = CreateDateFilter();
        secondLine.Controls.Add(CreateFilterLabel("Từ ngày:", 68));
        secondLine.Controls.Add(_historyFrom);
        secondLine.Controls.Add(CreateFilterLabel("Đến ngày:", 76));
        secondLine.Controls.Add(_historyTo);
        filters.Controls.Add(secondLine, 0, 1);

        var actionLine = CreateFilterLine();
        var searchButton = CreateHistoryButton("Tra cứu", Burgundy, 90);
        searchButton.Click += async (_, _) => await LoadHistoryAsync(forceQuery: true);
        var refreshButton = CreateHistoryButton("Làm mới", Color.FromArgb(90, 75, 65), 95);
        refreshButton.Click += async (_, _) =>
        {
            _historyReader.SelectedValue = 0;
            _historySearch.Clear();
            _historyStatus.SelectedIndex = 0;
            _historyFrom.Checked = false;
            _historyTo.Checked = false;
            await LoadHistoryAsync(forceQuery: true);
        };
        var detailButton = CreateHistoryButton("Xem chi tiết", Color.FromArgb(100, 67, 42), 120);
        detailButton.Click += async (_, _) => await ViewHistoryDetailAsync();
        actionLine.Controls.Add(searchButton);
        actionLine.Controls.Add(refreshButton);
        actionLine.Controls.Add(detailButton);
        filters.Controls.Add(actionLine, 0, 2);
        layout.Controls.Add(filters, 0, 1);

        _historyGrid = new DataGridView
        {
            Dock = DockStyle.Fill, BackgroundColor = CardBg, BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
            EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9.5f),
            ColumnHeadersHeight = 42, RowTemplate = { Height = 32 },
            GridColor = Color.FromArgb(225, 215, 195)
        };
        _historyGrid.Columns.Add("SoPhieu", "Mã phiếu");
        _historyGrid.Columns.Add("DocGia", "Độc giả");
        _historyGrid.Columns.Add("NgayMuon", "Ngày mượn");
        _historyGrid.Columns.Add("HanTra", "Hạn trả");
        _historyGrid.Columns.Add("SoLuongConMuon", "Còn mượn");
        _historyGrid.Columns.Add("TrangThai", "Trạng thái");
        _historyGrid.Columns[0].FillWeight = 72;
        _historyGrid.Columns[1].FillWeight = 150;
        _historyGrid.Columns[2].FillWeight = 90;
        _historyGrid.Columns[3].FillWeight = 90;
        _historyGrid.Columns[4].FillWeight = 82;
        _historyGrid.Columns[5].FillWeight = 100;
        _historyGrid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0) await ViewHistoryDetailAsync();
        };
        DataGridViewStyleHelper.ApplyLibraryStyle(_historyGrid);
        layout.Controls.Add(_historyGrid, 0, 2);

        _historyCount = new Label
        {
            Dock = DockStyle.Fill, Text = "Tổng số: 0 phiếu",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
        };
        layout.Controls.Add(_historyCount, 0, 3);
        return layout;
    }

    private async Task LoadHistoryAsync(bool forceQuery = false)
    {
        if (_loadingHistory || (_historyLoaded && !forceQuery)) return;
        _loadingHistory = true;
        try
        {
            Cursor = Cursors.WaitCursor;
            _historyCount.Text = "Đang tải lịch sử mượn…";
            if (!_historyLoaded)
            {
                // API hiện chưa phân trang; lấy danh sách lịch sử một lần để tạo lựa chọn độc giả,
                // sau đó các lần lọc tiếp theo dùng endpoint/query lọc đã có.
                var allLoans = await _api.SearchHistoryAsync(null, null, null, null, CancellationToken.None);
                _historyReaders.Clear();
                _historyReaders.Add(new HistoryReaderOption(0, "Tất cả độc giả"));
                _historyReaders.AddRange(allLoans
                    .GroupBy(x => x.MaDocGia)
                    .Select(g => g.First())
                    .OrderBy(x => x.TenDocGia)
                    .Select(x => new HistoryReaderOption(x.MaDocGia, $"{x.TenDocGia} · {x.MaDocGia}")));
                _historyReader.DataSource = _historyReaders;
                _historyReader.SelectedValue = 0;
                _historyLoaded = true;
                if (string.IsNullOrWhiteSpace(_historySearch.Text) && _historyStatus.SelectedIndex == 0 &&
                    !_historyFrom.Checked && !_historyTo.Checked)
                {
                    ShowHistoryRows(allLoans);
                    return;
                }
            }

            var search = _historySearch.Text.Trim();
            var status = GetHistoryStatusQuery();
            DateTime? from = _historyFrom.Checked ? _historyFrom.Value.Date : null;
            DateTime? to = _historyTo.Checked ? _historyTo.Value.Date : null;
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                MessageBox.Show("Từ ngày không được sau đến ngày.", "Khoảng thời gian không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            List<MuonTraDto> loans;
            if (_historyReader.SelectedValue is int readerId && readerId > 0)
            {
                loans = await _api.GetReaderLoansAsync(readerId, CancellationToken.None);
                loans = loans.Where(x =>
                    (string.IsNullOrWhiteSpace(search) || x.SoPhieu.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
                    (status is null || string.Equals(x.TrangThai, status, StringComparison.OrdinalIgnoreCase)) &&
                    (!from.HasValue || x.NgayMuon.Date >= from.Value) &&
                    (!to.HasValue || x.NgayMuon.Date <= to.Value)).ToList();
            }
            else
            {
                loans = await _api.SearchHistoryAsync(search, status, from, to, CancellationToken.None);
            }
            ShowHistoryRows(loans);
        }
        catch (Exception ex)
        {
            _historyCount.Text = "Không tải được lịch sử mượn.";
            ShowApiError(ex);
        }
        finally { _loadingHistory = false; Cursor = Cursors.Default; }
    }

    private string? GetHistoryStatusQuery() => _historyStatus.SelectedItem?.ToString() switch
    {
        "Đang mượn" => "Đang mượn",
        "Đã trả" => "Đã hoàn tất",
        "Quá hạn" => "Quá hạn",
        _ => null
    };

    private void ShowHistoryRows(IEnumerable<MuonTraDto> loans)
    {
        var rows = loans.OrderByDescending(x => x.NgayMuon).ThenByDescending(x => x.MaPhieu).ToList();
        _historyGrid.Rows.Clear();
        foreach (var loan in rows)
        {
            var row = _historyGrid.Rows.Add(loan.SoPhieu, loan.TenDocGia,
                loan.NgayMuon.ToString("dd/MM/yyyy"), loan.HanTra.ToString("dd/MM/yyyy"),
                loan.SoLuongConMuon, loan.TrangThai);
            _historyGrid.Rows[row].Tag = loan.MaPhieu;
        }
        _historyCount.Text = $"Tổng số: {rows.Count} phiếu";
    }

    private async Task ViewHistoryDetailAsync()
    {
        if (_historyGrid.CurrentRow?.Tag is not int loanId)
        {
            MessageBox.Show("Chọn một phiếu mượn để xem chi tiết.", "Lịch sử mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            var loan = await _api.GetAsync(loanId, CancellationToken.None);
            using var dialog = new MuonTraHistoryDetailDialog(loan);
            dialog.ShowDialog(this);
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private static FlowLayoutPanel CreateFilterLine() => new()
    {
        Dock = DockStyle.Fill, WrapContents = true, AutoScroll = false,
        FlowDirection = FlowDirection.LeftToRight, Margin = Padding.Empty,
        Padding = new Padding(0, 2, 0, 0)
    };

    private static Label CreateFilterLabel(string text, int width) => new()
    {
        Text = text, Width = width, Height = 30, Margin = new Padding(3, 2, 2, 2),
        TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(44, 30, 22)
    };

    private static DateTimePicker CreateDateFilter() => new()
    {
        Width = 135, Height = 30, Format = DateTimePickerFormat.Short,
        ShowCheckBox = true, Font = new Font("Segoe UI", 9.5f),
        Margin = new Padding(3, 3, 12, 2)
    };

    private static Button CreateHistoryButton(string text, Color color, int width)
    {
        var button = new Button
        {
            Text = text, Width = width, Height = 30, Margin = new Padding(4, 2, 4, 2),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = color,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private sealed record HistoryReaderOption(int Id, string Label);
}
