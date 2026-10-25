using System.Drawing;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.Services;

internal static class DataGridViewStyleHelper
{
    private static readonly Color Burgundy = Color.FromArgb(122, 40, 34);
    private static readonly Color HeaderBrown = Color.FromArgb(74, 53, 37);
    private static readonly Color HeaderCream = Color.FromArgb(245, 235, 215);
    private static readonly Color Cream = Color.FromArgb(255, 252, 245);
    private static readonly Color AlternateCream = Color.FromArgb(247, 243, 233);

    public static void ApplyLibraryStyle(DataGridView grid)
    {
        grid.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.ReadOnly = true;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBrown;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = HeaderCream;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBrown;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = HeaderCream;
        grid.RowHeadersDefaultCellStyle.BackColor = HeaderBrown;
        grid.RowHeadersDefaultCellStyle.ForeColor = HeaderCream;
        grid.RowHeadersDefaultCellStyle.SelectionBackColor = HeaderBrown;
        grid.RowHeadersDefaultCellStyle.SelectionForeColor = HeaderCream;
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 42;
        grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, 32);
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.DefaultCellStyle.BackColor = Cream;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(44, 30, 22);
        grid.DefaultCellStyle.SelectionBackColor = Burgundy;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.RowsDefaultCellStyle.BackColor = Cream;
        grid.RowsDefaultCellStyle.ForeColor = Color.FromArgb(44, 30, 22);
        grid.RowsDefaultCellStyle.SelectionBackColor = Burgundy;
        grid.RowsDefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = AlternateCream;
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Burgundy;
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
        grid.ShowCellToolTips = true;
        grid.ColumnAdded -= ApplyColumnStyle;
        grid.ColumnAdded += ApplyColumnStyle;
        foreach (DataGridViewColumn column in grid.Columns)
            ApplyColumnStyle(grid, new DataGridViewColumnEventArgs(column));
        grid.RowsAdded -= ApplyRowStyle;
        grid.RowsAdded += ApplyRowStyle;
        foreach (DataGridViewRow row in grid.Rows)
            ApplyRowStyle(grid, new DataGridViewRowsAddedEventArgs(row.Index, 1));
        grid.CellToolTipTextNeeded -= SetCellToolTip;
        grid.CellToolTipTextNeeded += SetCellToolTip;
    }

    private static void ApplyColumnStyle(object? sender, DataGridViewColumnEventArgs e)
    {
        var column = e.Column;
        column.DefaultCellStyle.SelectionBackColor = Burgundy;
        column.DefaultCellStyle.SelectionForeColor = Color.White;
        var measured = TextRenderer.MeasureText(column.HeaderText, column.DataGridView?.ColumnHeadersDefaultCellStyle.Font ?? SystemFonts.MessageBoxFont).Width + 24;
        column.MinimumWidth = Math.Max(column.MinimumWidth, Math.Min(220, Math.Max(72, measured)));
        column.HeaderCell.Style.SelectionBackColor = HeaderBrown;
        column.HeaderCell.Style.SelectionForeColor = HeaderCream;
    }

    private static void ApplyRowStyle(object? sender, DataGridViewRowsAddedEventArgs e)
    {
        if (sender is not DataGridView grid) return;
        for (var i = e.RowIndex; i < Math.Min(grid.Rows.Count, e.RowIndex + e.RowCount); i++)
        {
            var row = grid.Rows[i];
            row.DefaultCellStyle.SelectionBackColor = Burgundy;
            row.DefaultCellStyle.SelectionForeColor = Color.White;
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.SelectionBackColor = Burgundy;
                cell.Style.SelectionForeColor = Color.White;
            }
        }
    }

    private static void SetCellToolTip(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            e.ToolTipText = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue?.ToString() ?? string.Empty;
    }
}
