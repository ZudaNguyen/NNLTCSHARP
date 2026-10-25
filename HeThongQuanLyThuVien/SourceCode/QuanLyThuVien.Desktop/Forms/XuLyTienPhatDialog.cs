using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class XuLyTienPhatDialog : Form
{
    private readonly MuonTraApiClient _api = new();
    private readonly MuonTraChiTietDto _loan;
    private readonly string _collector;
    private readonly ComboBox _fineItems;
    private readonly NumericUpDown _amount;
    private readonly TextBox _details;
    private readonly Label _summary;
    private readonly Button _pay;
    private List<(LanTraSachDto Fine, string Book)> _fines = [];
    private bool _submitting;

    public XuLyTienPhatDialog(MuonTraChiTietDto loan, string collector)
    {
        _loan = loan;
        _collector = collector;
        Text = "Xử lý tiền phạt";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        MinimumSize = new Size(540, 490);
        Size = new Size(620, 560);
        BackColor = Color.FromArgb(247, 243, 233);
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 7, BackColor = BackColor };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var header = new Label { Text = $"PHIẾU {_loan.SoPhieu}  •  {_loan.TenDocGia}", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(122, 40, 34), Font = new Font("Georgia", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        layout.Controls.Add(header, 0, 0); layout.SetColumnSpan(header, 2);
        _fineItems = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _fineItems.SelectedIndexChanged += (_, _) => UpdateSelectedFine();
        AddRow(layout, "Khoản phạt:", _fineItems, 1);
        _details = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, ForeColor = Color.FromArgb(44, 30, 22), BackColor = Color.FromArgb(255, 252, 245), BorderStyle = BorderStyle.FixedSingle };
        AddRow(layout, "Thông tin:", _details, 2);
        _summary = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoEllipsis = true };
        AddRow(layout, "Tổng quan:", _summary, 3);
        _amount = new NumericUpDown { Dock = DockStyle.Left, Width = 240, Minimum = 1, Maximum = 1, ThousandsSeparator = true, DecimalPlaces = 0 };
        AddRow(layout, "Số tiền thu:", _amount, 4);

        var configure = Button("Cấu hình mức phạt/ngày", Color.FromArgb(224, 213, 191), Color.FromArgb(44, 30, 22));
        configure.Click += ConfigureRateAsync;
        AddRow(layout, "Mức phạt:", configure, 5);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 8, 0, 0) };
        _pay = Button("Xác nhận thu", Color.FromArgb(45, 106, 79), Color.White);
        _pay.Click += PayAsync;
        var close = Button("Đóng", Color.FromArgb(90, 75, 65), Color.White);
        close.DialogResult = DialogResult.Cancel;
        actions.Controls.AddRange([close, _pay]);
        layout.Controls.Add(actions, 0, 6); layout.SetColumnSpan(actions, 2);
        Controls.Add(layout);
        AcceptButton = _pay;
        CancelButton = close;
        Shown += LoadFineDataAsync;
        Shown += (_, _) =>
        {
            var bounds = Screen.FromControl(this).WorkingArea;
            if (Width > bounds.Width) Width = bounds.Width;
            if (Height > bounds.Height) Height = bounds.Height;
            Left = Math.Max(bounds.Left, bounds.Left + (bounds.Width - Width) / 2);
            Top = Math.Max(bounds.Top, bounds.Top + (bounds.Height - Height) / 2);
        };
    }

    private static void AddRow(TableLayoutPanel layout, string title, Control value, int row)
    {
        layout.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, row);
        layout.Controls.Add(value, 1, row);
    }

    private static Button Button(string text, Color background, Color foreground) => new()
    {
        Text = text, Width = 145, Height = 34, BackColor = background, ForeColor = foreground,
        FlatStyle = FlatStyle.Flat, Font = new Font("Georgia", 9, FontStyle.Bold), UseVisualStyleBackColor = false
    };

    private async void LoadFineDataAsync(object? sender, EventArgs e)
    {
        try
        {
            var fresh = await _api.GetAsync(_loan.MaPhieu, CancellationToken.None);
            _fines = fresh.ChiTiet.SelectMany(line => line.LichSuTra.Select(fine => (fine, line.TenSach)))
                .Where(x => x.fine.SoTienPhatPhaiThu is not null).Select(x => (x.fine, x.TenSach)).ToList();
            _fineItems.Items.Clear();
            foreach (var item in _fines)
                _fineItems.Items.Add($"{item.Book} — lần trả #{item.Fine.MaLanTra} — {item.Fine.TrangThaiThanhToan}");
            if (_fineItems.Items.Count > 0) _fineItems.SelectedIndex = 0;
            var total = _fines.Sum(x => x.Fine.SoTienPhatPhaiThu ?? 0m);
            var paid = _fines.Sum(x => x.Fine.DaThu);
            var outstanding = Math.Max(0, total - paid);
            _summary.Text = $"Phải thu: {total:N0} đ     Đã thu: {paid:N0} đ     Còn nợ: {outstanding:N0} đ";
            _pay.Enabled = outstanding > 0;
            if (_fineItems.Items.Count == 0) _details.Text = "Phiếu chưa có khoản phạt đã được xác định.";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tiền phạt", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void UpdateSelectedFine()
    {
        if (_fineItems.SelectedIndex < 0 || _fineItems.SelectedIndex >= _fines.Count) return;
        var (fine, book) = _fines[_fineItems.SelectedIndex];
        var history = fine.LichSuThu.Count == 0 ? "Chưa có lịch sử thu." : string.Join(Environment.NewLine,
            fine.LichSuThu.OrderBy(x => x.NgayThu).Select(x => $"{x.NgayThu:dd/MM/yyyy HH:mm} | {x.SoTien:N0} đ | {x.NguoiThu ?? ""}"));
        _details.Text = $"Sách: {book}\r\nNgày mượn: {_loan.NgayMuon:dd/MM/yyyy}    Hạn trả: {_loan.HanTra:dd/MM/yyyy}\r\nNgày trả: {fine.NgayTra:dd/MM/yyyy}    Quá hạn: {fine.SoNgayTre} ngày\r\nLý do: {fine.TinhTrangSach}\r\nPhải thu: {fine.SoTienPhatPhaiThu:N0} đ    Đã thu: {fine.DaThu:N0} đ    Còn nợ: {fine.ConNo:N0} đ\r\nTrạng thái: {fine.TrangThaiThanhToan}\r\nLịch sử thu:\r\n{history}";
        var remaining = fine.ConNo ?? 0;
        _amount.Maximum = Math.Max(1, remaining);
        _amount.Value = Math.Max(1, remaining);
        _amount.Enabled = remaining > 0;
        _pay.Enabled = remaining > 0;
    }

    private async void PayAsync(object? sender, EventArgs e)
    {
        if (_submitting || _fineItems.SelectedIndex < 0) return;
        var selected = _fines[_fineItems.SelectedIndex].Fine;
        if (selected.ConNo is not > 0 || _amount.Value > selected.ConNo.Value) return;
        try
        {
            _submitting = true; _pay.Enabled = false;
            await _api.PayFineAsync(_loan.MaPhieu, new ThuTienPhatDto
            {
                MaLanTra = selected.MaLanTra, SoTien = _amount.Value,
                NguoiThu = _collector, IdempotencyKey = Guid.NewGuid()
            }, CancellationToken.None);
            MessageBox.Show("Đã lưu khoản thu và lịch sử thanh toán.", "Thu tiền thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tiền phạt", MessageBoxButtons.OK, MessageBoxIcon.Error); _pay.Enabled = true; }
        finally { _submitting = false; }
    }

    private async void ConfigureRateAsync(object? sender, EventArgs e)
    {
        try
        {
            var current = await _api.GetRateAsync(CancellationToken.None);
            using var dialog = new Form { Text = "Cấu hình mức phạt", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, Width = 390, Height = 175, MinimizeBox = false, MaximizeBox = false, BackColor = BackColor, AutoScaleMode = AutoScaleMode.Dpi };
            var label = new Label { Text = "Mức phạt mỗi ngày trễ (VNĐ):", Left = 16, Top = 18, Width = 340 };
            var input = new NumericUpDown { Left = 16, Top = 45, Width = 340, Maximum = 1000000000, Minimum = 1, ThousandsSeparator = true };
            if (current.MucPhatMoiNgay is > 0) input.Value = Math.Min(input.Maximum, current.MucPhatMoiNgay.Value);
            var save = Button("Lưu cấu hình", Color.FromArgb(122, 40, 34), Color.White); save.Left = 210; save.Top = 82; save.DialogResult = DialogResult.OK;
            dialog.Controls.AddRange([label, input, save]); dialog.AcceptButton = save;
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            await _api.SetRateAsync(new CapNhatCauHinhTienPhatDto { MucPhatMoiNgay = input.Value }, CancellationToken.None);
            MessageBox.Show("Đã lưu mức phạt mỗi ngày.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tiền phạt", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
