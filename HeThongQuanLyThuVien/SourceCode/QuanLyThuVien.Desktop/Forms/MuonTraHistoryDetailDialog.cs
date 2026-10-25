using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.Forms;

internal sealed class MuonTraHistoryDetailDialog : Form
{
    public MuonTraHistoryDetailDialog(MuonTraChiTietDto loan)
    {
        Text = $"Chi tiết lịch sử — {loan.SoPhieu}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(820, 540);
        Size = new Size(1060, 690);
        BackColor = Color.FromArgb(247, 243, 233);
        Font = new Font("Segoe UI", 9.5f);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
            Padding = new Padding(12), BackColor = BackColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var title = new Label
        {
            Dock = DockStyle.Fill, Text = $"CHI TIẾT PHIẾU {loan.SoPhieu}",
            Font = new Font("Georgia", 12, FontStyle.Bold), ForeColor = Color.FromArgb(122, 40, 34),
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(title, 0, 0);

        var lateDays = loan.SoLuongConMuon > 0 && loan.HanTra.Date < DateTime.Today
            ? (DateTime.Today - loan.HanTra.Date).Days : 0;
        var summary = new Label
        {
            Dock = DockStyle.Fill,
            Text = $"Độc giả: {loan.TenDocGia} (mã {loan.MaDocGia})\r\nNgày mượn: {loan.NgayMuon:dd/MM/yyyy}    Hạn trả: {loan.HanTra:dd/MM/yyyy}    Trạng thái: {loan.TrangThai}" +
                (lateDays > 0 ? $"\r\nCòn quá hạn {lateDays} ngày." : string.Empty),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(44, 30, 22),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4), AutoEllipsis = true
        };
        layout.Controls.Add(summary, 0, 1);

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            RowHeadersVisible = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.FromArgb(255, 252, 245), BorderStyle = BorderStyle.FixedSingle,
            EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9.5f),
            ColumnHeadersHeight = 44, RowTemplate = { Height = 34 }
        };
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
            if (state == "Quá hạn") grid.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(160, 55, 35);
        }
        DataGridViewStyleHelper.ApplyLibraryStyle(grid);
        layout.Controls.Add(grid, 0, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, Padding = new Padding(0, 5, 0, 0)
        };
        var close = new Button
        {
            Text = "Đóng", Width = 100, Height = 32, DialogResult = DialogResult.Cancel,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = Color.FromArgb(90, 75, 65),
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        close.FlatAppearance.BorderSize = 0;
        actions.Controls.Add(close);
        layout.Controls.Add(actions, 0, 3);
        Controls.Add(layout);
        CancelButton = close;
    }
}
