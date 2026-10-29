using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Desktop.UserControls;

/// <summary>
/// Màn hình yêu cầu khóa tài khoản độc giả.
/// Thủ thư (<c>isAdmin = false</c>) gửi và hủy yêu cầu của mình;
/// Admin (<c>isAdmin = true</c>) xem tất cả và duyệt / từ chối.
/// </summary>
public sealed class YeuCauKhoaTaiKhoanControl : UserControl
{
    private readonly bool _isAdmin;
    private readonly YeuCauKhoaTaiKhoanApiClient _api = new();
    private readonly System.Windows.Forms.Timer _filterTimer = new() { Interval = 350 };
    private readonly System.Windows.Forms.Timer _readerTimer = new() { Interval = 450 };

    private TextBox txtReaderSearch = null!, txtReason = null!, txtSearch = null!;
    private ComboBox cboReader = null!, cboStatus = null!;
    private NumericUpDown numDays = null!;
    private Label lblReaderInfo = null!, lblSummary = null!;
    private DataGridView grid = null!;
    private AppButton btnCreate = null!, btnResetForm = null!, btnCancel = null!, btnApprove = null!, btnReject = null!, btnReload = null!;
    private bool _loadingReaders;

    public YeuCauKhoaTaiKhoanControl(bool isAdmin)
    {
        _isAdmin = isAdmin;
        BuildUi();
        Load += async (_, _) => await InitialLoadAsync();
        _filterTimer.Tick += async (_, _) => { _filterTimer.Stop(); await LoadRequestsAsync(); };
        _readerTimer.Tick += async (_, _) => { _readerTimer.Stop(); await LoadReaderAccountsAsync(); };
    }

    private void BuildUi()
    {
        var rows = new List<Control>();
        if (!_isAdmin) rows.Add(BuildCreateCard());
        rows.Add(BuildListCard());
        Ui.SetupPage(this, Ui.ScrollStack(_isAdmin ? 320 : 250, [.. rows]));
    }

    private CardPanel BuildCreateCard()
    {
        txtReaderSearch = Ui.TextBox("Tìm tên đăng nhập hoặc họ tên độc giả", 320);
        txtReaderSearch.TextChanged += (_, _) => { _readerTimer.Stop(); _readerTimer.Start(); };

        cboReader = Ui.ComboBox();
        cboReader.SelectedIndexChanged += (_, _) => UpdateReaderInfo();

        lblReaderInfo = Ui.Hint("Chọn tài khoản độc giả cần khóa. Chỉ tài khoản đang hoạt động mới nhận yêu cầu.");

        txtReason = Ui.MultilineBox(72, "Ví dụ: độc giả làm mất sách, vi phạm nội quy thư viện...");
        numDays = Ui.Number(0, 365, 0, 120);

        btnCreate = Ui.Button("Gửi yêu cầu", ButtonKind.Primary);
        btnResetForm = Ui.Button("Làm mới biểu mẫu");
        btnCreate.Click += async (_, _) => await CreateAsync();
        btnResetForm.Click += (_, _) => ClearForm();

        var fields = new FormGrid(2);
        fields.Add("Tìm tài khoản độc giả", txtReaderSearch);
        fields.Add("Tài khoản độc giả", cboReader);
        fields.Add("Lý do khóa (10–500 ký tự)", txtReason, 2);
        fields.Add("Thời hạn dự kiến (ngày, 0 = không đặt)", numDays);

        var card = new CardPanel("Gửi yêu cầu khóa tài khoản độc giả");
        card.AddRow(fields);
        card.AddRow(lblReaderInfo);
        card.AddRow(Ui.ButtonBar(btnCreate, btnResetForm));
        return card;
    }

