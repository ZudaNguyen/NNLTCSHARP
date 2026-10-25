using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class LichSuMuonDocGiaDialog : Form
{
    private readonly MuonTraApiClient _api = new();
    private readonly int _readerId;
    private readonly string _readerName;
    private readonly DataGridView _loans = new();
    private readonly DataGridView _books = new();
    private readonly Label _summary = new();
    private bool _loading;

    public LichSuMuonDocGiaDialog(int readerId, string readerName)
    {
        _readerId = readerId;
        _readerName = readerName;
        Text = $"Lịch sử mượn — {readerName}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(850, 550);
        Size = new Size(1040, 680);
        BackColor = Color.FromArgb(247, 243, 233);
        Font = new Font("Segoe UI", 9.5f);

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 54,
            Padding = new Padding(14, 8, 8, 4),
            Text = $"LỊCH SỬ MƯỢN CỦA ĐỘC GIẢ  ·  {_readerName}  ·  Mã {_readerId}",
            Font = new Font("Georgia", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(122, 40, 34)
        };
        Controls.Add(header);

        var split = new SplitContainer
        {
            Orientation = Orientation.Horizontal,
            Size = new Size(1000, 560),
            BackColor = Color.FromArgb(247, 243, 233),
            Padding = new Padding(8)
        };
        split.SplitterDistance = 245;
        split.Dock = DockStyle.Fill;
        ConfigureGrid(_loans);
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

        ConfigureGrid(_books);
        _books.Columns.Add("Sach", "Sách");
        _books.Columns.Add("SoLuong", "Mượn / Đã trả / Còn");
        _books.Columns.Add("LichSuTra", "Ngày trả · Tình trạng");
        _books.Columns.Add("TienPhat", "Phải thu / Đã thu / Còn nợ (đ)");
        split.Panel1.Controls.Add(_loans);
        split.Panel2.Controls.Add(_books);
        Controls.Add(split);

        _summary.Dock = DockStyle.Bottom;
        _summary.Height = 34;
        _summary.Padding = new Padding(14, 6, 6, 4);
        _summary.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _summary.ForeColor = Color.FromArgb(74, 53, 37);
        Controls.Add(_summary);

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
                if (loan.TrangThai == "Quá hạn") _loans.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(160, 55, 35);
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
                    _books.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(160, 55, 35);
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

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.RowHeadersVisible = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.FromArgb(255, 252, 245);
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
        DataGridViewStyleHelper.ApplyLibraryStyle(grid);
    }
}
