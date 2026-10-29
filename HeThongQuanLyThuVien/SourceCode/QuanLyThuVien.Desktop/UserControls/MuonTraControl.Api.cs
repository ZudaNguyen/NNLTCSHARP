using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Forms;

namespace QuanLyThuVien.Desktop.UserControls;

public partial class MuonTraControl
{
    private async Task LoadInitialDataAsync()
    {
        try
        {
            _mucPhatMoiNgay = (await _api.GetRateAsync(CancellationToken.None)).MucPhatMoiNgay;
            var readers = await _api.GetReadersAsync(CancellationToken.None);
            cboDocGia.DisplayMember = nameof(DocGiaDto.HoTen); cboDocGia.ValueMember = nameof(DocGiaDto.MaDocGia); cboDocGia.DataSource = readers;
            await LoadBooksAsync(); await LoadLoansAsync();
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

    private async Task LoadLoansAsync(int? preferredLoanId = null)
    {
        preferredLoanId ??= dgvPhieuMuon.CurrentRow?.Tag is MuonTraDto current ? current.MaPhieu : null;
        var status = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem?.ToString() : null;
        var loans = await _api.SearchAsync(txtTimPhieu.Text, status, CancellationToken.None);
        dgvPhieuMuon.Rows.Clear();
        foreach (var loan in loans)
        {
            var i = dgvPhieuMuon.Rows.Add(loan.SoPhieu, loan.TenDocGia, loan.NguoiLap,
                loan.NgayMuon.ToString("dd/MM/yyyy"), loan.HanTra.ToString("dd/MM/yyyy"),
                $"{loan.SoLanGiaHan}/{loan.SoLanGiaHanToiDa}", loan.TrangThai);
            dgvPhieuMuon.Rows[i].Tag = loan;
        }
        var selectedIndex = -1;
        for (var i = 0; i < dgvPhieuMuon.Rows.Count; i++)
            if (dgvPhieuMuon.Rows[i].Tag is MuonTraDto row && row.MaPhieu == preferredLoanId) { selectedIndex = i; break; }
        if (selectedIndex < 0 && dgvPhieuMuon.Rows.Count > 0) selectedIndex = 0;
        if (selectedIndex >= 0) { dgvPhieuMuon.Rows[selectedIndex].Selected = true; dgvPhieuMuon.CurrentCell = dgvPhieuMuon.Rows[selectedIndex].Cells[0]; }
    }

    private async void ApiFilterMasterGrid()
    {
        try { await LoadLoansAsync(); } catch (Exception ex) { ShowApiError(ex); }
    }

    private async void ApiBtnGiaHan_Click(object? sender, EventArgs e)
    {
        if (dgvPhieuMuon.CurrentRow?.Tag is not MuonTraDto loan)
        {
            MessageBox.Show("Chọn phiếu mượn cần gia hạn.", "Gia hạn phiếu mượn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!loan.CoTheGiaHan)
        {
            MessageBox.Show(loan.LyDoKhongGiaHan ?? "Phiếu mượn không đủ điều kiện gia hạn.",
                "Không thể gia hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var hanTraMoi = loan.HanTra.AddDays(loan.SoNgayGiaHanMoiLan);
        var conLaiSauKhiGiaHan = loan.SoLanGiaHanToiDa - loan.SoLanGiaHan - 1;
        var confirm = MessageBox.Show(
            $"Gia hạn phiếu {loan.SoPhieu} của {loan.TenDocGia}?\n\n" +
            $"Hạn trả: {loan.HanTra:dd/MM/yyyy} → {hanTraMoi:dd/MM/yyyy} (+{loan.SoNgayGiaHanMoiLan} ngày)\n" +
            $"Đã gia hạn: {loan.SoLanGiaHan}/{loan.SoLanGiaHanToiDa} lần. Sau thao tác này còn {conLaiSauKhiGiaHan} lần.",
            "Xác nhận gia hạn phiếu mượn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var result = await _api.GiaHanAsync(loan.MaPhieu, CancellationToken.None);
            await LoadLoansAsync(result.MaPhieu);
            MessageBox.Show(
                $"Đã gia hạn {result.SoPhieu}.\nHạn trả mới: {result.HanTra:dd/MM/yyyy}.\n" +
                $"Đã dùng {result.SoLanGiaHan}/{result.SoLanGiaHanToiDa} lần gia hạn.",
                "Gia hạn thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private async Task OpenNhanTraDialogAsync(int loanId)
    {
        try
        {
            var loan = await _api.GetAsync(loanId, CancellationToken.None);
            _mucPhatMoiNgay = (await _api.GetRateAsync(CancellationToken.None)).MucPhatMoiNgay;
            using var dialog = new NhanTraSachDialog(loan, _mucPhatMoiNgay, _nguoiLap);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadBooksAsync();
                await LoadLoansAsync(loanId);
            }
        }
        catch (Exception ex) { ShowApiError(ex); }
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
            await LoadBooksAsync(); await LoadLoansAsync(result.MaPhieu);
            if (_tabs is not null) _tabs.SelectedIndex = 1;
            MessageBox.Show($"Đã tạo phiếu {result.SoPhieu}.", "Lập phiếu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowApiError(ex); }
    }

    private static void ShowApiError(Exception ex) => MessageBox.Show(ex.Message, "Lỗi API mượn/trả", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
