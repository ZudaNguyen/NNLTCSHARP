using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls;

public sealed class ReaderSelfServiceControl : UserControl
{
    private readonly DocGiaApiClient _readerApi = new();
    private readonly MuonTraApiClient _loanApi = new();
    private readonly Label _profile = new();
    private readonly Label _fineSummary = new();
    private readonly DataGridView _loans = Ui.Grid();
    private readonly DataGridView _details = Ui.Grid();
    private readonly DataGridView _fines = Ui.Grid();
    private IReadOnlyList<MuonTraDto> _loanRows = [];
    private int? _loadingLoanId;

    public ReaderSelfServiceControl()
    {
        BuildUi();
        Load += async (_, _) => await LoadProfileAndLoansAsync();
        _loans.SelectionChanged += async (_, _) => await LoadSelectedLoanAsync();
    }

    private void BuildUi()
    {
        // ----- Hồ sơ -----
        _profile.AutoSize = true;
        _profile.Font = UiTheme.Body;
        _profile.ForeColor = UiTheme.Text;
        _profile.Text = "Đang tải hồ sơ độc giả...";
        var profileCard = new CardPanel("Thông tin độc giả");
        profileCard.AddRow(_profile);

        // ----- Lịch sử mượn -----
        _loans.Columns.Add("Number", "Số phiếu");
        _loans.Columns.Add("Borrowed", "Ngày mượn");
        _loans.Columns.Add("Due", "Hạn trả");
        _loans.Columns.Add("Outstanding", "Số lượng còn mượn");
        _loans.Columns.Add("Status", "Trạng thái");
        var loansCard = new CardPanel("Lịch sử mượn", autoHeight: false);
        loansCard.AddRow(_loans, fill: true);

        // ----- Chi tiết sách + tiền phạt -----
        _details.Columns.Add("Book", "Sách");
        _details.Columns.Add("Borrowed", "Đã mượn");
        _details.Columns.Add("Processed", "Đã xử lý");
        _details.Columns.Add("Remaining", "Còn mượn");
        var detailsCard = new CardPanel("Chi tiết sách và trả sách", autoHeight: false);
        detailsCard.AddRow(_details, fill: true);

        _fineSummary.AutoSize = true;
        _fineSummary.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        _fineSummary.ForeColor = UiTheme.Text;
        _fineSummary.Margin = new Padding(0, 0, 0, 8);
        _fineSummary.Text = "Chọn phiếu mượn để xem các khoản phạt.";
        _fines.Columns.Add("Return", "Lần trả");
        _fines.Columns.Add("LateDays", "Ngày trễ");
        _fines.Columns.Add("Fine", "Tiền phạt");
        _fines.Columns.Add("Paid", "Đã thu");
        _fines.Columns.Add("Balance", "Còn nợ");
        var fineCard = new CardPanel("Tiền phạt theo phiếu đang chọn", autoHeight: false);
        fineCard.AddRow(_fineSummary);
        fineCard.AddRow(_fines, fill: true);

        var bottom = new TableLayoutPanel
        {
            ColumnCount = 2, RowCount = 1, BackColor = UiTheme.PageBg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detailsCard.Dock = DockStyle.Fill;
        detailsCard.Margin = new Padding(0, 0, 8, 0);
        fineCard.Dock = DockStyle.Fill;
        fineCard.Margin = new Padding(8, 0, 0, 0);
        bottom.Controls.Add(detailsCard, 0, 0);
        bottom.Controls.Add(fineCard, 1, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = UiTheme.PageBg,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        profileCard.Dock = DockStyle.Fill;
        profileCard.Margin = new Padding(0, 0, 0, 16);
        loansCard.Dock = DockStyle.Fill;
        loansCard.Margin = new Padding(0, 0, 0, 16);
        bottom.Dock = DockStyle.Fill;
        layout.Controls.Add(profileCard, 0, 0);
        layout.Controls.Add(loansCard, 0, 1);
        layout.Controls.Add(bottom, 0, 2);

        Ui.SetupPage(this, layout);
    }

    private async Task LoadProfileAndLoansAsync()
    {
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            var profile = await _readerApi.GetMeAsync(CancellationToken.None);
            _profile.Text = $"Độc giả: {profile.HoTen}\nĐiện thoại: {profile.SoDienThoai}    Email: {profile.Email ?? "Chưa cập nhật"}\nTrạng thái thẻ: {profile.TrangThai}    Địa chỉ: {profile.DiaChi ?? "Chưa cập nhật"}";
            _loanRows = await _loanApi.GetMyLoansAsync(CancellationToken.None);
            _loans.Rows.Clear();
            foreach (var loan in _loanRows)
                _loans.Rows.Add(loan.SoPhieu, loan.NgayMuon.ToString("dd/MM/yyyy"), loan.HanTra.ToString("dd/MM/yyyy"), loan.SoLuongConMuon, loan.TrangThai);
            if (_loans.Rows.Count == 0)
            {
                _details.Rows.Clear();
                _fines.Rows.Clear();
                _fineSummary.Text = "Chưa có lịch sử mượn hoặc khoản phạt.";
            }
            else
                await LoadSelectedLoanAsync();
        }
        catch (Exception ex)
        {
            if (!IsDisposed) MessageBox.Show(ex.Message, "Hồ sơ độc giả", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadSelectedLoanAsync()
    {
        if (IsDisposed || _loans.CurrentRow is null || _loans.CurrentRow.Index >= _loanRows.Count) return;
        var loan = _loanRows[_loans.CurrentRow.Index];
        if (_loadingLoanId == loan.MaPhieu) return;
        _loadingLoanId = loan.MaPhieu;
        try
        {
            var detail = await _loanApi.GetMyLoanAsync(loan.MaPhieu, CancellationToken.None);
            _details.Rows.Clear();
            foreach (var item in detail.ChiTiet)
                _details.Rows.Add(item.TenSach, item.SoLuongMuon, item.SoLuongDaXuLy, item.SoLuongConMuon);

            var fines = await _loanApi.GetMyFinesAsync(loan.MaPhieu, CancellationToken.None);
            _fines.Rows.Clear();
            foreach (var fine in fines.CacKhoanPhat)
                _fines.Rows.Add(fine.MaLanTra, fine.SoNgayTre, FormatMoney(fine.SoTienPhatPhaiThu), FormatMoney(fine.DaThu), FormatMoney(fine.ConNo));
            _fineSummary.Text = $"Tổng phạt: {FormatMoney(fines.TongPhaiThu)}    Đã thu: {FormatMoney(fines.DaThu)}    Còn nợ: {FormatMoney(fines.ConNo)}";
        }
        catch (Exception ex)
        {
            if (!IsDisposed) MessageBox.Show(ex.Message, "Chi tiết mượn và tiền phạt", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _loadingLoanId = null; }
    }

    private static string FormatMoney(decimal? amount) => amount.HasValue
        ? amount.Value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " ₫"
        : "Chưa xác định";
}
