using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls;

public partial class BaoCaoControl : UserControl
{
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
        var today = DateTime.Today;

        // ----- Bộ lọc báo cáo -----
        _reportType = Ui.ComboBox(new object[]
        {
            "1. Sách đang mượn và quá hạn",
            "2. Sách được mượn nhiều nhất",
            "3. Thu tiền phạt quá hạn / hư hỏng"
        }, width: 270);
        _from = Ui.DatePicker(new DateTime(today.Year, today.Month, 1), 125);
        _from.MaxDate = DateTime.Today.AddYears(10);
        _to = Ui.DatePicker(today, 125);
        _to.MaxDate = DateTime.Today.AddYears(10);
        _load = Ui.Button("Xem báo cáo", ButtonKind.Primary);
        _export = Ui.Button("Xuất CSV");
        _load.Click += async (_, _) => await LoadReportDataAsync();
        _export.Click += ExportReport_Click;
        _reportType.SelectedIndexChanged += async (_, _) => await LoadReportDataAsync();

        var toolbar = new ToolbarRow();
        toolbar.AddField("Loại báo cáo", _reportType);
        toolbar.AddField("Từ ngày", _from);
        toolbar.AddField("Đến ngày", _to);

        var filterCard = new CardPanel("Báo cáo & thống kê");
        filterCard.AddRow(toolbar);
        filterCard.AddRow(Ui.ButtonBar(_load, _export));

        // ----- Kết quả -----
        _grid = Ui.Grid();
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.AutoGenerateColumns = false;

        _summary = new Label
        {
            Text = "Tổng kết: chưa tải dữ liệu.", AutoSize = true, MaximumSize = new Size(1100, 0),
            Font = UiTheme.Font(9.5f, FontStyle.Bold), ForeColor = UiTheme.Text, Margin = new Padding(0, 10, 0, 2)
        };
        _status = new Label
        {
            Text = string.Empty, AutoSize = true, MaximumSize = new Size(1100, 0),
            Font = UiTheme.Font(9.5f), ForeColor = UiTheme.TextMuted, Margin = new Padding(0, 0, 0, 0)
        };

        var resultCard = new CardPanel("Kết quả", autoHeight: false);
        resultCard.AddRow(_grid, fill: true);
        resultCard.AddRow(_summary);
        resultCard.AddRow(_status);

        Ui.SetupPage(this, Ui.ScrollStack(260, filterCard, resultCard));
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
                        (nameof(SachDangMuonBaoCaoDto.MaPhieu), "Mã phiếu"),
                        (nameof(SachDangMuonBaoCaoDto.TenDocGia), "Độc giả"),
                        (nameof(SachDangMuonBaoCaoDto.MaSach), "Mã sách"),
                        (nameof(SachDangMuonBaoCaoDto.TenSach), "Tên sách"),
                        (nameof(SachDangMuonBaoCaoDto.NgayMuon), "Ngày mượn"),
                        (nameof(SachDangMuonBaoCaoDto.HanTra), "Hạn trả"),
                        (nameof(SachDangMuonBaoCaoDto.SoLuongConMuon), "SL còn mượn"),
                        (nameof(SachDangMuonBaoCaoDto.SoNgayQuaHan), "Ngày quá hạn"),
                        (nameof(SachDangMuonBaoCaoDto.TrangThai), "Tình trạng"));
                    _summary.Text = $"Tổng kết: {rows.Count:N0} dòng chi tiết đang mượn; {rows.Count(x => x.SoNgayQuaHan > 0):N0} dòng quá hạn. Lọc theo ngày mượn.";
                    break;
                }
                case 1:
                {
                    var rows = await _api.GetSachMuonNhieuAsync(from, to, 10, CancellationToken.None);
                    BindRows(rows,
                        (nameof(SachMuonNhieuDto.XepHang), "Thứ hạng"),
                        (nameof(SachMuonNhieuDto.MaSach), "Mã sách"),
                        (nameof(SachMuonNhieuDto.ISBN), "ISBN"),
                        (nameof(SachMuonNhieuDto.TenSach), "Tên sách"),
                        (nameof(SachMuonNhieuDto.TenTheLoai), "Thể loại"),
                        (nameof(SachMuonNhieuDto.SoLuotMuon), "Lượt mượn"),
                        (nameof(SachMuonNhieuDto.TongSoBanMuon), "Tổng bản đã mượn"));
                    _summary.Text = $"Tổng kết: Top {rows.Count:N0}; lượt mượn là số lần đầu sách xuất hiện trong chi tiết phiếu, tổng bản là tổng số lượng xuất mượn.";
                    break;
                }
                default:
                {
                    var report = await _api.GetTienPhatAsync(from, to, CancellationToken.None);
                    BindRows(report.ChiTiet,
                        (nameof(TienPhatBaoCaoDongDto.MaPhieu), "Mã phiếu"),
                        (nameof(TienPhatBaoCaoDongDto.TenDocGia), "Độc giả"),
                        (nameof(TienPhatBaoCaoDongDto.TenSach), "Sách"),
                        (nameof(TienPhatBaoCaoDongDto.NgayPhatSinh), "Ngày phát sinh"),
                        (nameof(TienPhatBaoCaoDongDto.LyDo), "Lý do"),
                        (nameof(TienPhatBaoCaoDongDto.PhatSinhTrongKy), "Phát sinh trong kỳ"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienPhatPhaiThu), "Tổng phải thu"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienDaThuTrongKy), "Đã thu trong kỳ"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienDaThuLuyKe), "Đã thu lũy kế"),
                        (nameof(TienPhatBaoCaoDongDto.SoTienConNo), "Còn nợ (VNĐ)"),
                        (nameof(TienPhatBaoCaoDongDto.NgayThuGanNhat), "Ngày thu gần nhất"),
                        (nameof(TienPhatBaoCaoDongDto.TrangThaiThanhToan), "Trạng thái"));
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
            var width = column.DataPropertyName.Contains("TenSach", StringComparison.Ordinal) ? 230
                : column.DataPropertyName.Contains("TenDocGia", StringComparison.Ordinal) ? 180
                : column.DataPropertyName.Contains("SoTien", StringComparison.Ordinal) ? 150
                : column.DataPropertyName.Contains("Ngay", StringComparison.Ordinal) ? 125
                : 140;
            column.Width = Ui.Px(width);
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
