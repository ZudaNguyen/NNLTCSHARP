using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class NhanTraSachDialog : Form
{
    private readonly MuonTraApiClient _api = new();
    private readonly MuonTraChiTietDto _loan;
    private readonly decimal? _dailyFine;
    private readonly string _staffName;
    private readonly DataGridView _books = Ui.Grid();
    private readonly DateTimePicker _returnDate = Ui.DatePicker(DateTime.Today);
    private readonly NumericUpDown _returnQuantity = Ui.Number(1, 1000, 1);
    private readonly ComboBox _condition = Ui.ComboBox(new object[] { "Nguyên vẹn", "Rách hoặc bẩn nhẹ", "Hỏng nặng", "Mất" });
    private readonly ComboBox _fineTarget = Ui.ComboBox();
    private readonly NumericUpDown _payment = Ui.Number(0, decimal.MaxValue, 0);
    private readonly Label _selection = new();
    private readonly Label _finePreview = new();
    private readonly AppButton _confirm = Ui.Button("Xác nhận trả sách & xử lý tiền phạt", ButtonKind.Primary);
    private int? _selectedLineId;
    private bool _submitting;

    public NhanTraSachDialog(MuonTraChiTietDto loan, decimal? dailyFine, string staffName)
    {
        _loan = loan;
        _dailyFine = dailyFine;
        _staffName = staffName;
        Ui.SetupDialog(this, $"Nhận trả sách - {loan.SoPhieu}", new Size(1080, 700), new Size(940, 620));

        // ----- Tiêu đề phiếu -----
        var header = new TableLayoutPanel
        {
            ColumnCount = 1, RowCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiTheme.PageBg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var title = Ui.Heading($"Phiếu {loan.SoPhieu} · {loan.TenDocGia}");
        title.Font = UiTheme.Title;
        title.Margin = new Padding(0, 0, 0, 2);
        var info = Ui.Hint($"Ngày mượn {loan.NgayMuon:dd/MM/yyyy}   ·   Hạn trả {loan.HanTra:dd/MM/yyyy}", 900);
        info.Margin = Padding.Empty;
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(info, 0, 1);

        // ----- Danh sách sách trong phiếu -----
        ConfigureGrid();
        _books.CellClick += (_, e) => { if (e.RowIndex >= 0) SelectLine(); };
        _books.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) SelectLine(); };
        var gridCard = new CardPanel("Sách trong phiếu", autoHeight: false);
        gridCard.AddRow(_books, fill: true);

        // ----- Nhận trả & xử lý phạt -----
        _selection.AutoSize = true;
        _selection.MaximumSize = new Size(980, 0);
        _selection.Font = UiTheme.BodyBold;
        _selection.ForeColor = UiTheme.Text;
        _selection.Margin = new Padding(0, 0, 0, 12);

        _returnDate.MaxDate = DateTime.Today;
        _returnDate.ValueChanged += (_, _) => UpdateFinePreview();
        _returnQuantity.ValueChanged += (_, _) => UpdateFinePreview();
        _fineTarget.SelectedIndexChanged += (_, _) => UpdateFinePreview();
        _fineTarget.Items.Add(new FineTargetOption(null, "Phạt từ lần trả này", null));
        foreach (var fine in _loan.ChiTiet.SelectMany(x => x.LichSuTra).Where(x => x.ConNo is > 0).OrderBy(x => x.NgayTra))
            _fineTarget.Items.Add(new FineTargetOption(fine.MaLanTra,
                $"Thu nợ lần trả #{fine.MaLanTra} — còn {fine.ConNo:N0} đ", fine.ConNo));
        _fineTarget.SelectedIndex = _fineTarget.Items.Count > 1 && !_loan.ChiTiet.Any(x => x.SoLuongConMuon > 0) ? 1 : 0;

        _payment.DecimalPlaces = 2;
        _payment.ThousandsSeparator = true;
        _payment.TextAlign = HorizontalAlignment.Right;
        _finePreview.AutoSize = true;
        _finePreview.MaximumSize = new Size(720, 0);
        _finePreview.ForeColor = UiTheme.Danger;
        _finePreview.Font = UiTheme.BodyBold;
        _finePreview.Margin = new Padding(0, 2, 0, 0);

        var fields = new FormGrid(4);
        fields.Add("Ngày nhận", _returnDate);
        fields.Add("Số lượng trả", _returnQuantity);
        fields.Add("Tình trạng sách", _condition);
        fields.Add("Khoản phạt cần thu", _fineTarget);
        fields.Add("Số tiền thu (có thể thu một phần)", _payment);
        fields.Add("Tiền phạt", _finePreview, 3);

        _confirm.Click += ConfirmAsync;
        var close = Ui.Button("Đóng");
        close.DialogResult = DialogResult.Cancel;
        var editorCard = new CardPanel("Nhận trả và xử lý phạt");
        editorCard.AddRow(_selection);
        editorCard.AddRow(fields);
        editorCard.AddRow(Ui.ButtonBar(_confirm, close));

        Controls.Add(Ui.Stack(1, header, gridCard, editorCard));
        AcceptButton = _confirm;
        CancelButton = close;
        Ui.ApplyDpi(this);

        LoadBookRows();
    }

    private void ConfigureGrid()
    {
        _books.Columns.Add("TenSach", "Sách");
        _books.Columns.Add("SoLuong", "Mượn / Đã trả / Còn");
        _books.Columns.Add("QuaHan", "Quá hạn");
        _books.Columns.Add("LichSu", "Lịch sử nhận trả");
        _books.Columns.Add("Phat", "Phạt đã chốt / đã thu");
    }

    private void LoadBookRows()
    {
        _books.Rows.Clear();
        foreach (var line in _loan.ChiTiet)
        {
            var overdueDays = line.SoLuongConMuon > 0 ? Math.Max(0, (DateTime.Today - _loan.HanTra.Date).Days) : 0;
            var history = string.Join("; ", line.LichSuTra.Select(x => $"{x.NgayTra:dd/MM/yyyy} ×{x.SoLuongXuLy} ({x.TinhTrangSach})"));
            if (history.Length == 0) history = "Chưa trả";
            var fine = line.LichSuTra.Sum(x => x.SoTienPhatPhaiThu ?? 0m);
            var paid = line.LichSuTra.Sum(x => x.DaThu);
            var row = _books.Rows.Add(line.TenSach, $"{line.SoLuongMuon} / {line.SoLuongDaXuLy} / {line.SoLuongConMuon}",
                overdueDays == 0 ? "Không" : $"{overdueDays} ngày", history, $"{fine:N0} / {paid:N0} đ");
            _books.Rows[row].Tag = line.MaChiTiet;
            if (line.SoLuongConMuon == 0) _books.Rows[row].DefaultCellStyle.ForeColor = UiTheme.TextMuted;
            else if (overdueDays > 0) _books.Rows[row].DefaultCellStyle.ForeColor = UiTheme.Danger;
        }

        var firstOpen = _loan.ChiTiet.FirstOrDefault(x => x.SoLuongConMuon > 0);
        if (firstOpen is null)
        {
            _selectedLineId = null;
            _selection.Text = "Phiếu này đã trả đủ các sách.";
            _confirm.Enabled = _fineTarget.Items.Count > 1;
            UpdateFinePreview();
            return;
        }
        for (var i = 0; i < _books.Rows.Count; i++)
            if (_books.Rows[i].Tag is int id && id == firstOpen.MaChiTiet)
            {
                _books.CurrentCell = _books.Rows[i].Cells[0];
                _books.Rows[i].Selected = true;
                break;
            }
        SelectLine();
    }

    private void SelectLine()
    {
        if (_books.CurrentRow?.Tag is not int id) return;
        var line = _loan.ChiTiet.FirstOrDefault(x => x.MaChiTiet == id);
        if (line is null) return;
        _selectedLineId = id;
        var overdueDays = line.SoLuongConMuon > 0 ? Math.Max(0, (DateTime.Today - _loan.HanTra.Date).Days) : 0;
        _selection.Text = $"Đang chọn: {line.TenSach} — còn mượn {line.SoLuongConMuon} cuốn" +
            (overdueDays > 0 ? $" — QUÁ HẠN {overdueDays} ngày" : string.Empty);
        _returnQuantity.Maximum = Math.Max(1, line.SoLuongConMuon);
        _returnQuantity.Value = Math.Max(1, line.SoLuongConMuon);
        _returnQuantity.Enabled = line.SoLuongConMuon > 0;
        _condition.Enabled = line.SoLuongConMuon > 0;
        _confirm.Enabled = line.SoLuongConMuon > 0;
        UpdateFinePreview();
    }

    private void UpdateFinePreview()
    {
        var target = _fineTarget.SelectedItem as FineTargetOption;
        if (target?.MaLanTra is int fineId)
        {
            _selection.Text = $"Đang chọn thu khoản phạt #{fineId} đã phát sinh trước đó.";
            _returnDate.Enabled = false;
            _returnQuantity.Enabled = false;
            _condition.Enabled = false;
            _confirm.Enabled = true;
            _payment.Maximum = target.ConNo ?? 0m;
            _payment.Value = _payment.Maximum;
            _finePreview.Text = $"Khoản phạt đã chốt còn nợ {target.ConNo:N0} đ. Xác nhận sẽ ghi nhận khoản thu này.";
            return;
        }
        _returnDate.Enabled = true;
        var line = _selectedLineId is int id ? _loan.ChiTiet.FirstOrDefault(x => x.MaChiTiet == id) : null;
        if (line is null || line.SoLuongConMuon <= 0)
        {
            _finePreview.Text = "Chọn một đầu sách còn đang mượn để ghi nhận trả.";
            _payment.Value = 0;
            _confirm.Enabled = false;
            return;
        }
        _returnQuantity.Enabled = true;
        _condition.Enabled = true;
        _confirm.Enabled = true;
        var days = Math.Max(0, (_returnDate.Value.Date - _loan.HanTra.Date).Days);
        if (_dailyFine is not decimal rate)
        {
            _finePreview.Text = "Chưa cấu hình mức phạt mỗi ngày; hệ thống không thể tính phạt tự động.";
            _payment.Maximum = 0;
            _payment.Value = 0;
            return;
        }

        var amount = days * (int)_returnQuantity.Value * rate;
        _finePreview.Text = $"Tự tính: {days} ngày trễ × {(int)_returnQuantity.Value} cuốn × {rate:N0} đ/ngày = {amount:N0} đ.\r\nSố tiền thu có thể để 0 hoặc thu một phần.";
        _payment.Maximum = amount;
        _payment.Value = amount;
    }

    private async void ConfirmAsync(object? sender, EventArgs e)
    {
        if (_submitting) return;
        var fineTarget = _fineTarget.SelectedItem as FineTargetOption;
        var lineId = _selectedLineId;
        var line = lineId is int selectedId ? _loan.ChiTiet.FirstOrDefault(x => x.MaChiTiet == selectedId) : null;
        if (fineTarget?.MaLanTra is null && (lineId is null || line is null || line.SoLuongConMuon <= 0)) return;
        if (fineTarget?.MaLanTra is not null && _payment.Value <= 0)
        {
            MessageBox.Show("Nhập số tiền thu lớn hơn 0.", "Thiếu số tiền thu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _submitting = true;
        _confirm.Enabled = false;
        try
        {
            if (fineTarget?.MaLanTra is int oldFineId)
            {
                await _api.PayFineAsync(_loan.MaPhieu, new ThuTienPhatDto
                {
                    MaLanTra = oldFineId,
                    SoTien = _payment.Value,
                    NguoiThu = _staffName,
                    IdempotencyKey = Guid.NewGuid()
                }, CancellationToken.None);
                MessageBox.Show($"Đã ghi nhận thu {_payment.Value:N0} đ cho khoản phạt #{oldFineId}.",
                    "Thu phạt thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            var updated = await _api.ReturnAsync(_loan.MaPhieu, lineId!.Value, new TraChiTietDto
            {
                SoLuongXuLy = (int)_returnQuantity.Value,
                NgayTra = _returnDate.Value.Date,
                TinhTrangSach = _condition.SelectedItem?.ToString() ?? "Nguyên vẹn",
                SoTienThu = _payment.Value,
                NguoiThu = _staffName,
                IdempotencyKey = Guid.NewGuid()
            }, CancellationToken.None);
            var returned = updated.ChiTiet.First(x => x.MaChiTiet == lineId).LichSuTra.Last();
            var unpaid = (returned.ConNo ?? 0m);
            MessageBox.Show(
                $"Đã ghi nhận trả {returned.SoLuongXuLy} cuốn {line.TenSach}.\r\n" +
                $"Tình trạng: {returned.TinhTrangSach}\r\nSố ngày trễ: {returned.SoNgayTre}\r\n" +
                $"Tiền phạt: {returned.SoTienPhatPhaiThu?.ToString("N0") ?? "Chưa xác định"} đ\r\n" +
                $"Đã thu: {returned.DaThu:N0} đ · Còn nợ: {unpaid:N0} đ",
                "Đã nhận trả và xử lý phạt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể xác nhận nhận trả", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _submitting = false;
            _confirm.Enabled = true;
        }
    }

    private sealed record FineTargetOption(int? MaLanTra, string Label, decimal? ConNo)
    {
        public override string ToString() => Label;
    }
}
