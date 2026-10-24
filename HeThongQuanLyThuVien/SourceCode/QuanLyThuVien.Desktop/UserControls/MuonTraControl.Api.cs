using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls;

public partial class MuonTraControl
{
    private async Task LoadInitialDataAsync()
    {
        try
        {
            var readers = await _api.GetReadersAsync(CancellationToken.None);
            cboDocGia.DisplayMember = nameof(DocGiaDto.HoTen); cboDocGia.ValueMember = nameof(DocGiaDto.MaDocGia); cboDocGia.DataSource = readers;
            await LoadBooksAsync(); await LoadLoansAsync();
            _mucPhatMoiNgay = (await _api.GetRateAsync(CancellationToken.None)).MucPhatMoiNgay;
            lblTienPhat.Text = _mucPhatMoiNgay.HasValue ? "Tiền phạt (đ):" : "Tiền phạt (chưa cấu hình):";
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private async Task LoadBooksAsync()
    {
        _sachConHang.Clear(); _sachConHang.AddRange(await _api.GetBooksAsync(CancellationToken.None));
        clbSachMuon.Items.Clear(); clbSachMuon.FormattingEnabled = true;
        clbSachMuon.Format -= FormatBook; clbSachMuon.Format += FormatBook;
        foreach (var book in _sachConHang) clbSachMuon.Items.Add(book);
    }

    private static void FormatBook(object? sender, ListControlConvertEventArgs e)
    {
        if (e.ListItem is SachMuonOptionDto b) e.Value = $"{b.MaSach} - {b.TenSach} (Còn: {b.SoLuongCon})";
    }

    private async Task LoadLoansAsync()
    {
        var status = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem?.ToString() : null;
        var loans = await _api.SearchAsync(txtTimPhieu.Text, status, CancellationToken.None);
        _phieuMuon.Clear(); _phieuMuon.AddRange(loans); dgvPhieuMuon.Rows.Clear();
        foreach (var loan in loans)
        {
            var i = dgvPhieuMuon.Rows.Add(loan.SoPhieu, loan.TenDocGia, loan.NguoiLap,
                loan.NgayMuon.ToString("dd/MM/yyyy"), loan.HanTra.ToString("dd/MM/yyyy"), loan.TrangThai);
            dgvPhieuMuon.Rows[i].Tag = loan.MaPhieu;
        }
        if (dgvPhieuMuon.Rows.Count > 0) dgvPhieuMuon.Rows[0].Selected = true;
        else { dgvChiTiet.Rows.Clear(); _selectedLineId = null; }
        if (loans.Count > 0) await LoadDetailAsync(loans[0].MaPhieu);
    }

    private async void ApiFilterMasterGrid()
    {
        try { await LoadLoansAsync(); } catch (Exception ex) { ShowApiError(ex); }
    }

    private async void ApiDgvPhieuMuon_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvPhieuMuon.CurrentRow?.Tag is int id) await LoadDetailAsync(id);
    }

    private async Task LoadDetailAsync(int id)
    {
        try
        {
            var loan = await _api.GetAsync(id, CancellationToken.None); _chiTietPhieu[id] = loan; dgvChiTiet.Rows.Clear();
            foreach (var line in loan.ChiTiet)
            {
                var history = line.LichSuTra;
                var dates = history.Count == 0 ? "Chưa trả" : string.Join(", ", history.Select(x => x.NgayTra.ToString("dd/MM/yyyy")));
                var fine = history.Sum(x => x.SoTienPhatPhaiThu ?? 0m);
                var paid = history.Sum(x => x.DaThu);
                var fineText = history.Any(x => x.SoNgayTre > 0 && x.SoTienPhatPhaiThu is null)
                    ? "Chưa xác định" : $"Phải {fine:N0} / Thu {paid:N0}";
                var state = history.Count == 0 ? "Đang mượn" : history.Last().TinhTrangSach;
                var i = dgvChiTiet.Rows.Add(line.MaSach, line.TenSach, $"{line.SoLuongConMuon} còn / {line.SoLuongMuon}", dates, fineText, state);
                dgvChiTiet.Rows[i].Tag = line.MaChiTiet;
            }
            lblSachDangChon.Text = "Ấn phẩm: (Chọn 1 dòng bên trái)"; _selectedLineId = null;
            numTienPhat.Value = 0; btnXacNhanTra.Enabled = false;
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private void ApiDgvChiTiet_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgvChiTiet.CurrentRow == null) return;
        _selectedLineId = dgvChiTiet.CurrentRow.Tag as int?;
        if (dgvPhieuMuon.CurrentRow?.Tag is int loanId && _chiTietPhieu.TryGetValue(loanId, out var selectedLoan))
        {
            var selectedLine = selectedLoan.ChiTiet.FirstOrDefault(x => x.MaChiTiet == _selectedLineId);
            if (selectedLine is not null && selectedLine.SoLuongConMuon > 0)
            {
                numSoLuongTra.Maximum = selectedLine.SoLuongConMuon;
                numSoLuongTra.Value = selectedLine.SoLuongConMuon;
            }
        }
        lblSachDangChon.Text = $"Ấn phẩm: {dgvChiTiet.CurrentRow.Cells["TenSach"].Value}";
        btnXacNhanTra.Enabled = _selectedLineId.HasValue && !dgvChiTiet.CurrentRow.Cells["SoLuong"].Value!.ToString()!.StartsWith("0 còn");
        ApiTinhTienPhatTuDong();
    }

