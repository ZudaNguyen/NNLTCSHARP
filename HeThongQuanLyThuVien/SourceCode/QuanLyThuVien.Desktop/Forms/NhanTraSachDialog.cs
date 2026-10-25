using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class NhanTraSachDialog : Form
{
    private readonly MuonTraApiClient _api = new();
    private readonly MuonTraChiTietDto _loan;
    private readonly decimal? _dailyFine;
    private readonly string _staffName;
    private readonly DataGridView _books = new();
    private readonly DateTimePicker _returnDate = new();
    private readonly NumericUpDown _returnQuantity = new();
    private readonly ComboBox _condition = new();
    private readonly ComboBox _fineTarget = new();
    private readonly NumericUpDown _payment = new();
    private readonly Label _selection = new();
    private readonly Label _finePreview = new();
    private readonly Button _confirm = new();
    private int? _selectedLineId;
    private bool _submitting;

    public NhanTraSachDialog(MuonTraChiTietDto loan, decimal? dailyFine, string staffName)
    {
        _loan = loan;
        _dailyFine = dailyFine;
        _staffName = staffName;
        Text = $"Chi tiết & nhận trả sách — {loan.SoPhieu}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(980, 620);
        Size = new Size(1100, 720);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(247, 243, 233);
        Font = new Font("Segoe UI", 9.5f);

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(14, 8, 8, 4),
            Text = $"Phiếu {loan.SoPhieu}  ·  {loan.TenDocGia}  ·  Mượn {loan.NgayMuon:dd/MM/yyyy}  ·  Hạn trả {loan.HanTra:dd/MM/yyyy}",
            Font = new Font("Georgia", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(122, 40, 34)
        };
        Controls.Add(header);

        ConfigureGrid();
        _books.Dock = DockStyle.Fill;
        _books.Margin = new Padding(12);
        _books.CellClick += (_, e) => { if (e.RowIndex >= 0) SelectLine(); };
        _books.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) SelectLine(); };
        Controls.Add(_books);

        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 250, Padding = new Padding(12), BackColor = Color.FromArgb(255, 252, 245),
            ColumnCount = 4, RowCount = 4, GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };
        for (var i = 0; i < 4; i++) editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _selection.Dock = DockStyle.Fill;
        _selection.AutoEllipsis = true;
        _selection.Margin = new Padding(3, 0, 3, 2);
        _selection.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        editor.Controls.Add(_selection, 0, 0);
        editor.SetColumnSpan(_selection, 4);
        AddField(editor, "Ngày nhận", _returnDate, 0, 1);
        AddField(editor, "Số lượng trả", _returnQuantity, 1, 1);
        AddField(editor, "Tình trạng sách", _condition, 2, 1);
        AddField(editor, "Khoản phạt cần thu", _fineTarget, 3, 1);
        _returnDate.Format = DateTimePickerFormat.Short;
        _returnDate.MaxDate = DateTime.Today;
        _returnDate.Value = DateTime.Today;
        _returnDate.ValueChanged += (_, _) => UpdateFinePreview();
        _returnQuantity.Minimum = 1;
        _returnQuantity.Maximum = 1000;
        _returnQuantity.ValueChanged += (_, _) => UpdateFinePreview();
        _condition.DropDownStyle = ComboBoxStyle.DropDownList;
        _condition.Items.AddRange(["Nguyên vẹn", "Rách hoặc bẩn nhẹ", "Hỏng nặng", "Mất"]);
        _condition.SelectedIndex = 0;
        _fineTarget.DropDownStyle = ComboBoxStyle.DropDownList;
        _fineTarget.SelectedIndexChanged += (_, _) => UpdateFinePreview();
        _fineTarget.Items.Add(new FineTargetOption(null, "Phạt từ lần trả này", null));
        foreach (var fine in _loan.ChiTiet.SelectMany(x => x.LichSuTra).Where(x => x.ConNo is > 0).OrderBy(x => x.NgayTra))
            _fineTarget.Items.Add(new FineTargetOption(fine.MaLanTra,
                $"Thu nợ lần trả #{fine.MaLanTra} — còn {fine.ConNo:N0} đ", fine.ConNo));
        _fineTarget.SelectedIndex = _fineTarget.Items.Count > 1 && !_loan.ChiTiet.Any(x => x.SoLuongConMuon > 0) ? 1 : 0;
        AddField(editor, "Số tiền thu (có thể thu một phần)", _payment, 0, 2);
        var previewPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4, 3, 4, 3), Padding = new Padding(6, 3, 3, 3) };
        _payment.DecimalPlaces = 2;
        _payment.Maximum = decimal.MaxValue;
        _payment.ThousandsSeparator = true;
        _payment.TextAlign = HorizontalAlignment.Right;
        _finePreview.Dock = DockStyle.Fill;
        _finePreview.ForeColor = Color.FromArgb(122, 40, 34);
        _finePreview.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _finePreview.AutoEllipsis = true;
        previewPanel.Controls.Add(_finePreview);
        editor.Controls.Add(previewPanel, 1, 2);
        editor.SetColumnSpan(previewPanel, 3);

        _confirm.Text = "Xác nhận trả sách & xử lý tiền phạt";
        _confirm.Dock = DockStyle.Fill;
        _confirm.Margin = new Padding(3, 5, 6, 2);
        _confirm.BackColor = Color.FromArgb(122, 40, 34);
        _confirm.ForeColor = Color.White;
        _confirm.FlatStyle = FlatStyle.Flat;
        _confirm.FlatAppearance.BorderSize = 0;
        _confirm.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _confirm.Click += ConfirmAsync;
        editor.Controls.Add(_confirm, 0, 3);
        editor.SetColumnSpan(_confirm, 2);

        var close = new Button { Text = "Đóng", Dock = DockStyle.Fill, Margin = new Padding(6, 5, 3, 2), DialogResult = DialogResult.Cancel, Font = new Font("Segoe UI", 9.5f) };
        editor.Controls.Add(close, 2, 3);
        Controls.Add(editor);
        AcceptButton = _confirm;
        CancelButton = close;

        LoadBookRows();
    }

    private void ConfigureGrid()
    {
        _books.ReadOnly = true;
        _books.AllowUserToAddRows = false;
        _books.RowHeadersVisible = false;
        _books.MultiSelect = false;
        _books.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _books.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _books.BackgroundColor = Color.FromArgb(255, 252, 245);
        _books.EnableHeadersVisualStyles = false;
        _books.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
        _books.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
        DataGridViewStyleHelper.ApplyLibraryStyle(_books);
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
            if (line.SoLuongConMuon == 0) _books.Rows[row].DefaultCellStyle.ForeColor = Color.DimGray;
            else if (overdueDays > 0) _books.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(160, 55, 35);
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
        _finePreview.Text = $"Tự tính: {days} ngày trễ × {(int)_returnQuantity.Value} cuốn × {rate:N0} đ/ngày = {amount:N0} đ.\r\nThu tiền có thể để 0 hoặc thu một phần.";
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

    private static void AddField(TableLayoutPanel parent, string label, Control control, int column, int row)
    {
        var field = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4, 2, 4, 2), Padding = new Padding(2) };
        var title = new Label
        {
            Text = label, Dock = DockStyle.Top, Height = 24,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), AutoEllipsis = true
        };
        control.Dock = DockStyle.Top;
        control.Margin = Padding.Empty;
        if (control is ComboBox combo) combo.Height = 30;
        if (control is DateTimePicker picker) picker.Height = 30;
        if (control is NumericUpDown number) number.Height = 30;
        field.Controls.Add(control);
        field.Controls.Add(title);
        parent.Controls.Add(field, column, row);
    }

    private sealed record FineTargetOption(int? MaLanTra, string Label, decimal? ConNo)
    {
        public override string ToString() => Label;
    }
}