    private CardPanel BuildListCard()
    {
        txtSearch = Ui.TextBox("Mã yêu cầu, tài khoản, người gửi hoặc lý do", 320);
        txtSearch.TextChanged += (_, _) => { _filterTimer.Stop(); _filterTimer.Start(); };

        cboStatus = Ui.ComboBox(new object[]
        {
            "Tất cả trạng thái",
            YeuCauKhoaTaiKhoanTrangThai.ChoDuyet,
            YeuCauKhoaTaiKhoanTrangThai.DaDuyet,
            YeuCauKhoaTaiKhoanTrangThai.TuChoi,
            YeuCauKhoaTaiKhoanTrangThai.DaHuy
        }, width: 190);
        cboStatus.SelectedIndexChanged += (_, _) => { _filterTimer.Stop(); _filterTimer.Start(); };

        btnReload = Ui.Button("Tải lại");
        btnReload.Click += async (_, _) => await LoadRequestsAsync();

        var toolbar = new ToolbarRow();
        toolbar.AddField("Tra cứu", txtSearch);
        toolbar.AddField("Trạng thái", cboStatus);

        var actions = new List<Control> { btnReload };
        if (_isAdmin)
        {
            btnApprove = Ui.Button("Duyệt & khóa", ButtonKind.Danger);
            btnReject = Ui.Button("Từ chối");
            btnApprove.Click += async (_, _) => await ApproveAsync();
            btnReject.Click += async (_, _) => await RejectAsync();
            actions.Insert(0, btnApprove);
            actions.Insert(1, btnReject);
        }
        else
        {
            btnCancel = Ui.Button("Hủy yêu cầu của tôi", ButtonKind.Danger);
            btnCancel.Click += async (_, _) => await CancelAsync();
            actions.Insert(0, btnCancel);
        }
        toolbar.AddActions([.. actions]);

        grid = Ui.Grid();
        grid.Columns.Add("Number", "Mã yêu cầu");
        grid.Columns.Add("Account", "Tài khoản");
        grid.Columns.Add("Reader", "Độc giả");
        grid.Columns.Add("Requester", "Người gửi");
        grid.Columns.Add("Reason", "Lý do");
        grid.Columns.Add("Deadline", "Thời hạn");
        grid.Columns.Add("Status", "Trạng thái");
        grid.Columns.Add("Created", "Ngày gửi");
        grid.Columns.Add("Approver", "Người duyệt");
        grid.Columns.Add("Approved", "Ngày duyệt");
        grid.SelectionChanged += (_, _) => UpdateActions();

        lblSummary = Ui.Hint("Chưa tải dữ liệu.");

        var card = new CardPanel(
            _isAdmin ? "Duyệt yêu cầu khóa tài khoản" : "Yêu cầu khóa tài khoản của tôi",
            autoHeight: false);
        card.AddRow(toolbar);
        card.AddRow(grid, fill: true);
        card.AddRow(lblSummary);
        return card;
    }

    private async Task InitialLoadAsync()
    {
        if (!_isAdmin) await LoadReaderAccountsAsync();
        await LoadRequestsAsync();
    }

    private async Task LoadReaderAccountsAsync()
    {
        if (_isAdmin || _loadingReaders || IsDisposed || !IsHandleCreated) return;
        _loadingReaders = true;
        try
        {
            var selectedId = (cboReader.SelectedItem as ReaderOption)?.Account.MaTaiKhoan;
            var accounts = await _api.GetReaderAccountsAsync(txtReaderSearch.Text, CancellationToken.None);

            cboReader.BeginUpdate();
            cboReader.Items.Clear();
            foreach (var account in accounts) cboReader.Items.Add(new ReaderOption(account));
            if (selectedId.HasValue)
            {
                for (var i = 0; i < cboReader.Items.Count; i++)
                {
                    if (cboReader.Items[i] is ReaderOption option && option.Account.MaTaiKhoan == selectedId)
                    {
                        cboReader.SelectedIndex = i;
                        break;
                    }
                }
            }
            cboReader.EndUpdate();

            if (accounts.Count == 0)
                lblReaderInfo.Text = "Không tìm thấy tài khoản độc giả phù hợp.";
            UpdateReaderInfo();
        }
        catch (Exception ex)
        {
            if (!IsDisposed) MessageBox.Show(ex.Message, "Lỗi tải tài khoản độc giả", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loadingReaders = false;
        }
    }

    private void UpdateReaderInfo()
    {
        if (cboReader.SelectedItem is not ReaderOption option)
        {
            if (lblReaderInfo.Text != "Không tìm thấy tài khoản độc giả phù hợp.")
                lblReaderInfo.Text = "Chọn tài khoản độc giả cần khóa.";
            return;
        }

        var account = option.Account;
        lblReaderInfo.Text =
            $"Tài khoản: {account.TenDangNhap} · Hồ sơ: {account.TenDocGia ?? "—"} · " +
            $"Thẻ: {account.TrangThaiDocGia ?? "—"} · Tài khoản: {account.TrangThaiTaiKhoan}";
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
                var index = grid.Rows.Add(
                    request.SoYeuCau,
                    request.TenDangNhap,
                    request.TenDocGia ?? request.TenHienThi,
                    request.TenNhanVien,
                    request.LyDo,
                    request.ThoiHanKhoaNgay.HasValue ? $"{request.ThoiHanKhoaNgay} ngày" : "Không đặt",
                    request.TrangThai,
                    request.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                    request.NguoiDuyet ?? "—",
                    request.NgayDuyet?.ToString("dd/MM/yyyy HH:mm") ?? "—");
                grid.Rows[index].Tag = request;
            }

            if (selectedId.HasValue)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Tag is YeuCauKhoaTaiKhoanDto request && request.MaYeuCau == selectedId)
                    {
                        row.Selected = true;
                        grid.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }

