using System.ComponentModel;

namespace QuanLyThuVien.Desktop.Theme;

internal enum ButtonKind
{
    /// <summary>Hành động chính của màn hình (Thêm, Lưu, Đăng nhập).</summary>
    Primary,
    /// <summary>Hành động phụ (Cập nhật, Làm mới, Xem chi tiết).</summary>
    Secondary,
    /// <summary>Hành động ẩn/khóa/xóa.</summary>
    Danger,
    /// <summary>Hành động xác nhận hoàn tất (Thu phạt, Xác nhận trả).</summary>
    Success
}

/// <summary>Nút phẳng, một họ font, 4 kiểu màu cố định.</summary>
internal sealed class AppButton : Button
{
    private ButtonKind _kind;

    public AppButton(string text, ButtonKind kind = ButtonKind.Secondary)
    {
        Text = text;
        FlatStyle = FlatStyle.Flat;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Font = UiTheme.Font(9.5f, FontStyle.Bold);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MinimumSize = new Size(96, 36);
        Padding = new Padding(14, 0, 14, 0);
        Margin = new Padding(0, 0, 8, 8);
        FlatAppearance.BorderSize = 1;
        _kind = kind;
        ApplyStyle();
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ButtonKind Kind
    {
        get => _kind;
        set { _kind = value; ApplyStyle(); }
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        if (!Enabled)
        {
            BackColor = UiTheme.InputDisabled;
            ForeColor = UiTheme.TextMuted;
            FlatAppearance.BorderColor = UiTheme.Border;
            FlatAppearance.MouseOverBackColor = UiTheme.InputDisabled;
            FlatAppearance.MouseDownBackColor = UiTheme.InputDisabled;
            return;
        }

        switch (_kind)
        {
            case ButtonKind.Primary:
                Colorize(UiTheme.Primary, Color.White, UiTheme.Primary, UiTheme.PrimaryHover, UiTheme.PrimaryPressed);
                break;
            case ButtonKind.Success:
                Colorize(UiTheme.Success, Color.White, UiTheme.Success, UiTheme.SuccessHover, UiTheme.SuccessHover);
                break;
            case ButtonKind.Danger:
                Colorize(UiTheme.Surface, UiTheme.Danger, Color.FromArgb(252, 165, 165), UiTheme.DangerSoft, Color.FromArgb(254, 202, 202));
                break;
            default:
                Colorize(UiTheme.Surface, UiTheme.Text, UiTheme.BorderStrong, UiTheme.TableHeaderBg, UiTheme.Border);
                break;
        }
    }

    private void Colorize(Color back, Color fore, Color border, Color hover, Color pressed)
    {
        BackColor = back;
        ForeColor = fore;
        FlatAppearance.BorderColor = border;
        FlatAppearance.MouseOverBackColor = hover;
        FlatAppearance.MouseDownBackColor = pressed;
    }
}

/// <summary>Khối nền trắng viền mảnh chứa một nhóm nội dung.</summary>
internal sealed class CardPanel : Panel
{
    private readonly TableLayoutPanel _rows;

    /// <param name="title">Tiêu đề khối (tùy chọn).</param>
    /// <param name="autoHeight">true: cao theo nội dung. false: lấp đầy ô chứa, dùng khi có bảng dữ liệu.</param>
    public CardPanel(string? title = null, bool autoHeight = true)
    {
        BackColor = UiTheme.Surface;
        Padding = new Padding(20, 16, 20, 12);
        Margin = Padding.Empty;
        DoubleBuffered = true;
        ResizeRedraw = true;

        _rows = new TableLayoutPanel
        {
            ColumnCount = 1, BackColor = UiTheme.Surface, Margin = Padding.Empty, Padding = Padding.Empty,
            Dock = autoHeight ? DockStyle.Top : DockStyle.Fill
        };
        _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        if (autoHeight)
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _rows.AutoSize = true;
            _rows.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }
        Controls.Add(_rows);
        if (!string.IsNullOrEmpty(title)) AddRow(Ui.Heading(title));
    }

