using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class ReaderCatalogControl : UserControl
    {
        private readonly SachApiClient _api = new();
        private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 350 };
        private TextBox txtSearch = null!;
        private DataGridView grid = null!;

        public ReaderCatalogControl()
        {
            BuildUi();
            Load += async (_, _) => await LoadBooksAsync();
            _searchTimer.Tick += async (_, _) => { _searchTimer.Stop(); await LoadBooksAsync(); };
        }

        private void BuildUi()
        {
            txtSearch = Ui.TextBox("Tên sách, ISBN hoặc tác giả", 360);
            txtSearch.TextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
            var toolbar = new ToolbarRow();
            toolbar.AddField("Tra cứu sách", txtSearch);

            grid = Ui.Grid();
            grid.Columns.Add("Title", "Tên sách");
            grid.Columns.Add("ISBN", "ISBN");
            grid.Columns.Add("Genre", "Thể loại");
            grid.Columns.Add("Authors", "Tác giả");
            grid.Columns.Add("Year", "Năm XB");
            grid.Columns.Add("Available", "Còn lại");

            var card = new CardPanel("Danh mục sách", autoHeight: false);
            card.AddRow(toolbar);
            card.AddRow(grid, fill: true);
            Ui.SetupPage(this, Ui.Stack(card));
        }

        private async Task LoadBooksAsync()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                var books = await _api.GetBooksAsync(txtSearch.Text, null, false, false, CancellationToken.None);
                grid.Rows.Clear();
                foreach (var book in books)
                    grid.Rows.Add(book.TenSach, book.ISBN, book.TenTheLoai, string.Join(", ", book.TenTacGia), book.NamXB, book.SoLuongCon);
            }
            catch (Exception ex) { if (!IsDisposed) MessageBox.Show(ex.Message, "Tra cứu sách", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