            lblSummary.Text = $"Tổng cộng {requests.Count} yêu cầu" +
                              (_isAdmin ? " trên toàn hệ thống." : " do bạn gửi.");
            UpdateActions();
        }
        catch (Exception ex)
        {
            if (!IsDisposed) MessageBox.Show(ex.Message, "Lỗi tải yêu cầu khóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private YeuCauKhoaTaiKhoanDto? CurrentRequest =>
        grid?.CurrentRow?.Tag as YeuCauKhoaTaiKhoanDto;

    private void UpdateActions()
    {
        var canAct = CurrentRequest?.TrangThai == YeuCauKhoaTaiKhoanTrangThai.ChoDuyet;
        if (btnCancel != null) btnCancel.Enabled = canAct;
        if (btnApprove != null) btnApprove.Enabled = canAct;
        if (btnReject != null) btnReject.Enabled = canAct;
    }

    private async Task CreateAsync()
    {
        if (cboReader.SelectedItem is not ReaderOption option)
        {
            MessageBox.Show("Chọn tài khoản độc giả cần khóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var reason = txtReason.Text.Trim();
        if (reason.Length < 10)
        {
            MessageBox.Show("Lý do khóa phải có ít nhất 10 ký tự.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (reason.Length > 500)
        {
            MessageBox.Show("Lý do khóa tối đa 500 ký tự.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var account = option.Account;
        var confirm = MessageBox.Show(
            $"Gửi yêu cầu khóa tài khoản '{account.TenDangNhap}' của độc giả {account.TenDocGia ?? account.TenHienThi}?\n" +
            "Tài khoản chỉ bị khóa sau khi Admin duyệt.",
            "Xác nhận gửi yêu cầu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            SetBusy(true);
            var result = await _api.CreateAsync(new TaoYeuCauKhoaTaiKhoanDto
            {
                MaTaiKhoan = account.MaTaiKhoan,
                LyDo = reason,
                ThoiHanKhoaNgay = numDays.Value == 0 ? null : (int)numDays.Value
            }, CancellationToken.None);
            await LoadRequestsAsync();
            await LoadReaderAccountsAsync();
            ClearForm();
            MessageBox.Show($"Đã gửi yêu cầu {result.SoYeuCau}. Vui lòng chờ Admin duyệt.",
                "Yêu cầu khóa tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi gửi yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CancelAsync()
    {
        var request = CurrentRequest;
        if (request is null || request.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
        {
            MessageBox.Show("Chọn yêu cầu đang chờ duyệt để hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"Hủy yêu cầu {request.SoYeuCau} cho tài khoản '{request.TenDangNhap}'?",
                "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            SetBusy(true);
            await _api.CancelAsync(request.MaYeuCau, CancellationToken.None);
            await LoadRequestsAsync();
            MessageBox.Show("Đã hủy yêu cầu khóa tài khoản.", "Yêu cầu khóa tài khoản",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi hủy yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ApproveAsync()
    {
        var request = CurrentRequest;
        if (request is null || request.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
        {
            MessageBox.Show("Chọn yêu cầu đang chờ duyệt để duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Duyệt yêu cầu {request.SoYeuCau}?\n\n" +
            $"Tài khoản: {request.TenDangNhap}\nĐộc giả: {request.TenDocGia ?? request.TenHienThi}\n" +
            $"Lý do: {request.LyDo}\n\n" +
            "Tài khoản sẽ bị khóa, thẻ độc giả chuyển sang 'Khóa thẻ' và mọi phiên đăng nhập bị thu hồi.",
            "Xác nhận duyệt khóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            SetBusy(true);
            await _api.ApproveAsync(request.MaYeuCau, CancellationToken.None);
            await LoadRequestsAsync();
            MessageBox.Show($"Đã duyệt {request.SoYeuCau}. Tài khoản '{request.TenDangNhap}' đã bị khóa.",
                "Duyệt yêu cầu khóa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi duyệt yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RejectAsync()
    {
        var request = CurrentRequest;
        if (request is null || request.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
        {
            MessageBox.Show("Chọn yêu cầu đang chờ duyệt để từ chối.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new Form();
        Ui.SetupDialog(dialog, "Từ chối yêu cầu khóa tài khoản", new Size(480, 280), fixedSize: true);
        var reason = Ui.MultilineBox(90, "Lý do từ chối (không bắt buộc)");
        var form = new FormGrid(1);
        form.Add($"Yêu cầu {request.SoYeuCau} · {request.TenDangNhap}", reason);
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
            MessageBox.Show($"Đã từ chối yêu cầu {request.SoYeuCau}.", "Từ chối yêu cầu khóa",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi từ chối yêu cầu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ClearForm()
    {
        txtReaderSearch.Clear();
        txtReason.Clear();
        numDays.Value = 0;
        cboReader.SelectedIndex = -1;
        lblReaderInfo.Text = "Chọn tài khoản độc giả cần khóa.";
        grid.ClearSelection();
    }

    private void SetBusy(bool busy)
    {
        if (btnCreate != null) btnCreate.Enabled = !busy;
        if (btnResetForm != null) btnResetForm.Enabled = !busy;
        if (btnReload != null) btnReload.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        if (!busy) UpdateActions();
    }

    private sealed class ReaderOption(TaiKhoanDocGiaOptionDto account)
    {
        public TaiKhoanDocGiaOptionDto Account { get; } = account;

        public override string ToString() =>
            $"{Account.TenHienThi} ({Account.TenDangNhap}) · ĐG #{Account.MaDocGia?.ToString() ?? "—"}";
    }
}
