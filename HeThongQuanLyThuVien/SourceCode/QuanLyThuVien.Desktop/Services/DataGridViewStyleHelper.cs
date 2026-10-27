using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.Services;

/// <summary>Áp kiểu bảng dữ liệu thống nhất: tiêu đề xám nhạt, dòng kẻ ngang mảnh, dòng chọn xanh nhạt.</summary>
internal static class DataGridViewStyleHelper
{
    public static void ApplyLibraryStyle(DataGridView grid)
    {
        grid.BackgroundColor = UiTheme.Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = UiTheme.TableGrid;
        grid.Font = UiTheme.Font(9.5f);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ShowCellToolTips = true;

        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        var header = grid.ColumnHeadersDefaultCellStyle;
        header.BackColor = UiTheme.TableHeaderBg;
        header.ForeColor = UiTheme.TableHeaderText;
        header.SelectionBackColor = UiTheme.TableHeaderBg;
        header.SelectionForeColor = UiTheme.TableHeaderText;
        header.Font = UiTheme.Font(9.5f, FontStyle.Bold);
        header.Alignment = DataGridViewContentAlignment.MiddleLeft;
        header.WrapMode = DataGridViewTriState.False;
        header.Padding = new Padding(Ui.Px(8), Ui.Px(8), Ui.Px(8), Ui.Px(8));

        grid.RowTemplate.Height = 34;
        var body = grid.DefaultCellStyle;
        body.BackColor = UiTheme.Surface;
        body.ForeColor = UiTheme.Text;
        body.SelectionBackColor = UiTheme.PrimarySoft;
        body.SelectionForeColor = UiTheme.Text;
        body.WrapMode = DataGridViewTriState.False;
        body.Padding = new Padding(Ui.Px(8), 0, Ui.Px(8), 0);

        var alternate = grid.AlternatingRowsDefaultCellStyle;
        alternate.BackColor = UiTheme.TableAltRow;
        alternate.ForeColor = UiTheme.Text;
        alternate.SelectionBackColor = UiTheme.PrimarySoft;
        alternate.SelectionForeColor = UiTheme.Text;

        grid.ColumnAdded -= ApplyColumnStyle;
        grid.ColumnAdded += ApplyColumnStyle;
        foreach (DataGridViewColumn column in grid.Columns)
            ApplyColumnStyle(grid, new DataGridViewColumnEventArgs(column));
        grid.CellToolTipTextNeeded -= SetCellToolTip;
        grid.CellToolTipTextNeeded += SetCellToolTip;
    }

    private static void ApplyColumnStyle(object? sender, DataGridViewColumnEventArgs e)
    {
        var column = e.Column;
        var headerFont = column.DataGridView?.ColumnHeadersDefaultCellStyle.Font ?? UiTheme.Font(9.5f, FontStyle.Bold);
        var measured = TextRenderer.MeasureText(column.HeaderText, headerFont).Width + Ui.Px(30);
        column.MinimumWidth = Math.Max(column.MinimumWidth, Math.Min(Ui.Px(220), Math.Max(Ui.Px(64), measured)));
    }

    private static void SetCellToolTip(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            e.ToolTipText = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue?.ToString() ?? string.Empty;
    }
}
