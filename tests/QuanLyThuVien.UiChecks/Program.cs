using System.ComponentModel;
using System.Reflection;
using QuanLyThuVien.Desktop.Forms;
using QuanLyThuVien.Desktop.Models;
using QuanLyThuVien.Desktop.UserControls;

namespace QuanLyThuVien.UiChecks;

internal static class Program
{
    private static int _failed;

    [STAThread]
    private static int Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        ApplicationConfiguration.Initialize();
        Check("Book selection and edit actions", () => CheckSelection(new SachControl(), "dgvSach", "txtMaSach", "MaSach", "txtTenSach", "Tu Chỉnh", "Lưu Kho"));
        Check("Reader selection and edit actions", () => CheckSelection(new DocGiaControl(), "dgvDocGia", "txtMaDocGia", "MaDocGia", "txtHoTen", "Cập Nhật", "Khóa / Mở"));
        Check("Employee selection and edit actions", () => CheckSelection(new NhanVienControl("admin"), "dgvNhanVien", "txtMaNhanVien", "MaNV", "txtHoTen", "Cập Nhật", "Thu Hồi"));
        Check("Category and author selection", CheckCatalogSelection);
        Check("Loan/return selection", CheckLoanSelection);
        Check("Navigation preserves data, draft and filters; closes cached views", CheckNavigation);
        foreach (float scale in new[] { 1f, 1.25f, 1.5f })
            Check($"All screens remain accessible at scale {scale}", () => CheckLayouts(scale));
        Console.WriteLine(_failed == 0 ? "All UI checks passed." : $"{_failed} UI check(s) failed.");
        return _failed == 0 ? 0 : 1;
    }

    private static void Check(string name, Action test)
    {
        try { test(); Console.WriteLine($"PASS: {name}"); }
        catch (Exception ex) { _failed++; Console.WriteLine($"FAIL: {name}\n{ex}"); }
    }

    private static T Field<T>(object target, string name) =>
        (T)(target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(target) ?? throw new InvalidOperationException(name));

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    private static void Prepare(Control control)
    {
        _ = control.Handle; // Create handles without showing a window.
        foreach (Control child in control.Controls) Prepare(child);
        control.PerformLayout();
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Click(Button button) => typeof(Button).GetMethod("OnClick",
        BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, new object[] { EventArgs.Empty });

    private static Button ButtonContaining(Control view, string text) =>
        Descendants(view).OfType<Button>().Single(b => b.Text.Contains(text));

    private static void CheckSelection(UserControl view, string gridName, string identityName,
        string keyColumn, string inputName, params string[] actionNames)
    {
        using (view)
        {
            view.Size = new Size(1100, 750);
            Prepare(view);
            var grid = Field<DataGridView>(view, gridName);
            var identity = Field<TextBox>(view, identityName);
            var input = Field<TextBox>(view, inputName);
            var actions = actionNames.Select(name => ButtonContaining(view, name)).ToArray();
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.CurrentCell = grid.Rows[1].Cells[0]; // Same path as keyboard navigation.
            Assert(identity.Text == grid.CurrentRow!.Cells[keyColumn].Value?.ToString(), "Form points to a different row.");
            Assert(actions.All(b => b.Enabled), "Actions should be enabled for a selected row.");
            input.Text = "Unsaved draft";
            grid.CurrentCell = grid.CurrentRow.Cells[1];
            Assert(input.Text == "Unsaved draft", "Moving within a row discarded the draft.");
            grid.Sort(grid.Columns[keyColumn]!, ListSortDirection.Descending);
            Assert(identity.Text == grid.CurrentRow!.Cells[keyColumn].Value?.ToString(), "Selection changed after sorting.");
            grid.CurrentCell = null;
            Assert(identity.Text.Length == 0 && actions.All(b => !b.Enabled), "Actions remain enabled without a selected row.");
        }
    }

    private static void CheckCatalogSelection()
    {
        using var view = new TacGiaTheLoaiControl { Size = new Size(1100, 750) };
        Prepare(view);
        foreach (var names in new[] { ("dgvTheLoai", "txtMaTheLoai", "MaTL"), ("dgvTacGia", "txtMaTacGia", "MaTG") })
        {
            var grid = Field<DataGridView>(view, names.Item1);
            var identity = Field<TextBox>(view, names.Item2);
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.CurrentCell = grid.Rows[1].Cells[0];
            Assert(identity.Text == grid.CurrentRow!.Cells[names.Item3].Value?.ToString(), "Catalog form does not follow selection.");
            grid.CurrentCell = null;
            Assert(identity.Text.Length == 0, "Catalog identity remains selected.");
        }
    }

    private static void CheckLoanSelection()
    {
        using var view = new MuonTraControl("admin") { Size = new Size(1100, 750) };
        Prepare(view);
        var master = Field<DataGridView>(view, "dgvPhieuMuon");
        var detail = Field<DataGridView>(view, "dgvChiTiet");
        var label = Field<Label>(view, "lblSachDangChon");
        var confirm = Field<Button>(view, "btnXacNhanTra");
        var condition = Field<ComboBox>(view, "cboTinhTrangTra");
        master.CurrentCell = master.Rows[0].Cells[0];
        detail.CurrentCell = detail.Rows[0].Cells[0];
        Assert(confirm.Enabled && label.Text.Contains(detail.CurrentRow!.Cells["TenSach"].Value!.ToString()!), "Return selection is out of sync.");
        condition.SelectedIndex = 1;
        detail.CurrentCell = detail.Rows[1].Cells[0];
        Assert(!confirm.Enabled && !condition.Enabled, "Returned book can be returned again.");
        detail.CurrentCell = detail.Rows[0].Cells[0];
        Assert(confirm.Enabled && condition.SelectedIndex == 0, "Condition from previous book leaked into new selection.");
        detail.Sort(detail.Columns["TenSach"]!, ListSortDirection.Descending);
        Assert(label.Text.Contains(detail.CurrentRow!.Cells["TenSach"].Value!.ToString()!), "Return label does not match sorted selection.");
        master.CurrentCell = master.Rows[1].Cells[0];
        Assert(detail.Rows.Count == 1 && !confirm.Enabled, "Previous return selection survived a different loan.");
        master.CurrentCell = null;
        Assert(detail.Rows.Count == 0 && !confirm.Enabled, "Details remain active without a loan.");
        Field<TextBox>(view, "txtTimPhieu").Text = "no matching loan";
        Assert(!master.Rows.Cast<DataGridViewRow>().Any(r => r.Visible) && !confirm.Enabled, "Empty filter leaves return action enabled.");
    }

    private static void CheckNavigation()
    {
        var form = new MainForm(Roles.Admin, "admin");
        var cached = Field<Dictionary<Type, UserControl>>(form, "_views");
        try
        {
            Prepare(form);
            Click(ButtonContaining(form, "Kho Sách"));
            var books = (SachControl)cached[typeof(SachControl)];
            var grid = Field<DataGridView>(books, "dgvSach");
            grid.Rows.Add("99", "9780000000000", "Added during this session", "Kỹ năng sống", "Dale Carnegie", "2026", "", "1", "1", "Hoạt động");
            int count = grid.Rows.Count;
            Field<TextBox>(books, "txtTimKiem").Text = "Clean";
            Field<TextBox>(books, "txtTenSach").Text = "Draft book title";
            Click(ButtonContaining(form, "Thẻ Độc giả"));
            Click(ButtonContaining(form, "Kho Sách"));
            Assert(ReferenceEquals(books, cached[typeof(SachControl)]), "Navigation recreated the screen.");
            Assert(grid.Rows.Count == count, "Session data was discarded.");
            Assert(Field<TextBox>(books, "txtTenSach").Text == "Draft book title", "Draft was discarded.");
            Assert(Field<TextBox>(books, "txtTimKiem").Text == "Clean", "Filter was discarded.");
            Click(ButtonContaining(form, "Kho Sách"));
            Assert(Field<TextBox>(books, "txtTenSach").Text == "Draft book title", "Clicking the active menu discarded the draft.");
            foreach (var menu in new[] { "Tác giả & Thể loại", "Sổ Mượn", "Quản lý Thủ thư", "Thống kê & Báo cáo" })
                Click(ButtonContaining(form, menu));
            Assert(cached.Count == 7, "Unexpected screen cache count.");
        }
        finally { form.Dispose(); }
        Assert(cached.Values.All(v => v.IsDisposed), "Cached screens were not disposed with MainForm.");

        using var librarian = new MainForm(Roles.ThuThu, "thuthu01");
        Click(ButtonContaining(librarian, "Quản lý Thủ thư"));
        Assert(!Field<Dictionary<Type, UserControl>>(librarian, "_views").ContainsKey(typeof(NhanVienControl)), "Employee screen bypassed role check.");
    }

    private static void CheckLayouts(float scale)
    {
        Func<UserControl>[] factories = [() => new DashboardControl(), () => new SachControl(),
            () => new DocGiaControl(), () => new NhanVienControl("admin"),
            () => new TacGiaTheLoaiControl(), () => new MuonTraControl("admin"), () => new BaoCaoControl()];
        foreach (var factory in factories)
        {
            using var view = factory();
            view.Size = new Size(730, 500);
            Prepare(view);
            if (scale != 1) view.Scale(new SizeF(scale, scale));
            view.Size = new Size(730, 500);
            view.PerformLayout();
            var viewport = (Panel)view.Controls["ViewViewport"]!;
            var content = (Panel)viewport.Controls["ViewContent"]!;
            Assert(content.Width >= content.MinimumSize.Width && content.Height >= content.MinimumSize.Height, $"{view.GetType().Name}: content shrank below minimum.");
            Assert(viewport.HorizontalScroll.Visible == (content.Width > viewport.ClientSize.Width)
                && viewport.VerticalScroll.Visible == (content.Height > viewport.ClientSize.Height),
                $"{view.GetType().Name}: missing scrollbars for overflowing content.");
            foreach (var control in Descendants(content).Where(c => c is Button or TextBox or ComboBox or DateTimePicker or NumericUpDown or CheckedListBox or DataGridView))
            {
                var parent = control.Parent!;
                Assert(control.Left >= 0 && control.Top >= 0 && control.Right <= parent.ClientSize.Width + 1 && control.Bottom <= parent.ClientSize.Height + 1,
                    $"{view.GetType().Name}: clipped {control.GetType().Name} '{control.Text}' {control.Bounds} in {parent.ClientSize}.");
            }
            view.Size = new Size(1800, 1200);
            view.PerformLayout();
            Assert(content.Size == viewport.ClientSize, $"{view.GetType().Name}: content does not fill a larger window.");
        }
    }
}
