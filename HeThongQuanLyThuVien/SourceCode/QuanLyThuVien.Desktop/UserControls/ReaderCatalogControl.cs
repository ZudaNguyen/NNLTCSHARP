using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class ReaderCatalogControl : UserControl
    {
        private readonly SachApiClient _api = new();
        private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 350 };
        private TextBox txtSearch = null!;
        private DataGridView grid = null!;
        private readonly Color paper = Color.FromArgb(247, 243, 233);
        private readonly Color card = Color.FromArgb(255, 252, 245);
        private readonly Color wood = Color.FromArgb(44, 30, 22);

        public ReaderCatalogControl()
        {
            BuildUi();
            Load += async (_, _) => await LoadBooksAsync();
            _searchTimer.Tick += async (_, _) => { _searchTimer.Stop(); await LoadBooksAsync(); };
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill; BackColor = paper; Padding = new Padding(20);
            var filter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = paper };
            filter.Controls.Add(new Label { Text = "Tra cứu sách:", Location = new Point(5, 16), AutoSize = true, Font = new Font("Georgia", 10, FontStyle.Bold), ForeColor = wood });
            txtSearch = new TextBox { Location = new Point(125, 12), Width = 360, Font = new Font("Segoe UI", 10) };
            txtSearch.TextChanged += (_, _) => { _searchTimer.Stop(); _searchTimer.Start(); };
            filter.Controls.Add(txtSearch);
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = card, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = card, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, EnableHeadersVisualStyles = false, Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 38, RowTemplate = { Height = 32 }, GridColor = Color.FromArgb(225, 215, 195) };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37); grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215); grid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9.5f, FontStyle.Bold);
            grid.DefaultCellStyle.BackColor = card; grid.DefaultCellStyle.ForeColor = wood; grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 195, 160); grid.DefaultCellStyle.SelectionForeColor = wood;
            grid.Columns.Add("Title", "Tên sách"); grid.Columns.Add("ISBN", "ISBN"); grid.Columns.Add("Genre", "Thể loại"); grid.Columns.Add("Authors", "Tác giả"); grid.Columns.Add("Year", "Năm XB"); grid.Columns.Add("Available", "Còn lại");
            panel.Controls.Add(grid); Controls.Add(panel); Controls.Add(filter);
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