    public void AddRow(Control control, bool fill = false)
    {
        control.Dock = fill ? DockStyle.Fill : DockStyle.Top;
        _rows.RowStyles.Add(fill ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
        _rows.RowCount = _rows.RowStyles.Count;
        _rows.Controls.Add(control, 0, _rows.RowCount - 1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(UiTheme.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}

/// <summary>
/// Lưới nhập liệu: nhãn nằm trên ô nhập, chia đều theo số cột, mọi dòng tự co theo nội dung
/// nên không bị cắt chữ khi đổi font hoặc DPI.
/// </summary>
internal sealed class FormGrid : TableLayoutPanel
{
    private readonly int _columns;
    private int _cursor;
    private int _lineBase = -2;

    public FormGrid(int columns = 2)
    {
        _columns = columns;
        ColumnCount = columns;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Dock = DockStyle.Top;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        BackColor = UiTheme.Surface;
        for (var i = 0; i < columns; i++)
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
    }

    /// <summary>Thêm một ô nhập kèm nhãn. <paramref name="span"/> là số cột ô nhập chiếm.</summary>
    public T Add<T>(string label, T input, int span = 1) where T : Control
    {
        span = Math.Clamp(span, 1, _columns);
        if (_lineBase < 0 || _cursor + span > _columns) NewLine();

        var caption = Ui.FieldLabel(label);
        caption.Margin = new Padding(0, 0, 12, 4);
        input.Margin = new Padding(0, 0, 16, 14);
        input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        Controls.Add(caption, _cursor, _lineBase);
        SetColumnSpan(caption, span);
        Controls.Add(input, _cursor, _lineBase + 1);
        SetColumnSpan(input, span);
        _cursor += span;
        return input;
    }

    /// <summary>Thêm một control chiếm trọn một dòng (hàng nút, ghi chú).</summary>
    public void AddFull(Control control)
    {
        NewLine();
        var row = _lineBase;
        RowStyles[row + 1] = new RowStyle(SizeType.Absolute, 0);
        RowStyles[row] = new RowStyle(SizeType.AutoSize);
        control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        Controls.Add(control, 0, row);
        SetColumnSpan(control, _columns);
        _cursor = _columns;
    }

    private void NewLine()
    {
        _lineBase = RowStyles.Count;
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RowCount = RowStyles.Count;
        _cursor = 0;
    }
}

/// <summary>Thanh lọc/tìm kiếm: các nhóm "nhãn + ô nhập" tự xuống dòng khi hẹp.</summary>
internal sealed class ToolbarRow : FlowLayoutPanel
{
    public ToolbarRow()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        WrapContents = false;
        FlowDirection = FlowDirection.LeftToRight;
        Dock = DockStyle.Top;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        BackColor = UiTheme.Surface;
    }

    public T AddField<T>(string caption, T control) where T : Control
    {
        var group = NewGroup();
        var label = Ui.FieldLabel(caption);
        label.Margin = new Padding(0, 0, 0, 4);
        control.Margin = Padding.Empty;
        group.Controls.Add(label, 0, 0);
        group.Controls.Add(control, 0, 1);
        Controls.Add(group);
        return control;
    }

    /// <summary>Thêm các nút, căn thẳng hàng với các ô nhập bên cạnh.</summary>
    public void AddActions(params Control[] buttons)
    {
        var group = NewGroup();
        var spacer = Ui.FieldLabel(" ");
        spacer.Margin = new Padding(0, 0, 0, 4);
        var bar = Ui.ButtonBar(buttons);
        bar.Margin = Padding.Empty;
        foreach (var button in buttons) button.Margin = new Padding(0, 0, 8, 0);
        group.Controls.Add(spacer, 0, 0);
        group.Controls.Add(bar, 0, 1);
        Controls.Add(group);
    }

    private static TableLayoutPanel NewGroup()
    {
        var group = new TableLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 2,
            Margin = new Padding(0, 0, 16, 12), Padding = Padding.Empty, BackColor = UiTheme.Surface
        };
        group.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        group.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        return group;
    }
}

/// <summary>Mục điều hướng ở thanh bên: tự vẽ, có vạch nhấn khi đang chọn.</summary>
internal sealed class NavButton : Button
{
    private bool _active;
    private bool _hover;

    public NavButton(string text)
    {
        Text = text;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        Height = 44;
        MinimumSize = new Size(0, 44);
        Margin = Padding.Empty;
        Font = UiTheme.Font(10);
        TextColor = UiTheme.SidebarText;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TextColor { get; set; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Active
    {
        get => _active;
        set { _active = value; Invalidate(); }
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var back = _active ? UiTheme.SidebarActive : _hover ? UiTheme.SidebarHover : UiTheme.Sidebar;
        using (var brush = new SolidBrush(back)) e.Graphics.FillRectangle(brush, ClientRectangle);
        if (_active)
        {
            using var accent = new SolidBrush(Color.FromArgb(96, 165, 250));
            e.Graphics.FillRectangle(accent, 0, 0, Ui.Px(4), Height);
        }

        var font = _active ? UiTheme.Font(10, FontStyle.Bold) : UiTheme.Font(10);
        var color = _active ? Color.White : _hover ? Color.White : TextColor;
        var bounds = new Rectangle(Ui.Px(24), 0, Math.Max(10, Width - Ui.Px(36)), Height);
        TextRenderer.DrawText(e.Graphics, Text, font, bounds, color,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        if (Focused && ShowFocusCues)
        {
            var focus = ClientRectangle;
            focus.Inflate(-2, -2);
            ControlPaint.DrawFocusRectangle(e.Graphics, focus, Color.White, back);
        }
    }
}

/// <summary>Thanh tab phẳng, vạch chân màu nhấn; thay cho TabControl mặc định của WinForms.</summary>
internal sealed class PageTabs : Panel
{
    private sealed class Page
    {
        public required string Title { get; init; }
        public required Control Content { get; init; }
        public Rectangle Bounds { get; set; }
    }

    private sealed class HeaderPanel : Panel
    {
        public HeaderPanel() { DoubleBuffered = true; ResizeRedraw = true; }
    }

    private readonly HeaderPanel _header;
    private readonly Panel _body;
    private readonly List<Page> _pages = [];
    private int _selected = -1;
    private int _hot = -1;

    public event EventHandler? SelectedIndexChanged;

    public PageTabs(Color? background = null)
    {
        var back = background ?? UiTheme.PageBg;
        BackColor = back;
        Dock = DockStyle.Fill;
        _header = new HeaderPanel { Dock = DockStyle.Top, Height = 44, BackColor = back };
        _body = new Panel { Dock = DockStyle.Fill, BackColor = back, Padding = new Padding(0, 14, 0, 0) };
        Controls.Add(_body);
        Controls.Add(_header);
        _header.Paint += (_, e) => DrawHeader(e.Graphics);
        _header.MouseMove += (_, e) => SetHot(HitTest(e.Location));
        _header.MouseLeave += (_, _) => SetHot(-1);
        _header.MouseClick += (_, e) =>
        {
            var index = HitTest(e.Location);
            if (index >= 0) SelectedIndex = index;
        };
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => _selected;
        set
        {
            if (value < 0 || value >= _pages.Count || value == _selected) return;
            if (_selected >= 0) _pages[_selected].Content.Visible = false;
            _selected = value;
            _pages[_selected].Content.Visible = true;
            _header.Invalidate();
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Control? SelectedPage => _selected >= 0 ? _pages[_selected].Content : null;

    public void AddPage(string title, Control content)
    {
        content.Dock = DockStyle.Fill;
        content.Visible = false;
        _body.Controls.Add(content);
        _pages.Add(new Page { Title = title, Content = content });
        if (_selected < 0) SelectedIndex = 0;
        _header.Invalidate();
    }

    private void LayoutTabs()
    {
        var x = 0;
        var font = UiTheme.Font(10, FontStyle.Bold);
        foreach (var page in _pages)
        {
            var width = TextRenderer.MeasureText(page.Title, font).Width + Ui.Px(40);
            page.Bounds = new Rectangle(x, 0, width, _header.Height - 1);
            x += width + Ui.Px(4);
        }
    }

    private int HitTest(Point point)
    {
        LayoutTabs();
        for (var i = 0; i < _pages.Count; i++)
            if (_pages[i].Bounds.Contains(point)) return i;
        return -1;
    }

    private void SetHot(int index)
    {
        if (_hot == index) return;
        _hot = index;
        _header.Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
        _header.Invalidate();
    }

    private void DrawHeader(Graphics graphics)
    {
        LayoutTabs();
        using (var line = new Pen(UiTheme.BorderStrong))
            graphics.DrawLine(line, 0, _header.Height - 1, _header.Width, _header.Height - 1);

        var font = UiTheme.Font(10, FontStyle.Bold);
        for (var i = 0; i < _pages.Count; i++)
        {
            var page = _pages[i];
            var selected = i == _selected;
            var color = selected ? UiTheme.Primary : i == _hot ? UiTheme.Text : UiTheme.TextMuted;
            TextRenderer.DrawText(graphics, page.Title, font, page.Bounds, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (selected)
            {
                using var brush = new SolidBrush(UiTheme.Primary);
                var thickness = Ui.Px(3);
                graphics.FillRectangle(brush, page.Bounds.Left, _header.Height - thickness, page.Bounds.Width, thickness);
            }
        }
    }
}
