using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls;

public partial class BaoCaoControl : UserControl
{
    private static readonly Color PaperCream = Color.FromArgb(247, 243, 233);
    private static readonly Color CardBg = Color.FromArgb(255, 252, 245);
    private static readonly Color WoodDark = Color.FromArgb(44, 30, 22);
    private static readonly Color Burgundy = Color.FromArgb(122, 40, 34);
    private static readonly Color ForestGreen = Color.FromArgb(45, 106, 79);
    private readonly ThongKeApiClient _api = new();
    private ComboBox _reportType = null!;
    private DateTimePicker _from = null!;
    private DateTimePicker _to = null!;
    private DataGridView _grid = null!;
    private Label _summary = null!;
    private Label _status = null!;
    private Button _load = null!;
    private Button _export = null!;

    public BaoCaoControl()
    {
        InitializeComponent();
        SetupBaoCaoUI();
        Load += async (_, _) => await LoadReportDataAsync();
    }

    private void SetupBaoCaoUI()
    {
        Dock = DockStyle.Fill;
        BackColor = PaperCream;
        Padding = new Padding(20);
        var today = DateTime.Today;

        var filter = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(12), Margin = Padding.Empty };
        filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        var title = new Label
        {
            Text = "📊 Trích Xuất Sổ Báo Cáo & Thống Kê Nghiệp Vụ", Font = new Font("Georgia", 11.5f, FontStyle.Bold),
            ForeColor = Burgundy, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
        };
        filter.Controls.Add(title, 0, 0);
        var primaryFilters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = false, FlowDirection = FlowDirection.LeftToRight, Margin = Padding.Empty, Padding = Padding.Empty };
        var lblType = MakeLabel("Loại báo cáo:", 0, 0); lblType.Width = 100; lblType.Height = 34; lblType.TextAlign = ContentAlignment.MiddleLeft; lblType.Margin = new Padding(3, 2, 4, 2);
        _reportType = new ComboBox { Width = 560, Height = 34, Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 4, 6, 2), Anchor = AnchorStyles.Left | AnchorStyles.Right };
        _reportType.Items.AddRange([
            "1. Danh sách Sách đang mượn & Quá hạn",
            "2. Thống kê Sách mượn nhiều nhất (Top Ấn phẩm)",
            "3. Báo cáo Thu tiền phạt quá hạn / hư hỏng"
        ]);
        _reportType.SelectedIndex = 0;
        primaryFilters.Controls.AddRange([lblType, _reportType]);
        filter.Controls.Add(primaryFilters, 0, 1);
        var dateActions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = false, FlowDirection = FlowDirection.LeftToRight, Margin = Padding.Empty, Padding = Padding.Empty };
        var lblFrom = MakeLabel("Từ ngày:", 0, 0); lblFrom.Width = 64; lblFrom.Height = 34; lblFrom.TextAlign = ContentAlignment.MiddleLeft; lblFrom.Margin = new Padding(3, 2, 2, 2);
        _from = new DateTimePicker { Width = 125, Format = DateTimePickerFormat.Short, Value = new DateTime(today.Year, today.Month, 1), MaxDate = DateTime.Today.AddYears(10), Margin = new Padding(3, 4, 10, 2) };
        var lblTo = MakeLabel("Đến ngày:", 0, 0); lblTo.Width = 70; lblTo.Height = 34; lblTo.TextAlign = ContentAlignment.MiddleLeft; lblTo.Margin = new Padding(3, 2, 2, 2);
        _to = new DateTimePicker { Width = 125, Format = DateTimePickerFormat.Short, Value = today, MaxDate = DateTime.Today.AddYears(10), Margin = new Padding(3, 4, 10, 2) };
        _load = MakeButton("🔍 Xem báo cáo", 0, 0, 145, Color.FromArgb(180, 120, 20)); _load.Margin = new Padding(3, 2, 8, 2);
        _export = MakeButton("📥 Xuất CSV", 0, 0, 135, ForestGreen); _export.Margin = new Padding(3, 2, 8, 2);
        _load.Click += async (_, _) => await LoadReportDataAsync();
        _export.Click += ExportReport_Click;
        _reportType.SelectedIndexChanged += async (_, _) => await LoadReportDataAsync();
        dateActions.Controls.AddRange([lblFrom, _from, lblTo, _to, _load, _export]);
        filter.Controls.Add(dateActions, 0, 2);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 66, BackColor = Color.FromArgb(238, 230, 212), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(15, 8, 15, 0) };
        _summary = new Label { Text = "Tổng kết: Chưa tải dữ liệu.", Font = new Font("Georgia", 9.5f, FontStyle.Bold | FontStyle.Italic), ForeColor = Burgundy, Dock = DockStyle.Top, Height = 26 };
        _status = new Label { Text = "", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(90, 70, 55), Dock = DockStyle.Fill, AutoEllipsis = true };
        footer.Controls.Add(_status);
        footer.Controls.Add(_summary);

        var gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(12) };
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill, BackgroundColor = CardBg, BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, EnableHeadersVisualStyles = false,
            Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 38, RowTemplate = { Height = 32 },
            GridColor = Color.FromArgb(225, 215, 195), AutoGenerateColumns = false
        };
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
        QuanLyThuVien.Desktop.Services.DataGridViewStyleHelper.ApplyLibraryStyle(_grid);
        gridPanel.Controls.Add(_grid);

        var pageLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = PaperCream, Margin = Padding.Empty, Padding = Padding.Empty };
        pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        pageLayout.Controls.Add(filter, 0, 0);
        pageLayout.Controls.Add(gridPanel, 0, 1);
        pageLayout.Controls.Add(footer, 0, 2);
        Controls.Add(pageLayout);
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text = text, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark,
        Location = new Point(x, y), AutoSize = true
    };

    private static Button MakeButton(string text, int x, int y, int width, Color color)
    {
        var button = new Button
        {
            Text = text, Location = new Point(x, y), Size = new Size(width, 34),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = color,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private async Task LoadReportDataAsync()
    {
        if (_grid is null || _grid.IsDisposed || _load.Enabled == false) return;
        var from = DateOnly.FromDateTime(_from.Value.Date);
        var to = DateOnly.FromDateTime(_to.Value.Date);
        if (from > to)
        {
            MessageBox.Show("Ngày bắt đầu không được sau ngày kết thúc.", "Khoảng thời gian không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetLoading(true);
        _status.Text = "Đang truy vấn dữ liệu từ API...";
        try
        {
            switch (_reportType.SelectedIndex)
            {
                case 0:
                {
                    var rows = await _api.GetSachDangMuonAsync(from, to, CancellationToken.None);
                    BindRows(rows,
                        (nameof(SachDangMuonBaoCaoDto.MaPhieu), "Mã Phiếu"),
                        (nameof(SachDangMuonBaoCaoDto.TenDocGia), "Độc Giả"),
                        (nameof(SachDangMuonBaoCaoDto.MaSach), "Mã Sách"),
                        (nameof(SachDangMuonBaoCaoDto.TenSach), "Tên Ấn Phẩm"),
                        (nameof(SachDangMuonBaoCaoDto.NgayMuon), "Ngày Mượn"),
                        (nameof(SachDangMuonBaoCaoDto.HanTra), "Hạn Trả"),
                        (nameof(SachDangMuonBaoCaoDto.SoLuongConMuon), "SL Còn Mượn"),
                        (nameof(SachDangMuonBaoCaoDto.SoNgayQuaHan), "Ngày Quá Hạn"),
                        (nameof(SachDangMuonBaoCaoDto.TrangThai), "Tình Trạng"));
                    _summary.Text = $"Tổng kết: {rows.Count:N0} dòng chi tiết đang mượn; {rows.Count(x => x.SoNgayQuaHan > 0):N0} dòng quá hạn. Lọc theo ngày mượn.";
                    break;
                }
                case 1:
                {
                    var rows = await _api.GetSachMuonNhieuAsync(from, to, 10, CancellationToken.None);
                    BindRows(rows,
                        (nameof(SachMuonNhieuDto.XepHang), "Thứ Hạng"),
                        (nameof(SachMuonNhieuDto.MaSach), "Mã Sách"),
                        (nameof(SachMuonNhieuDto.ISBN), "Mã ISBN"),
                        (nameof(SachMuonNhieuDto.TenSach), "Tên Ấn Phẩm"),
                        (nameof(SachMuonNhieuDto.TenTheLoai), "Thể Loại"),
                        (nameof(SachMuonNhieuDto.SoLuotMuon), "Lượt Mượn"),
                        (nameof(SachMuonNhieuDto.TongSoBanMuon), "Tổng Bản Đã Mượn"));
                    _summary.Text = $"Tổng kết: Top {rows.Count:N0}; lượt mượn là số lần đầu sách xuất hiện trong chi tiết phiếu, tổng bản là tổng số lượng xuất mượn.";
                    break;
                }
                default:
                {
                    var report = await _api.GetTienPhatAsync(from, to, CancellationToken.None);
                    BindRows(report.ChiTiet,
                        (nameof(TienPhatBaoCaoDongDto.MaPhieu), "Mã Phiếu"),
                        (nameof(TienPhatBaoCaoDongDto.TenDocGia), "Độc Giả"),
                        (nameof(TienPhatBaoCaoDongDto.TenSach), "Ấn Phẩm"),
                        (nameof(TienPhatBaoCaoDongDto.NgayPhatSinh), "Ngày Phát Sinh"),
                        (nameof(TienPhatBaoCaoDongDto.LyDo), "Lý Do"),
                        (nameof(TienPhatBaoCaoDongDto.PhatSinhTrongKy), "Phát Sinh Trong Kỳ"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienPhatPhaiThu), "Tổng Phải Thu"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienDaThuTrongKy), "Đã Thu Trong Kỳ"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienDaThuLuyKe), "Đã Thu Lũy Kế"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienConNo), "Còn Nợ (VNĐ)"),
                        (nameof(TienPhatBaoCaoDongDto.NgayThuGanNhat), "Ngày Thu Gần Nhất"),
                        (nameof(TienPhatBaoCaoDongDto.TrangThaiThanhToan), "Trạng Thái"));
                    _summary.Text = $"Tổng kết {from:dd/MM/yyyy}–{to:dd/MM/yyyy}: phát sinh {report.TongPhatSinh:N0} VNĐ; đã thu trong kỳ {report.TongDaThuTrongKy:N0} VNĐ; còn nợ của khoản phát sinh kỳ này {report.TongConNo:N0} VNĐ.";
                    _status.Text = $"Phát sinh lọc theo {report.TruongNgayLoc}; tiền thu lọc theo ngày giao dịch ThuTienPhat. Khoản cũ có giao dịch thu trong kỳ cũng được hiển thị.";
                    break;
                }
            }
            if (_grid.Rows.Count == 0) _status.Text = "Không có dữ liệu trong khoảng thời gian đã chọn.";
            else if (_reportType.SelectedIndex != 2) _status.Text = $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}.";
        }
        catch (HttpRequestException ex)
        {
            _status.Text = ex.Message;
            MessageBox.Show(ex.Message, "Lỗi API báo cáo", MessageBoxButtons.OK, ex.StatusCode == HttpStatusCode.Forbidden ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            _status.Text = "Không thể tải dữ liệu báo cáo.";
            MessageBox.Show($"Không thể tải báo cáo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { SetLoading(false); }
    }

    private void BindRows<T>(IReadOnlyList<T> rows, params (string Property, string Header)[] columns)
    {
        _grid.DataSource = null;
        _grid.Columns.Clear();
        foreach (var (property, header) in columns)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = header, Name = property, SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.DataSource = rows.ToList();
        foreach (DataGridViewColumn column in _grid.Columns)
        {
            if (column.DataPropertyName.Contains("Ngay", StringComparison.Ordinal)) column.DefaultCellStyle.Format = "dd/MM/yyyy";
            if (column.DataPropertyName.Contains("SoTien", StringComparison.Ordinal)) column.DefaultCellStyle.Format = "N0";
            column.Width = column.DataPropertyName.Contains("TenSach", StringComparison.Ordinal) ? 230
                : column.DataPropertyName.Contains("TenDocGia", StringComparison.Ordinal) ? 180
                : column.DataPropertyName.Contains("SoTien", StringComparison.Ordinal) ? 150
                : column.DataPropertyName.Contains("Ngay", StringComparison.Ordinal) ? 115
                : 135;
        }
    }

    private void SetLoading(bool loading)
    {
        _load.Enabled = !loading;
        _export.Enabled = !loading;
        _reportType.Enabled = !loading;
        _from.Enabled = !loading;
        _to.Enabled = !loading;
    }

    private void ExportReport_Click(object? sender, EventArgs e)
    {
        if (_grid.Rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất báo cáo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var reportName = _reportType.SelectedIndex switch { 0 => "SachDangMuon", 1 => "SachMuonNhieu", _ => "TienPhat" };
        using var dialog = new SaveFileDialog { Filter = "CSV Excel File (*.csv)|*.csv", FileName = $"BaoCao_{reportName}_{DateTime.Now:yyyyMMdd_HHmm}.csv", AddExtension = true, DefaultExt = "csv" };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", _grid.Columns.Cast<DataGridViewColumn>().Select(column => EscapeCsv(column.HeaderText))));
            foreach (DataGridViewRow row in _grid.Rows)
            {
                var values = new List<string>(_grid.Columns.Count);
                foreach (DataGridViewCell cell in row.Cells)
                {
                    var value = Convert.ToString(cell.FormattedValue, CultureInfo.CurrentCulture) ?? string.Empty;
                    var firstNonWhitespace = value.TrimStart(' ', '\t', '\r', '\n').FirstOrDefault();
                    if (cell.Value is string && firstNonWhitespace is '=' or '+' or '-' or '@') value = "'" + value;
                    values.Add(EscapeCsv(value));
                }
                csv.AppendLine(string.Join(",", values));
            }
            File.WriteAllText(dialog.FileName, csv.ToString(), new UTF8Encoding(true));
            MessageBox.Show($"Đã xuất báo cáo tại:\n{dialog.FileName}", "Xuất báo cáo thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Không lưu được file. Hãy kiểm tra thư mục đích hoặc đóng file đang mở.\n{ex.Message}", "Lỗi lưu file", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Không có quyền ghi file tại đường dẫn đã chọn.\n{ex.Message}", "Lỗi lưu file", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string EscapeCsv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
