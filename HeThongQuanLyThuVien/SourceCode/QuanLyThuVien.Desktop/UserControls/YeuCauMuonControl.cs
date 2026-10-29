using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Desktop.UserControls;

/// <summary>
/// Màn hình duyệt yêu cầu mượn sách do độc giả gửi từ Web.
/// Thủ thư và Admin dùng chung vì API <c>api/yeucau</c> cho phép vai trò Staff.
/// </summary>
public sealed class YeuCauMuonControl : UserControl
{
    private readonly YeuCauMuonApiClient _api = new();
    private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 350 };

    private TextBox txtSearch = null!;
    private ComboBox cboStatus = null!;
    private Label lblSummary = null!;
    private DataGridView grid = null!;
    private AppButton btnApprove = null!, btnReject = null!, btnReload = null!;

    public YeuCauMuonControl()
    {
        BuildUi();
        Load += async (_, _) => await LoadRequestsAsync();
        _filterTimer.Tick += async (_, _) => { _filterTimer.Stop(); await LoadRequestsAsync(); };
    }

    private void BuildUi()
    {
        Ui.SetupPage(this, Ui.ScrollStack(320, BuildListCard()));
    }

    private CardPanel BuildListCard()
    {
        txtSearch = Ui.TextBox("Mã yêu cầu, độc giả, tên sách hoặc ISBN", 320);
        txtSearch.TextChanged += (_, _) => { _filterTimer.Stop(); _filterTimer.Start(); };

        cboStatus = Ui.ComboBox(new object[]
        {
            "Tất cả trạng thái",
            YeuCauMuonTrangThai.ChoDuyet,
            YeuCauMuonTrangThai.DaDuyet,
            YeuCauMuonTrangThai.TuChoi,
            YeuCauMuonTrangThai.DaHuy
        }, selectedIndex: 1, width: 190);
        cboStatus.SelectedIndexChanged += (_, _) => { _filterTimer.Stop(); _filterTimer.Start(); };

        btnReload = Ui.Button("Tải lại");
        btnReload.Click += async (_, _) => await LoadRequestsAsync();

        btnApprove = Ui.Button("Duyệt & tạo phiếu", ButtonKind.Primary);
        btnReject = Ui.Button("Từ chối", ButtonKind.Danger);
        btnApprove.Click += async (_, _) => await ApproveAsync();
        btnReject.Click += async (_, _) => await RejectAsync();

        var toolbar = new ToolbarRow();
        toolbar.AddField("Tra cứu", txtSearch);
        toolbar.AddField("Trạng thái", cboStatus);
        toolbar.AddActions(btnApprove, btnReject, btnReload);

        grid = Ui.Grid();
        grid.Columns.Add("Number", "Mã yêu cầu");
        grid.Columns.Add("Reader", "Độc giả");
        grid.Columns.Add("Book", "Sách");
        grid.Columns.Add("Isbn", "ISBN");
        grid.Columns.Add("Quantity", "SL");
        grid.Columns.Add("Created", "Ngày gửi");
        grid.Columns.Add("Status", "Trạng thái");
        grid.Columns.Add("Loan", "Phiếu mượn");
        grid.Columns.Add("Approver", "Người duyệt");
        grid.Columns.Add("Approved", "Ngày duyệt");
        grid.Columns.Add("Note", "Ghi chú / lý do từ chối");
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ShowDetail(); };
        grid.SelectionChanged += (_, _) => UpdateActions();

        lblSummary = Ui.Hint("Chưa tải dữ liệu.");

        var card = new CardPanel("Duyệt yêu cầu mượn sách", autoHeight: false);
        card.AddRow(toolbar);
        card.AddRow(grid, fill: true);
        card.AddRow(lblSummary);
        return card;
    }

    private async Task LoadRequestsAsync()
    {
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            var status = cboStatus.SelectedIndex <= 0 ? null : cboStatus.SelectedItem?.ToString();
            var requests = await _api.SearchAsync(status, txtSearch.Text, CancellationToken.None);

            var selectedId = CurrentRequest?.MaYeuCau;
            grid.Rows.Clear();
            foreach (var request in requests)
            {
                var note = request.LyDoTuChoi is { Length: > 0 } reason
                    ? $"Từ chối: {reason}"
                    : request.GhiChu ?? "—";

                var index = grid.Rows.Add(
                    request.SoYeuCau,
                    request.TenDocGia,
                    request.TenSach,
                    request.ISBN,
                    request.SoLuong,
                    request.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                    request.TrangThai,
                    request.SoPhieu ?? "—",
                    request.NguoiDuyet ?? "—",
                    request.NgayDuyet?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                    note);
                grid.Rows[index].Tag = request;
            }

            if (selectedId.HasValue) SelectRequest(selectedId.Value);

            var pending = requests.Count(r => r.TrangThai == YeuCauMuonTrangThai.ChoDuyet);
            lblSummary.Text = requests.Count == 0
                ? status == YeuCauMuonTrangThai.ChoDuyet
                    ? "Không có yêu cầu mượn nào đang chờ duyệt. Chọn 'Tất cả trạng thái' để xem lịch sử."
                    : "Không tìm thấy yêu cầu mượn phù hợp."
                : $"Hiển thị {requests.Count} yêu cầu · {pending} đang chờ duyệt.";
            UpdateActions();
        }
        catch (Exception ex)
        {
            if (!IsDisposed) MessageBox.Show(ex.Message, "Lỗi tải yêu cầu mượn", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SelectRequest(int maYeuCau)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Tag is YeuCauMuonDto request && request.MaYeuCau == maYeuCau)
            {
                row.Selected = true;
                grid.CurrentCell = row.Cells[0];
                return;
            }
        }
    }

    private YeuCauMuonDto? CurrentRequest =>
        grid?.CurrentRow?.Tag as YeuCauMuonDto;

    private void UpdateActions()
    {
        var canAct = CurrentRequest?.CoTheHuy == true;
        btnApprove.Enabled = canAct;
        btnReject.Enabled = canAct;
    }

    private void ShowDetail()
    {
        var request = CurrentRequest;
        if (request is null) return;

        var lines = new List<string>
        {
            $"Mã yêu cầu: {request.SoYeuCau}",
            $"Độc giả: {request.TenDocGia} (#{request.MaDocGia})",
            $"Sách: {request.TenSach} · ISBN {request.ISBN}",
            $"Số lượng: {request.SoLuong}",
            $"Ngày gửi: {request.NgayTao:dd/MM/yyyy HH:mm}",
            $"Trạng thái: {request.TrangThai}"
        };
        if (!string.IsNullOrWhiteSpace(request.GhiChu)) lines.Add($"Ghi chú của độc giả: {request.GhiChu}");
        if (!string.IsNullOrWhiteSpace(request.LyDoTuChoi)) lines.Add($"Lý do từ chối: {request.LyDoTuChoi}");
        if (request.SoPhieu is { Length: > 0 } soPhieu) lines.Add($"Phiếu mượn: {soPhieu}");
        if (request.NguoiDuyet is { Length: > 0 } nguoiDuyet)
            lines.Add($"Người duyệt: {nguoiDuyet} · {request.NgayDuyet:dd/MM/yyyy HH:mm}");

        MessageBox.Show(this, string.Join(Environment.NewLine, lines), $"Chi tiết {request.SoYeuCau}",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task ApproveAsync()
    {
        var request = CurrentRequest;
        if (request is null || request.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
        {
            MessageBox.Show("Chọn yêu cầu đang chờ duyệt để duyệt.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Duyệt yêu cầu {request.SoYeuCau}?\n\n" +
            $"Độc giả: {request.TenDocGia}\nSách: {request.TenSach}\nSố lượng: {request.SoLuong}\n" +
            (string.IsNullOrWhiteSpace(request.GhiChu) ? string.Empty : $"Ghi chú: {request.GhiChu}\n") +
            $"\nHệ thống sẽ tạo phiếu mượn {YeuCauMuonService.SoNgayMuon} ngày và trừ số bản còn lại trong kho.",
            "Xác nhận duyệt yêu cầu mượn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            SetBusy(true);
            var result = await _api.ApproveAsync(request.MaYeuCau, CancellationToken.None);
            await LoadRequestsAsync();
            MessageBox.Show(
                $"Đã duyệt {request.SoYeuCau}" + (result.SoPhieu is { Length: > 0 } phieu ? $" và tạo phiếu {phieu}." : "."),
                "Duyệt yêu cầu mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi duyệt yêu cầu mượn", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RejectAsync()
    {
        var request = CurrentRequest;
        if (request is null || request.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
        {
            MessageBox.Show("Chọn yêu cầu đang chờ duyệt để từ chối.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new Form();
        Ui.SetupDialog(dialog, "Từ chối yêu cầu mượn", new Size(480, 280), fixedSize: true);
        var reason = Ui.MultilineBox(90, "Lý do từ chối (không bắt buộc)");
        var form = new FormGrid(1);
        form.Add($"{request.SoYeuCau} · {request.TenDocGia} · {request.TenSach}", reason);
        var save = Ui.Button("Từ chối yêu cầu", ButtonKind.Danger);
        save.DialogResult = DialogResult.OK;
        var close = Ui.Button("Đóng");
        close.DialogResult = DialogResult.Cancel;
        form.AddFull(Ui.ButtonBar(save, close));
        dialog.Controls.Add(form);
        dialog.AcceptButton = save;
        dialog.CancelButton = close;
        Ui.ApplyDpi(dialog);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            SetBusy(true);
            await _api.RejectAsync(request.MaYeuCau, reason.Text.Trim(), CancellationToken.None);
            await LoadRequestsAsync();
            MessageBox.Show($"Đã từ chối yêu cầu {request.SoYeuCau}.", "Từ chối yêu cầu mượn",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi từ chối yêu cầu mượn", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        btnReload.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        if (!busy) UpdateActions();
    }
}
