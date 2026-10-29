using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UI
{
    internal static class GridSelection
    {
        public static void Bind(DataGridView grid, Action<DataGridViewRow> loadRow,
            Action clearIdentity, params Button[] rowActions)
        {
            DataGridViewRow? loadedRow = null;
            void UpdateSelection()
            {
                var row = grid.CurrentRow;
                bool selected = row != null && !row.IsNewRow && row.Visible;
                foreach (var button in rowActions) button.Enabled = selected;

                if (!selected)
                {
                    loadedRow = null;
                    clearIdentity();
                    return;
                }

                // Đổi cột trong cùng dòng không được xóa nội dung đang nhập dở.
                if (ReferenceEquals(row, loadedRow)) return;
                loadedRow = row;
                loadRow(row!);
            }

            // SelectionChanged có thể chạy trước khi CurrentRow được cập nhật.
            grid.CurrentCellChanged += (s, e) => UpdateSelection();
            grid.Sorted += (s, e) => UpdateSelection();
            UpdateSelection();
        }

        public static void Clear(DataGridView grid)
        {
            grid.CurrentCell = null;
            grid.ClearSelection();
        }
    }
}