    private void ApiTinhTienPhatTuDong()
    {
        if (dgvPhieuMuon.CurrentRow?.Tag is not int id || !_chiTietPhieu.TryGetValue(id, out var loan)) { numTienPhat.Value = 0; return; }
        var line = loan.ChiTiet.FirstOrDefault(x => x.MaChiTiet == _selectedLineId);
        if (line == null || !_mucPhatMoiNgay.HasValue) { numTienPhat.Value = 0; return; }
        var days = Math.Max(0, (dtpNgayTra.Value.Date - loan.HanTra.Date).Days);
        numTienPhat.Value = Math.Min(numTienPhat.Maximum, days * numSoLuongTra.Value * _mucPhatMoiNgay.Value);
    }

    private async void ApiBtnLapPhieu_Click(object? sender, EventArgs e)
    {
        try
        {
            if (cboDocGia.SelectedValue is not int readerId) { MessageBox.Show("Chọn độc giả đang hoạt động."); return; }
            if (dtpHanTra.Value.Date < dtpNgayMuon.Value.Date) { MessageBox.Show("Hạn trả không được trước ngày mượn."); return; }
            var books = clbSachMuon.CheckedItems.Cast<SachMuonOptionDto>().Select(x => new TaoChiTietMuonDto { MaSach = x.MaSach, SoLuong = 1 }).ToList();
            if (books.Count == 0) { MessageBox.Show("Vui lòng chọn ít nhất một sách."); return; }
            var result = await _api.CreateAsync(new TaoMuonTraDto { MaDocGia = readerId, NgayMuon = dtpNgayMuon.Value.Date, HanTra = dtpHanTra.Value.Date,
                NguoiLap = _nguoiLap, IdempotencyKey = Guid.NewGuid(), ChiTiet = books }, CancellationToken.None);
            await LoadBooksAsync(); await LoadLoansAsync();
            MessageBox.Show($"Đã tạo phiếu {result.SoPhieu}.", "Lập phiếu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private async void ApiBtnXacNhanTra_Click(object? sender, EventArgs e)
    {
        if (dgvPhieuMuon.CurrentRow?.Tag is not int id || !_selectedLineId.HasValue) { MessageBox.Show("Chọn phiếu và dòng sách cần trả."); return; }
        var line = _chiTietPhieu.GetValueOrDefault(id)?.ChiTiet.FirstOrDefault(x => x.MaChiTiet == _selectedLineId);
        if (line == null || line.SoLuongConMuon <= 0) return;
        try
        {
            await _api.ReturnAsync(id, line.MaChiTiet, new TraChiTietDto { SoLuongXuLy = Math.Min(line.SoLuongConMuon, (int)numSoLuongTra.Value),
                NgayTra = dtpNgayTra.Value.Date, TinhTrangSach = cboTinhTrangTra.SelectedItem?.ToString() ?? "Nguyên vẹn", IdempotencyKey = Guid.NewGuid() }, CancellationToken.None);
            await LoadBooksAsync(); await LoadLoansAsync();
            MessageBox.Show("Đã ghi nhận trả sách và cập nhật tồn kho.", "Trả sách thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private async void ApiBtnCauHinhPhat_Click(object? sender, EventArgs e)
    {
        using var dialog = new Form { Text = "Cấu hình tiền phạt", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, Width = 360, Height = 155, MinimizeBox = false, MaximizeBox = false };
        var label = new Label { Text = "Mức phạt mỗi ngày trễ (VNĐ):", Left = 15, Top = 18, Width = 300 };
        var input = new NumericUpDown { Left = 15, Top = 45, Width = 300, Maximum = 1000000000, ThousandsSeparator = true, Minimum = 1 };
        if (_mucPhatMoiNgay.HasValue) input.Value = Math.Min(input.Maximum, _mucPhatMoiNgay.Value);
        var save = new Button { Text = "Lưu cấu hình", DialogResult = DialogResult.OK, Left = 210, Top = 78, Width = 105 };
        dialog.Controls.AddRange(new Control[] { label, input, save }); dialog.AcceptButton = save;
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { _mucPhatMoiNgay = (await _api.SetRateAsync(new CapNhatCauHinhTienPhatDto { MucPhatMoiNgay = input.Value }, CancellationToken.None)).MucPhatMoiNgay; lblTienPhat.Text = "Tiền phạt (đ):"; ApiTinhTienPhatTuDong(); MessageBox.Show("Đã lưu mức phạt mỗi ngày."); }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private async void ApiBtnThuTienPhat_Click(object? sender, EventArgs e)
    {
        if (dgvPhieuMuon.CurrentRow?.Tag is not int loanId || !_chiTietPhieu.TryGetValue(loanId, out var loan))
        { MessageBox.Show("Chọn phiếu cần thu tiền phạt."); return; }
        var outstanding = loan.ChiTiet.SelectMany(x => x.LichSuTra)
            .Where(x => x.ConNo is > 0).ToList();
        if (outstanding.Count == 0) { MessageBox.Show("Phiếu này không có khoản phạt còn nợ."); return; }

        using var dialog = new Form { Text = "Thu tiền phạt", FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, Width = 390, Height = 205, MinimizeBox = false, MaximizeBox = false };
        var label = new Label { Text = "Chọn khoản phạt trả sách:", Left = 15, Top = 15, Width = 340 };
        var combo = new ComboBox { Left = 15, Top = 40, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var fine in outstanding) combo.Items.Add($"Lần trả #{fine.MaLanTra} — còn nợ {fine.ConNo:N0} VNĐ");
        combo.SelectedIndex = 0;
        var amount = new NumericUpDown { Left = 15, Top = 90, Width = 210, Maximum = 1000000000, Minimum = 1, ThousandsSeparator = true };
        void UpdateAmount() { if (combo.SelectedIndex >= 0) amount.Maximum = Math.Max(1, outstanding[combo.SelectedIndex].ConNo ?? 1); amount.Value = amount.Maximum; }
        combo.SelectedIndexChanged += (_, _) => UpdateAmount(); UpdateAmount();
        var save = new Button { Text = "Ghi nhận thu", DialogResult = DialogResult.OK, Left = 235, Top = 126, Width = 120 };
        dialog.Controls.AddRange(new Control[] { label, combo, amount, save }); dialog.AcceptButton = save;
        if (dialog.ShowDialog(this) != DialogResult.OK || combo.SelectedIndex < 0) return;
        var selected = outstanding[combo.SelectedIndex];
        try
        {
            await _api.PayFineAsync(loanId, new ThuTienPhatDto { MaLanTra = selected.MaLanTra, SoTien = amount.Value,
                NguoiThu = _nguoiLap, IdempotencyKey = Guid.NewGuid() }, CancellationToken.None);
            await LoadDetailAsync(loanId);
            MessageBox.Show("Đã ghi nhận khoản thu và lưu lịch sử thanh toán.", "Thu tiền thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private void ApiBtnLichSuThu_Click(object? sender, EventArgs e)
    {
        if (dgvPhieuMuon.CurrentRow?.Tag is not int loanId || !_chiTietPhieu.TryGetValue(loanId, out var loan))
        { MessageBox.Show("Chọn phiếu để xem lịch sử thu."); return; }
        var history = loan.ChiTiet.SelectMany(line => line.LichSuTra.SelectMany(payment => payment.LichSuThu
            .Select(receipt => new { receipt.NgayThu, receipt.SoTien, receipt.NguoiThu, payment.MaLanTra, line.TenSach })))
            .OrderBy(x => x.NgayThu).ToList();
        var message = history.Count == 0 ? "Phiếu này chưa có giao dịch thu tiền phạt."
            : string.Join(Environment.NewLine, history.Select(x => $"{x.NgayThu:dd/MM/yyyy HH:mm} | {x.SoTien:N0} VNĐ | {x.TenSach} | lần trả #{x.MaLanTra} | {x.NguoiThu}"));
        MessageBox.Show(message, "Lịch sử thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void ShowApiError(Exception ex) => MessageBox.Show(ex.Message, "Lỗi API mượn/trả", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
