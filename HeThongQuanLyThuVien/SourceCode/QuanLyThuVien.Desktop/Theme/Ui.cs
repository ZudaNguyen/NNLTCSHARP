using System.Runtime.CompilerServices;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.Theme;

/// <summary>
/// Các hàm tạo control dùng chung và bộ scale DPI.
/// Quy ước: kích thước pixel trong code được viết ở mức 96 DPI, sau khi dựng xong gọi <see cref="ApplyDpi"/> một lần.
/// </summary>
internal static class Ui
{
    private static float? _scale;
    private static readonly ConditionalWeakTable<Control, object> ScaledControls = new();
    private static readonly object ScaledMarker = new();

    /// <summary>Hệ số DPI của hệ thống (1.0 = 96 DPI, 1.25 = 120 DPI...).</summary>
    public static float Scale
    {
        get
        {
            if (_scale is { } cached) return cached;
            using var graphics = Graphics.FromHwnd(IntPtr.Zero);
            _scale = Math.Max(1f, graphics.DpiX / 96f);
            return _scale.Value;
        }
    }

    /// <summary>Đổi pixel 96 DPI sang pixel thực. Chỉ dùng trong code tự vẽ (OnPaint).</summary>
    public static int Px(int value) => (int)Math.Round(value * Scale);

    // ------------------------------------------------------------------
    // Nhãn
    // ------------------------------------------------------------------
    public static Label FieldLabel(string text) => new()
    {
        Text = text, AutoSize = true, Font = UiTheme.Font(9.5f), ForeColor = UiTheme.TextSecondary,
        Margin = new Padding(0, 0, 12, 4)
    };

    public static Label Heading(string text) => new()
    {
        Text = text, AutoSize = true, Font = UiTheme.Heading, ForeColor = UiTheme.Text,
        Margin = new Padding(0, 0, 0, 12)
    };

