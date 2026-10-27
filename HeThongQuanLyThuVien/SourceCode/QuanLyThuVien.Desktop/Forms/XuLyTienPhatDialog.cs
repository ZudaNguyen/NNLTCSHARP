using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

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
    private readonly AppButton _pay;
    private List<(LanTraSachDto Fine, string Book)> _fines = [];
    private bool _submitting;

    public XuLyTienPhatDialog(MuonTraChiTietDto loan, string collector)
    {
        _loan = loan;
        _collector = collector;
        Ui.SetupDialog(this, "Xử lý tiền phạt", new Size(620, 620), new Size(560, 560), fixedSize: true);

        var header = Ui.Heading($"Phiếu {_loan.SoPhieu} · {_loan.TenDocGia}");
        header.Font = UiTheme.Title;

        _fineItems = Ui.ComboBox();
        _fineItems.SelectedIndexChanged += (_, _) => UpdateSelectedFine();
        _details = Ui.MultilineBox(150);
        _details.ReadOnly = true;
        _details.BackColor = UiTheme.SurfaceMuted;
        _summary = new Label
        {
            AutoSize = true, MaximumSize = new Size(540, 0), Font = UiTheme.BodyBold, ForeColor = UiTheme.Text,
            Margin = new Padding(0, 0, 0, 4)
        };
        _amount = Ui.Number(1, 1, 1, 240);
        _amount.ThousandsSeparator = true;
        _amount.DecimalPlaces = 0;

        var configure = Ui.Button("Cấu hình mức phạt mỗi ngày");
        configure.Click += ConfigureRateAsync;
        _pay = Ui.Button("Xác nhận thu", ButtonKind.Success);
        _pay.Click += PayAsync;
        var close = Ui.Button("Đóng");
        close.DialogResult = DialogResult.Cancel;

        var form = new FormGrid(1);
        form.Add("Khoản phạt", _fineItems);
        form.Add("Thông tin chi tiết", _details);
        form.Add("Tổng quan", _summary);
        form.Add("Số tiền thu", _amount);
        form.AddFull(Ui.ButtonBar(_pay, close, configure));

        var card = new CardPanel(null);
        card.AddRow(form);
        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = UiTheme.PageBg,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Margin = new Padding(0, 0, 0, 14);
        card.Dock = DockStyle.Fill;
        stack.Controls.Add(header, 0, 0);
        stack.Controls.Add(card, 0, 1);
        Controls.Add(stack);
        AcceptButton = _pay;
        CancelButton = close;
        Ui.ApplyDpi(this);

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
            using var dialog = new Form();
            Ui.SetupDialog(dialog, "Cấu hình mức phạt", new Size(400, 200), fixedSize: true);
            var input = Ui.Number(1, 1000000000, 1);
            input.ThousandsSeparator = true;
            if (current.MucPhatMoiNgay is > 0) input.Value = Math.Min(input.Maximum, current.MucPhatMoiNgay.Value);
            var save = Ui.Button("Lưu cấu hình", ButtonKind.Primary);
            save.DialogResult = DialogResult.OK;
            var cancel = Ui.Button("Hủy");
            cancel.DialogResult = DialogResult.Cancel;
            var form = new FormGrid(1);
            form.Add("Mức phạt mỗi ngày trễ (VNĐ)", input);
            form.AddFull(Ui.ButtonBar(save, cancel));
            dialog.Controls.Add(form);
            dialog.AcceptButton = save;
            dialog.CancelButton = cancel;
            Ui.ApplyDpi(dialog);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            await _api.SetRateAsync(new CapNhatCauHinhTienPhatDto { MucPhatMoiNgay = input.Value }, CancellationToken.None);
            MessageBox.Show("Đã lưu mức phạt mỗi ngày.", "Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API tiền phạt", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
