using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class LichSuMuonDocGiaDialog : Form
{
    private readonly MuonTraApiClient _api = new();
    private readonly int _readerId;
    private readonly string _readerName;
    private readonly DataGridView _loans = Ui.Grid();
    private readonly DataGridView _books = Ui.Grid();
    private readonly Label _summary = new();
    private bool _loading;

    public LichSuMuonDocGiaDialog(int readerId, string readerName)
    {
        _readerId = readerId;
        _readerName = readerName;
        Ui.SetupDialog(this, $"Lịch sử mượn - {readerName}", new Size(1020, 640), new Size(860, 560));

        var title = Ui.Heading($"Lịch sử mượn · {_readerName} · Mã {_readerId}");
        title.Font = UiTheme.Title;
        title.Margin = new Padding(0, 0, 0, 14);

        _loans.Columns.Add("SoPhieu", "Phiếu");
        _loans.Columns.Add("NgayMuon", "Ngày mượn");
        _loans.Columns.Add("HanTra", "Hạn trả");
        _loans.Columns.Add("SoLuongConMuon", "Còn mượn");
        _loans.Columns.Add("TrangThai", "Trạng thái");
        _loans.SelectionChanged += async (_, _) =>
        {
            if (!_loading && _loans.CurrentRow?.Tag is int loanId)
                await LoadLoanDetailAsync(loanId);
        };
        var loansCard = new CardPanel("Danh sách phiếu mượn", autoHeight: false);
        loansCard.AddRow(_loans, fill: true);

        _books.Columns.Add("Sach", "Sách");
        _books.Columns.Add("SoLuong", "Mượn / Đã trả / Còn");
        _books.Columns.Add("LichSuTra", "Ngày trả · Tình trạng");
        _books.Columns.Add("TienPhat", "Phải thu / Đã thu / Còn nợ (đ)");
        var booksCard = new CardPanel("Sách trong phiếu đang chọn", autoHeight: false);
        booksCard.AddRow(_books, fill: true);

        _summary.AutoSize = true;
        _summary.MaximumSize = new Size(980, 0);
        _summary.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        _summary.ForeColor = UiTheme.TextSecondary;
        _summary.Margin = new Padding(0, 12, 0, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = UiTheme.PageBg,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        loansCard.Dock = DockStyle.Fill;
        loansCard.Margin = new Padding(0, 0, 0, 14);
        booksCard.Dock = DockStyle.Fill;
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(loansCard, 0, 1);
        layout.Controls.Add(booksCard, 0, 2);
        layout.Controls.Add(_summary, 0, 3);
        Controls.Add(layout);
        Ui.ApplyDpi(this);

        Load += async (_, _) => await LoadLoansAsync();
    }

    private async Task LoadLoansAsync()
    {
        try
        {
            var items = await _api.GetReaderLoansAsync(_readerId, CancellationToken.None);
            _loading = true;
            _loans.Rows.Clear();
            foreach (var loan in items)
            {
                var row = _loans.Rows.Add(loan.SoPhieu, loan.NgayMuon.ToString("dd/MM/yyyy"),
                    loan.HanTra.ToString("dd/MM/yyyy"), loan.SoLuongConMuon, loan.TrangThai);
                _loans.Rows[row].Tag = loan.MaPhieu;
                if (loan.TrangThai == "Quá hạn") _loans.Rows[row].DefaultCellStyle.ForeColor = UiTheme.Danger;
            }
            if (_loans.Rows.Count > 0)
            {
                _loans.CurrentCell = _loans.Rows[0].Cells[0];
                _loans.Rows[0].Selected = true;
            }
            else
            {
                _books.Rows.Clear();
                _summary.Text = "Độc giả chưa có lịch sử mượn.";
            }
            _loading = false;
            if (_loans.CurrentRow?.Tag is int loanId) await LoadLoanDetailAsync(loanId);
        }
        catch (Exception ex)
        {
            _loading = false;
            MessageBox.Show(ex.Message, "Không tải được lịch sử mượn", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadLoanDetailAsync(int loanId)
    {
        try
        {
            var loan = await _api.GetAsync(loanId, CancellationToken.None);
            _books.Rows.Clear();
            foreach (var line in loan.ChiTiet)
            {
                var history = line.LichSuTra.Count == 0
                    ? "Chưa trả"
                    : string.Join("; ", line.LichSuTra.Select(x => $"{x.NgayTra:dd/MM/yyyy} ×{x.SoLuongXuLy} · {x.TinhTrangSach}"));
                var due = line.LichSuTra.Sum(x => x.SoTienPhatPhaiThu ?? 0m);
                var paid = line.LichSuTra.Sum(x => x.DaThu);
                var row = _books.Rows.Add(line.TenSach,
                    $"{line.SoLuongMuon} / {line.SoLuongDaXuLy} / {line.SoLuongConMuon}", history,
                    $"{due:N0} / {paid:N0} / {Math.Max(0, due - paid):N0}");
                if (line.SoLuongConMuon > 0 && loan.HanTra.Date < DateTime.Today)
                    _books.Rows[row].DefaultCellStyle.ForeColor = UiTheme.Danger;
            }
            var totalDue = loan.ChiTiet.SelectMany(x => x.LichSuTra).Sum(x => x.SoTienPhatPhaiThu ?? 0m);
            var totalPaid = loan.ChiTiet.SelectMany(x => x.LichSuTra).Sum(x => x.DaThu);
            var overdueDays = loan.HanTra.Date < DateTime.Today && loan.SoLuongConMuon > 0
                ? (DateTime.Today - loan.HanTra.Date).Days : 0;
            _summary.Text = $"{loan.SoPhieu}: {loan.TrangThai}" +
                (overdueDays > 0 ? $" · Quá hạn {overdueDays} ngày" : string.Empty) +
                $" · Phạt đã chốt {totalDue:N0} đ · Đã thu {totalPaid:N0} đ · Còn nợ {Math.Max(0, totalDue - totalPaid):N0} đ";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không tải được chi tiết phiếu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