    /// <summary>Đoạn ghi chú nhỏ, tự xuống dòng khi quá dài.</summary>
    public static Label Hint(string text, int maxWidth = 1000) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(maxWidth, 0),
        Font = UiTheme.Font(9.5f), ForeColor = UiTheme.TextMuted, Margin = new Padding(0, 0, 0, 8)
    };

    // ------------------------------------------------------------------
    // Ô nhập
    // ------------------------------------------------------------------
    public static TextBox TextBox(string? placeholder = null, int? width = null)
    {
        var box = new TextBox
        {
            Font = UiTheme.Body, ForeColor = UiTheme.Text, BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle, PlaceholderText = placeholder ?? string.Empty
        };
        if (width.HasValue) box.Width = width.Value;
        return box;
    }

    /// <summary>Ô chỉ đọc, nền xám nhạt (ví dụ mã tự sinh).</summary>
    public static TextBox ReadOnlyBox(string placeholder = "Tự động")
    {
        var box = TextBox(placeholder);
        box.ReadOnly = true;
        box.BackColor = UiTheme.InputDisabled;
        box.TabStop = false;
        return box;
    }

    public static TextBox MultilineBox(int height, string? placeholder = null)
    {
        var box = TextBox(placeholder);
        box.Multiline = true;
        box.ScrollBars = ScrollBars.Vertical;
        box.Height = height;
        return box;
    }

    public static ComboBox ComboBox(IEnumerable<object>? items = null, int selectedIndex = 0,
        ComboBoxStyle style = ComboBoxStyle.DropDownList, int? width = null)
    {
        var combo = new ComboBox
        {
            Font = UiTheme.Body, DropDownStyle = style, ForeColor = UiTheme.Text,
            FlatStyle = FlatStyle.Standard, IntegralHeight = false
        };
        if (items != null) combo.Items.AddRange(items.ToArray());
        if (selectedIndex >= 0 && combo.Items.Count > selectedIndex) combo.SelectedIndex = selectedIndex;
        if (width.HasValue) combo.Width = width.Value;
        return combo;
    }

    public static DateTimePicker DatePicker(DateTime value, int? width = null)
    {
        var picker = new DateTimePicker { Font = UiTheme.Body, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = value };
        if (width.HasValue) picker.Width = width.Value;
        return picker;
    }

    public static NumericUpDown Number(decimal minimum, decimal maximum, decimal value, int? width = null)
    {
        var number = new NumericUpDown { Font = UiTheme.Body, Minimum = minimum, Maximum = maximum, Value = value };
        if (width.HasValue) number.Width = width.Value;
        return number;
    }

    // ------------------------------------------------------------------
    // Nút, thẻ, bảng
    // ------------------------------------------------------------------
    public static AppButton Button(string text, ButtonKind kind = ButtonKind.Secondary) => new(text, kind);

    /// <summary>Hàng nút hành động, tự xuống dòng khi hẹp.</summary>
    public static FlowLayoutPanel ButtonBar(params Control[] buttons)
    {
        var bar = new FlowLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0, 4, 0, 0),
            Padding = Padding.Empty, BackColor = UiTheme.Surface
        };
        bar.Controls.AddRange(buttons);
        return bar;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyLibraryStyle(grid);
        return grid;
    }

    /// <summary>
    /// Xếp các khối theo chiều dọc: mọi khối tự co theo nội dung, riêng khối <paramref name="fillIndex"/> chiếm phần còn lại.
    /// </summary>
    public static TableLayoutPanel Stack(int fillIndex, params Control[] rows)
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = rows.Length,
            BackColor = UiTheme.PageBg, Margin = Padding.Empty, Padding = Padding.Empty
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < rows.Length; i++)
        {
            var isFill = i == fillIndex;
            table.RowStyles.Add(isFill ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
            rows[i].Dock = DockStyle.Fill;
            rows[i].Margin = isFill || i == rows.Length - 1 ? Padding.Empty : new Padding(0, 0, 0, 16);
            table.Controls.Add(rows[i], 0, i);
        }
        return table;
    }

    /// <summary>Khối cuối cùng chiếm phần còn lại.</summary>
    public static TableLayoutPanel Stack(params Control[] rows) => Stack(rows.Length - 1, rows);

    /// <summary>
    /// Như <see cref="Stack(Control[])"/> nhưng khi cửa sổ thấp, cả trang cuộn dọc thay vì bóp khối cuối
    /// (thường là bảng dữ liệu) xuống dưới <paramref name="minFillHeight"/> pixel (96 DPI).
    /// </summary>
    public static Panel ScrollStack(int minFillHeight, params Control[] rows)
    {
        var stack = Stack(rows);
        var host = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.PageBg, Margin = Padding.Empty };
        host.Controls.Add(stack);
        void UpdateMinimumHeight()
        {
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                total += i == rows.Length - 1 ? Px(minFillHeight) : rows[i].PreferredSize.Height;
                total += rows[i].Margin.Vertical;
            }
            var size = new Size(0, total);
            if (host.AutoScrollMinSize != size) host.AutoScrollMinSize = size;
        }
        host.SizeChanged += (_, _) => UpdateMinimumHeight();
        host.HandleCreated += (_, _) => UpdateMinimumHeight();
        return host;
    }

    /// <summary>Gắn nền, đệm chuẩn cho một màn hình (UserControl), thêm nội dung rồi scale DPI.</summary>
    public static void SetupPage(UserControl page, Control content)
    {
        page.AutoScaleMode = AutoScaleMode.None;
        page.Dock = DockStyle.Fill;
        page.BackColor = UiTheme.PageBg;
        page.Padding = new Padding(24, 20, 24, 20);
        page.Controls.Add(content);
        ApplyDpi(page);
    }

    /// <summary>Cấu hình chung cho hộp thoại: nền, font, đệm, kích thước (viết ở 96 DPI). Gọi <see cref="ApplyDpi"/> sau khi thêm nội dung.</summary>
    public static void SetupDialog(Form form, string title, Size clientSize, Size? minimumSize = null, bool fixedSize = false)
    {
        form.AutoScaleMode = AutoScaleMode.None;
        form.Text = title;
        form.StartPosition = FormStartPosition.CenterParent;
        form.BackColor = UiTheme.PageBg;
        form.Font = UiTheme.Body;
        form.ClientSize = clientSize;
        form.Padding = new Padding(20);
        form.ShowInTaskbar = false;
        if (minimumSize.HasValue) form.MinimumSize = minimumSize.Value;
        if (fixedSize)
        {
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
        }
    }

    // ------------------------------------------------------------------
    // Scale DPI
    // ------------------------------------------------------------------
    /// <summary>
    /// Nhân các kích thước pixel (kích thước, lề, đệm, độ cao dòng bảng...) với hệ số DPI hệ thống.
    /// Font không bị nhân vì đã tính bằng point. Mỗi control chỉ được scale một lần.
    /// </summary>
    public static void ApplyDpi(Control root)
    {
        var scaled = new List<Control>();
        CollectUnscaled(root, scaled);
        // Nhãn và nút hiển thị nguyên văn: "&" là chữ "&", không phải ký tự phím tắt.
        foreach (var control in scaled)
        {
            if (control is Label label) label.UseMnemonic = false;
            else if (control is ButtonBase button) button.UseMnemonic = false;
        }
        if (Scale <= 1.01f) return;
        foreach (var control in scaled) control.SuspendLayout();
        foreach (var control in scaled) ScaleOne(control, isRoot: control == root);
        // Layout lại từ control con ra ngoài để các khối tự co (AutoSize) tính lại theo kích thước đã scale.
        for (var i = scaled.Count - 1; i >= 0; i--) scaled[i].ResumeLayout(performLayout: true);
        root.PerformLayout();
    }

    private static void CollectUnscaled(Control control, List<Control> scaled)
    {
        if (ScaledControls.TryGetValue(control, out _)) return;
        ScaledControls.Add(control, ScaledMarker);
        scaled.Add(control);
        foreach (Control child in control.Controls) CollectUnscaled(child, scaled);
    }

    private static int S(int value) => value == 0 || value == int.MaxValue ? value : (int)Math.Round(value * Scale);
    private static Size S(Size size) => new(S(size.Width), S(size.Height));
    private static Padding S(Padding padding) => new(S(padding.Left), S(padding.Top), S(padding.Right), S(padding.Bottom));

    private static bool IsInputBox(Control control) => control is TextBoxBase or System.Windows.Forms.ComboBox or DateTimePicker or UpDownBase;

    private static void ScaleOne(Control control, bool isRoot)
    {
        // Đọc kích thước vùng client của form trước khi đổi MinimumSize, vì MinimumSize lớn hơn
        // sẽ khiến WinForms tự nới form ra và ta sẽ nhân hệ số DPI lên hai lần.
        var formClientSize = control is Form currentForm ? currentForm.ClientSize : Size.Empty;

        control.MinimumSize = S(control.MinimumSize);
        control.MaximumSize = S(control.MaximumSize);
        control.Margin = S(control.Margin);
        control.Padding = S(control.Padding);

        if (control is Form form)
        {
            form.ClientSize = S(formClientSize);
        }
        else
        {
            var inCellLayout = control.Parent is TableLayoutPanel or FlowLayoutPanel;
            var automatic = control.AutoSize && !IsInputBox(control);
            if (!automatic)
            {
                switch (control.Dock)
                {
                    case DockStyle.None:
                        if (IsInputBox(control) && control is not System.Windows.Forms.TextBox { Multiline: true })
                            control.Width = S(control.Width);
                        else
                            control.Size = S(control.Size);
                        break;
                    case DockStyle.Top or DockStyle.Bottom:
                        control.Height = S(control.Height);
                        break;
                    case DockStyle.Left or DockStyle.Right:
                        control.Width = S(control.Width);
                        break;
                }
            }
            if (control.Dock == DockStyle.None && !inCellLayout && !isRoot)
                control.Location = new Point(S(control.Left), S(control.Top));
        }

        switch (control)
        {
            case TableLayoutPanel table:
                foreach (ColumnStyle style in table.ColumnStyles)
                    if (style.SizeType == SizeType.Absolute) style.Width = S((int)style.Width);
                foreach (RowStyle style in table.RowStyles)
                    if (style.SizeType == SizeType.Absolute) style.Height = S((int)style.Height);
                break;
            case DataGridView grid:
                if (grid.ColumnHeadersHeightSizeMode != DataGridViewColumnHeadersHeightSizeMode.AutoSize)
                    grid.ColumnHeadersHeight = S(grid.ColumnHeadersHeight);
                grid.RowTemplate.Height = S(grid.RowTemplate.Height);
                foreach (DataGridViewRow row in grid.Rows) row.Height = S(row.Height);
                break;
            case ListBox { DrawMode: not DrawMode.Normal } list:
                list.ItemHeight = S(list.ItemHeight);
                break;
        }

        if (control is ScrollableControl { AutoScroll: true } scroll)
            scroll.AutoScrollMinSize = S(scroll.AutoScrollMinSize);
    }
}
