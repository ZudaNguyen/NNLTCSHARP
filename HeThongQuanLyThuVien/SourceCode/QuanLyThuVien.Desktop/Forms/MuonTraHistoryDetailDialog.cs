using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class MuonTraHistoryDetailDialog : Form
{
    public MuonTraHistoryDetailDialog(MuonTraChiTietDto loan)
    {
        Ui.SetupDialog(this, $"Chi tiết lịch sử - {loan.SoPhieu}", new Size(1040, 640), new Size(860, 540));

        var lateDays = loan.SoLuongConMuon > 0 && loan.HanTra.Date < DateTime.Today
            ? (DateTime.Today - loan.HanTra.Date).Days : 0;
        var title = Ui.Heading($"Phiếu {loan.SoPhieu}");
        title.Font = UiTheme.Title;
        title.Margin = new Padding(0, 0, 0, 4);
        var summary = new Label
        {
            AutoSize = true, MaximumSize = new Size(980, 0), Font = UiTheme.Font(9.5f), ForeColor = UiTheme.TextSecondary,
            Margin = Padding.Empty,
            Text = $"Độc giả: {loan.TenDocGia} (mã {loan.MaDocGia})\r\nNgày mượn: {loan.NgayMuon:dd/MM/yyyy}    Hạn trả: {loan.HanTra:dd/MM/yyyy}    Trạng thái: {loan.TrangThai}" +
                (lateDays > 0 ? $"\r\nCòn quá hạn {lateDays} ngày." : string.Empty)
        };
        var header = new TableLayoutPanel
        {
            ColumnCount = 1, RowCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiTheme.PageBg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(summary, 0, 1);

        var grid = Ui.Grid();
        grid.Columns.Add("MaSach", "Mã sách");
        grid.Columns.Add("TenSach", "Tên sách");
        grid.Columns.Add("SoLuong", "Mượn / Đã trả / Còn");
        grid.Columns.Add("NgayTra", "Ngày trả · Tình trạng");
        grid.Columns.Add("TrangThai", "Trạng thái");
        grid.Columns.Add("TienPhat", "Phải thu / Đã thu / Còn nợ (đ)");
        grid.Columns[0].FillWeight = 55;
        grid.Columns[1].FillWeight = 145;
        grid.Columns[2].FillWeight = 90;
        grid.Columns[3].FillWeight = 150;
        grid.Columns[4].FillWeight = 100;
        grid.Columns[5].FillWeight = 130;
        foreach (var line in loan.ChiTiet)
        {
            var returns = line.LichSuTra;
            var returned = returns.Count == 0
                ? "Chưa trả"
                : string.Join("; ", returns.Select(x => $"{x.NgayTra:dd/MM/yyyy} ×{x.SoLuongXuLy} · {x.TinhTrangSach}"));
            var totalFine = returns.Sum(x => x.SoTienPhatPhaiThu ?? 0m);
            var paid = returns.Sum(x => x.DaThu);
            var state = line.SoLuongConMuon == 0 ? "Đã trả đủ"
                : loan.HanTra.Date < DateTime.Today ? "Quá hạn"
                : returns.Count > 0 ? "Trả một phần · đang mượn" : "Đang mượn";
            var row = grid.Rows.Add(line.MaSach, line.TenSach,
                $"{line.SoLuongMuon} / {line.SoLuongDaXuLy} / {line.SoLuongConMuon}",
                returned, state, $"{totalFine:N0} / {paid:N0} / {Math.Max(0, totalFine - paid):N0}");
            grid.Rows[row].Tag = line.MaChiTiet;
            if (state == "Quá hạn") grid.Rows[row].DefaultCellStyle.ForeColor = UiTheme.Danger;
        }
        var gridCard = new CardPanel("Sách trong phiếu", autoHeight: false);
        gridCard.AddRow(grid, fill: true);

        var close = Ui.Button("Đóng");
        close.DialogResult = DialogResult.Cancel;
        close.Margin = Padding.Empty;
        var actions = new FlowLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, BackColor = UiTheme.PageBg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        actions.Controls.Add(close);

        Controls.Add(Ui.Stack(1, header, gridCard, actions));
        CancelButton = close;
        Ui.ApplyDpi(this);
    }
}
