using System.Drawing;
using System.Windows.Forms;

namespace QuanLyThuVien.Desktop.UI
{
    internal static class ScrollableView
    {
        // Giữ kích thước đủ dùng cho biểu mẫu; cửa sổ nhỏ có thanh cuộn,
        // cửa sổ lớn tiếp tục mở rộng các bảng DockStyle.Fill.
        public static void Configure(UserControl view, Size minimumContentSize)
        {
            view.SuspendLayout();
            var children = view.Controls.Cast<Control>().ToArray();
            var content = new Panel
            {
                Name = "ViewContent",
                BackColor = view.BackColor,
                Padding = view.Padding,
                MinimumSize = minimumContentSize
            };
            var viewport = new Panel
            {
                Name = "ViewViewport",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = view.BackColor
            };

            view.Padding = Padding.Empty;
            foreach (var child in children) content.Controls.Add(child);
            viewport.Controls.Add(content);
            view.Controls.Add(viewport);

            bool resizing = false;
            void ResizeContent()
            {
                if (resizing) return;
                resizing = true;
                try
                {
                    // MinimumSize được WinForms scale cùng các control khi đổi DPI/font.
                    var minimum = content.MinimumSize;
                    viewport.AutoScrollMinSize = minimum;
                    content.Size = new Size(
                        Math.Max(viewport.ClientSize.Width, minimum.Width),
                        Math.Max(viewport.ClientSize.Height, minimum.Height));
                }
                finally { resizing = false; }
            }

            viewport.SizeChanged += (s, e) => ResizeContent();
            viewport.Layout += (s, e) => ResizeContent();
            view.DpiChangedAfterParent += (s, e) => ResizeContent();
            view.ResumeLayout(true);
            ResizeContent();
        }
    }
}
