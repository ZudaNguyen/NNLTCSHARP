using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class MuonTraControl : UserControl
    {
        // Bảng màu Thư viện cổ điển
        private readonly Color PaperCream = Color.FromArgb(247, 243, 233);
        private readonly Color CardBg = Color.FromArgb(255, 252, 245);
        private readonly Color WoodDark = Color.FromArgb(44, 30, 22);
        private readonly Color Burgundy = Color.FromArgb(122, 40, 34);
        private readonly Color ForestGreen = Color.FromArgb(45, 106, 79);

        // Controls Lập Phiếu Mượn Mới (Bảng PhieuMuon)
        private ComboBox cboDocGia;
        private DateTimePicker dtpNgayMuon;
        private DateTimePicker dtpHanTra;
        private CheckedListBox clbSachMuon;
        private TextBox txtTimPhieu;
        private ComboBox cboLocTrangThai;

        // Danh sách phiếu
        private DataGridView dgvPhieuMuon;

        private readonly MuonTraApiClient _api = new();
        private readonly string _nguoiLap;
        private readonly List<SachMuonOptionDto> _sachConHang = [];
        private decimal? _mucPhatMoiNgay;
        private TabControl? _tabs;

        public MuonTraControl(string nguoiLap = "")
        {
            _nguoiLap = nguoiLap;
            InitializeComponent();
            SetupMuonTraUI();
            Load += async (_, _) => await LoadInitialDataAsync();
        }

        private void SetupMuonTraUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = PaperCream;
            this.Padding = new Padding(18);
            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Georgia", 10, FontStyle.Bold), Padding = new Point(12, 5) };
            var tabCreate = new TabPage("  Lập phiếu mượn  ") { BackColor = PaperCream, Padding = new Padding(8) };
            var tabLoans = new TabPage("  Danh sách phiếu  ") { BackColor = PaperCream, Padding = new Padding(8) };
            var tabDetail = new TabPage("  Lịch sử mượn  ") { BackColor = PaperCream, Padding = new Padding(8) };
            _tabs = tabs;
            tabs.TabPages.AddRange([tabCreate, tabLoans, tabDetail]);
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.ItemSize = new Size(220, 38);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.DrawItem += (_, e) =>
            {
                var selected = e.Index == tabs.SelectedIndex;
                using var background = new SolidBrush(selected ? Burgundy : Color.FromArgb(224, 213, 191));
                e.Graphics.FillRectangle(background, e.Bounds);
                TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text.Trim(), tabs.Font, e.Bounds,
                    selected ? Color.White : WoodDark, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            tabs.SelectedIndexChanged += async (_, _) =>
            {
                if (tabs.SelectedTab == tabDetail) await LoadHistoryAsync();
            };

            // =========================================================
            // 1. KHU VỰC LẬP PHIẾU MƯỢN MỚI (TOP PANEL)
            // =========================================================
            Panel pnlCreateLoan = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 185,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                AutoScrollMinSize = new Size(0, 175)
            };

            Label lblTitleCreate = new Label
            {
                Text = "✒️ Lập Phiếu Mượn Sách Mới (Ghi Sổ Mượn)",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                ForeColor = Burgundy,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlCreateLoan.Controls.Add(lblTitleCreate);

            // Cột trái: Độc giả, Ngày mượn, Hạn trả
            AddLabel(pnlCreateLoan, "Thẻ độc giả:", 15, 42);
            cboDocGia = new ComboBox
            {
                Location = new Point(130, 39),
                Width = 270,
                Font = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlCreateLoan.Controls.Add(cboDocGia);

            AddLabel(pnlCreateLoan, "Ngày mượn:", 15, 80);
            dtpNgayMuon = new DateTimePicker
            {
                Location = new Point(130, 77),
                Width = 125,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlCreateLoan.Controls.Add(dtpNgayMuon);

            AddLabel(pnlCreateLoan, "Hạn trả:", 15, 118);
            dtpHanTra = new DateTimePicker
            {
                Location = new Point(130, 115),
                Width = 125,
                Font = new Font("Segoe UI", 9.5f),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(14) // Mặc định cho mượn 14 ngày
            };
            pnlCreateLoan.Controls.Add(dtpHanTra);

            Button btnLapPhieu = new Button
            {
                Text = "📜 Lập Phiếu Mượn",
                Location = new Point(265, 77),
                Size = new Size(135, 65),
                Font = new Font("Georgia", 9.5f, FontStyle.Bold),
                BackColor = ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLapPhieu.FlatAppearance.BorderSize = 0;
            btnLapPhieu.Click += ApiBtnLapPhieu_Click;
            pnlCreateLoan.Controls.Add(btnLapPhieu);

            // Cột phải: Chọn danh sách sách muốn mượn
            var lblBookSelection = new Label
            {
                Text = "Chọn sách còn tồn trong kho:", Location = new Point(425, 13), Size = new Size(520, 22),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = WoodDark, AutoEllipsis = true
            };
            pnlCreateLoan.Controls.Add(lblBookSelection);
            clbSachMuon = new CheckedListBox
            {
                Location = new Point(425, 39),
                Size = new Size(520, 130),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(255, 253, 248),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 25
            };
            clbSachMuon.DrawItem += (_, e) =>
            {
                if (e.Index < 0) return;
                var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var background = selected ? Color.FromArgb(238, 230, 212) : Color.FromArgb(255, 253, 248);
                using (var brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.Bounds);
                var checkState = clbSachMuon.GetItemChecked(e.Index)
                    ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal
                    : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 4, e.Bounds.Top + 4), checkState);
                var text = clbSachMuon.GetItemText(clbSachMuon.Items[e.Index]);
                TextRenderer.DrawText(e.Graphics, text, clbSachMuon.Font,
                    new Rectangle(e.Bounds.Left + 26, e.Bounds.Top + 1, Math.Max(10, e.Bounds.Width - 30), e.Bounds.Height - 2),
                    Color.FromArgb(44, 30, 22), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                    ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, Color.FromArgb(44, 30, 22), background);
            };
            clbSachMuon.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlCreateLoan.SizeChanged += (_, _) =>
            {
                if (pnlCreateLoan.ClientSize.Width > 500)
                {
                    clbSachMuon.Width = Math.Max(250, pnlCreateLoan.ClientSize.Width - clbSachMuon.Left - 18);
                    lblBookSelection.Width = Math.Max(250, pnlCreateLoan.ClientSize.Width - lblBookSelection.Left - 18);
                }
                if (pnlCreateLoan.ClientSize.Height > 80)
                    clbSachMuon.Height = Math.Max(55, pnlCreateLoan.ClientSize.Height - clbSachMuon.Top - 12);
            };
            pnlCreateLoan.Controls.Add(clbSachMuon);

            // =========================================================
            // 2. KHU VỰC DANH SÁCH PHIẾU MƯỢN - MASTER (MIDDLE PANEL)
            // =========================================================
            Panel pnlMaster = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 220,
                BackColor = PaperCream,
                Padding = new Padding(0, 10, 0, 10)
            };

            var pnlMasterBox = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = CardBg, BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10), Margin = Padding.Empty
            };
            pnlMasterBox.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlMasterBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            pnlMasterBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            pnlMasterBox.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Thanh tìm kiếm phiếu mượn
            var pnlMasterTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight, AutoScroll = false,
                Padding = new Padding(0, 2, 0, 2), Margin = Padding.Empty
            };
            Label lblMasterTitle = new Label
            {
                Text = "📚 Sổ phiếu mượn",
                Font = new Font("Georgia", 10f, FontStyle.Bold),
                ForeColor = WoodDark,
                Size = new Size(155, 34), TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 4, 2)
            };

            Label lblTim = new Label { Text = "Tìm phiếu/độc giả:", Size = new Size(115, 30), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(2) };
            txtTimPhieu = new TextBox { Width = 150, Font = new Font("Segoe UI", 9.5f), Margin = new Padding(2, 4, 6, 2) };
            txtTimPhieu.TextChanged += (s, e) => ApiFilterMasterGrid();

            Label lblLoc = new Label { Text = "Trạng thái:", Size = new Size(75, 30), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(2) };
            cboLocTrangThai = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f), Margin = new Padding(2, 4, 4, 2) };
            cboLocTrangThai.Items.AddRange(new object[] { "-- Tất cả --", "Đang mượn", "Quá hạn", "Đã hoàn tất" });
            cboLocTrangThai.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndexChanged += (s, e) => ApiFilterMasterGrid();

            var btnRefreshLoans = new Button { Text = "Làm mới", Size = new Size(88, 30), BackColor = Color.FromArgb(90, 75, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(3, 2, 3, 2), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            btnRefreshLoans.Click += async (_, _) => await LoadLoansAsync();
            var btnViewDetail = new Button { Text = "Chi tiết / Nhận trả", Size = new Size(155, 30), BackColor = Burgundy, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(3, 2, 3, 2), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            btnViewDetail.Click += async (_, _) =>
            {
                if (dgvPhieuMuon.CurrentRow?.Tag is int loanId) await OpenNhanTraDialogAsync(loanId);
                else MessageBox.Show("Chọn phiếu mượn cần xem.");
            };
            pnlMasterTop.Controls.AddRange([lblTim, txtTimPhieu, lblLoc, cboLocTrangThai, btnRefreshLoans, btnViewDetail]);

            dgvPhieuMuon = CreateStyledGrid();
            dgvPhieuMuon.Columns.Add("MaPhieu", "Mã Phiếu");
            dgvPhieuMuon.Columns["MaPhieu"].FillWeight = 55;
            dgvPhieuMuon.Columns.Add("DocGia", "Họ Tên Độc Giả");
            dgvPhieuMuon.Columns["DocGia"].FillWeight = 130;
            dgvPhieuMuon.Columns.Add("NhanVien", "Thủ Thư Lập");
            dgvPhieuMuon.Columns.Add("NgayMuon", "Ngày Mượn");
            dgvPhieuMuon.Columns.Add("HanTra", "Hạn Trả");
            dgvPhieuMuon.Columns.Add("TrangThai", "Trạng Thái Phiếu");
            dgvPhieuMuon.CellDoubleClick += async (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvPhieuMuon.Rows[e.RowIndex].Tag is int loanId)
                    await OpenNhanTraDialogAsync(loanId);
            };

            pnlMasterBox.Controls.Add(lblMasterTitle, 0, 0);
            pnlMasterBox.Controls.Add(pnlMasterTop, 0, 1);
            pnlMasterBox.Controls.Add(dgvPhieuMuon, 0, 2);
            pnlMaster.Controls.Add(pnlMasterBox);

            DataGridViewStyleHelper.ApplyLibraryStyle(dgvPhieuMuon);
            pnlCreateLoan.Dock = DockStyle.Fill;
            pnlMaster.Dock = DockStyle.Fill;
            tabCreate.Controls.Add(pnlCreateLoan);
            tabLoans.Controls.Add(pnlMaster);
            tabDetail.Controls.Add(CreateHistoryView());
            this.Controls.Add(tabs);
        }

        // =========================================================
        // HÀM HỖ TRỢ & XỬ LÝ NGHIỆP VỤ FRONTEND
        // =========================================================
        private DataGridView CreateStyledGrid()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 30 },
                GridColor = Color.FromArgb(225, 215, 195)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(74, 53, 37);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(245, 235, 215);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 9, FontStyle.Bold);

            DataGridViewStyleHelper.ApplyLibraryStyle(dgv);
            return dgv;
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = WoodDark,
                AutoSize = true
            });
        }

    }
}
