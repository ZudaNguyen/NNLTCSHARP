using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls;

public sealed class ReaderSelfServiceControl : UserControl
{
    private readonly DocGiaApiClient _readerApi = new();
    private readonly MuonTraApiClient _loanApi = new();
    private readonly Color _paper = Color.FromArgb(247, 243, 233);
    private readonly Color _card = Color.FromArgb(255, 252, 245);
    private readonly Color _wood = Color.FromArgb(44, 30, 22);
    private readonly Label _profile = new();
    private readonly Label _fineSummary = new();
    private readonly DataGridView _loans = new();
    private readonly DataGridView _details = new();
    private readonly DataGridView _fines = new();
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
        Dock = DockStyle.Fill;
        Size = new Size(900, 600);
        BackColor = _paper;
        Padding = new Padding(16);

        var profileCard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = _card,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(14)
        };
        _profile.Dock = DockStyle.Fill;
        _profile.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        _profile.ForeColor = _wood;
        _profile.Text = "Đang tải hồ sơ độc giả...";
        profileCard.Controls.Add(_profile);

        var loansCard = CreateCard("Lịch sử mượn", _loans);
        loansCard.Dock = DockStyle.Fill;
        ConfigureGrid(_loans);
        _loans.Columns.Add("Number", "Số phiếu");
        _loans.Columns.Add("Borrowed", "Ngày mượn");
        _loans.Columns.Add("Due", "Hạn trả");
        _loans.Columns.Add("Outstanding", "Số lượng còn mượn");
        _loans.Columns.Add("Status", "Trạng thái");

        var bottom = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Size = new Size(800, 400),
            Orientation = Orientation.Horizontal,
            SplitterDistance = 190,
            BackColor = _paper,
            BorderStyle = BorderStyle.None
        };
        var detailsCard = CreateCard("Chi tiết sách và trả sách", _details);
        ConfigureGrid(_details);
        _details.Columns.Add("Book", "Sách");
        _details.Columns.Add("Borrowed", "Đã mượn");
        _details.Columns.Add("Processed", "Đã xử lý");
        _details.Columns.Add("Remaining", "Còn mượn");
        bottom.Panel1.Controls.Add(detailsCard);

        var finePanel = new Panel { Dock = DockStyle.Fill, BackColor = _paper };
        var fineCard = CreateCard("Tiền phạt theo phiếu đang chọn", _fines);
        fineCard.Dock = DockStyle.Fill;
        fineCard.Padding = new Padding(8, 32, 8, 8);
        _fineSummary.Dock = DockStyle.Top;
        _fineSummary.Height = 28;
        _fineSummary.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _fineSummary.ForeColor = _wood;
        _fineSummary.TextAlign = ContentAlignment.MiddleLeft;
        _fineSummary.Text = "Chọn phiếu mượn để xem các khoản phạt.";
        ConfigureGrid(_fines);
        _fines.Columns.Add("Return", "Lần trả");
        _fines.Columns.Add("LateDays", "Ngày trễ");
        _fines.Columns.Add("Fine", "Tiền phạt");
        _fines.Columns.Add("Paid", "Đã thu");
        _fines.Columns.Add("Balance", "Còn nợ");
        finePanel.Controls.Add(fineCard);
        finePanel.Controls.Add(_fineSummary);
        bottom.Panel2.Controls.Add(finePanel);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = _paper,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0, 10, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.Controls.Add(profileCard, 0, 0);
        layout.Controls.Add(loansCard, 0, 1);
        layout.Controls.Add(bottom, 0, 2);
        Controls.Add(layout);
    }

    private Panel CreateCard(string title, Control content)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _card,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(8, 32, 8, 8)
        };
        var heading = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Georgia", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(122, 40, 34),
            BackColor = _card
        };
        content.Dock = DockStyle.Fill;
        panel.Controls.Add(content);
        panel.Controls.Add(heading);
        return panel;
    }

    private void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.BackgroundColor = _card;
        grid.BorderStyle = BorderStyle.None;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.Font = new Font("Segoe UI", 9);
        grid.ColumnHeadersHeight = 34;
        grid.RowTemplate.Height = 29;
        grid.GridColor = Color.FromArgb(225, 215, 195);
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = _card;
        grid.DefaultCellStyle.ForeColor = _wood;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160);
        grid.DefaultCellStyle.SelectionForeColor = _wood;
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
