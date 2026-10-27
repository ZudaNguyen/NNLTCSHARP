using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Forms;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

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
        _historyReader = Ui.ComboBox(style: ComboBoxStyle.DropDown, width: 300);
        _historyReader.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _historyReader.AutoCompleteSource = AutoCompleteSource.ListItems;
        _historyReader.DisplayMember = nameof(HistoryReaderOption.Label);
        _historyReader.ValueMember = nameof(HistoryReaderOption.Id);

        _historyStatus = Ui.ComboBox(new object[] { "Tất cả", "Đang mượn", "Đã trả", "Quá hạn" }, width: 160);
        _historySearch = Ui.TextBox("PM-000001", 150);
        _historyFrom = CreateDateFilter();
        _historyTo = CreateDateFilter();

        var searchButton = Ui.Button("Tra cứu", ButtonKind.Primary);
        searchButton.Click += async (_, _) => await LoadHistoryAsync(forceQuery: true);
        var refreshButton = Ui.Button("Làm mới");
        refreshButton.Click += async (_, _) =>
        {
            _historyReader.SelectedValue = 0;
            _historySearch.Clear();
            _historyStatus.SelectedIndex = 0;
            _historyFrom.Checked = false;
            _historyTo.Checked = false;
            await LoadHistoryAsync(forceQuery: true);
        };
        var detailButton = Ui.Button("Xem chi tiết");
        detailButton.Click += async (_, _) => await ViewHistoryDetailAsync();

        var toolbar = new ToolbarRow();
        toolbar.AddField("Độc giả", _historyReader);
        toolbar.AddField("Trạng thái", _historyStatus);
        toolbar.AddField("Mã phiếu", _historySearch);
        var dateBar = new ToolbarRow();
        dateBar.AddField("Từ ngày", _historyFrom);
        dateBar.AddField("Đến ngày", _historyTo);
        dateBar.AddActions(searchButton, refreshButton, detailButton);

        _historyGrid = Ui.Grid();
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

        _historyCount = new Label
        {
            Text = "Tổng số: 0 phiếu", AutoSize = true, Font = UiTheme.Font(9.5f, FontStyle.Bold),
            ForeColor = UiTheme.TextSecondary, Margin = new Padding(0, 10, 0, 0)
        };

        var card = new CardPanel("Lịch sử mượn sách", autoHeight: false);
        card.AddRow(toolbar);
        card.AddRow(dateBar);
        card.AddRow(_historyGrid, fill: true);
        card.AddRow(_historyCount);
        return card;
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

    /// <summary>Ô ngày có hộp chọn: bỏ chọn nghĩa là không lọc theo ngày.</summary>
    private static DateTimePicker CreateDateFilter()
    {
        var picker = Ui.DatePicker(DateTime.Today, 150);
        picker.ShowCheckBox = true;
        picker.Checked = false;
        return picker;
    }

    private sealed record HistoryReaderOption(int Id, string Label);
}
