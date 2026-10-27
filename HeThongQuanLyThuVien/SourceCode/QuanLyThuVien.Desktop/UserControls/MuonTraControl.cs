using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Desktop.Services;
using QuanLyThuVien.Desktop.Theme;

namespace QuanLyThuVien.Desktop.UserControls
{
    public partial class MuonTraControl : UserControl
    {
        // Controls lập phiếu mượn mới (bảng PhieuMuon)
        private ComboBox cboDocGia = null!;
        private DateTimePicker dtpNgayMuon = null!;
        private DateTimePicker dtpHanTra = null!;
        private CheckedListBox clbSachMuon = null!;
        private TextBox txtTimPhieu = null!;
        private ComboBox cboLocTrangThai = null!;

        // Danh sách phiếu
        private DataGridView dgvPhieuMuon = null!;

        private readonly MuonTraApiClient _api = new();
        private readonly string _nguoiLap;
        private readonly List<SachMuonOptionDto> _sachConHang = [];
        private decimal? _mucPhatMoiNgay;
        private PageTabs? _tabs;

        public MuonTraControl(string nguoiLap = "")
        {
            _nguoiLap = nguoiLap;
            InitializeComponent();
            SetupMuonTraUI();
            Load += async (_, _) => await LoadInitialDataAsync();
        }

        private void SetupMuonTraUI()
        {
            var tabs = new PageTabs();
            _tabs = tabs;
            var historyPage = CreateHistoryView();
            tabs.AddPage("Lập phiếu mượn", BuildCreateLoanPage());
            tabs.AddPage("Danh sách phiếu", BuildLoanListPage());
            tabs.AddPage("Lịch sử mượn", historyPage);
            tabs.SelectedIndexChanged += async (_, _) =>
            {
                if (tabs.SelectedPage == historyPage) await LoadHistoryAsync();
            };

            Ui.SetupPage(this, tabs);
        }

        private Control BuildCreateLoanPage()
        {
            cboDocGia = Ui.ComboBox();
            dtpNgayMuon = Ui.DatePicker(DateTime.Now);
            dtpHanTra = Ui.DatePicker(DateTime.Now.AddDays(14)); // Mặc định cho mượn 14 ngày

            var btnLapPhieu = Ui.Button("Lập phiếu mượn", ButtonKind.Primary);
            btnLapPhieu.Click += ApiBtnLapPhieu_Click;

            var left = new FormGrid(1) { Margin = new Padding(0, 0, 24, 0) };
            left.Add("Độc giả", cboDocGia);
            left.Add("Ngày mượn", dtpNgayMuon);
            left.Add("Hạn trả", dtpHanTra);
            left.AddFull(btnLapPhieu);

            clbSachMuon = new CheckedListBox
            {
                Dock = DockStyle.Fill, Font = UiTheme.Font(9.5f), BackColor = UiTheme.Surface, ForeColor = UiTheme.Text,
                BorderStyle = BorderStyle.FixedSingle, CheckOnClick = true, IntegralHeight = false
            };
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = UiTheme.Surface,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.Controls.Add(Ui.FieldLabel("Chọn sách còn trong kho"), 0, 0);
            right.Controls.Add(clbSachMuon, 0, 1);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = UiTheme.Surface,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.Dock = DockStyle.Top;
            body.Controls.Add(left, 0, 0);
            body.Controls.Add(right, 1, 0);

            var card = new CardPanel("Lập phiếu mượn mới", autoHeight: false);
            card.AddRow(body, fill: true);
            return card;
        }

        private Control BuildLoanListPage()
        {
            txtTimPhieu = Ui.TextBox("Mã phiếu hoặc tên độc giả", 280);
            txtTimPhieu.TextChanged += (s, e) => ApiFilterMasterGrid();
            cboLocTrangThai = Ui.ComboBox(new object[] { "Tất cả", "Đang mượn", "Quá hạn", "Đã hoàn tất" }, width: 170);
            cboLocTrangThai.SelectedIndexChanged += (s, e) => ApiFilterMasterGrid();

            var btnRefreshLoans = Ui.Button("Làm mới");
            btnRefreshLoans.Click += async (_, _) => await LoadLoansAsync();
            var btnViewDetail = Ui.Button("Chi tiết / Nhận trả", ButtonKind.Primary);
            btnViewDetail.Click += async (_, _) =>
            {
                if (dgvPhieuMuon.CurrentRow?.Tag is int loanId) await OpenNhanTraDialogAsync(loanId);
                else MessageBox.Show("Chọn phiếu mượn cần xem.");
            };

            var toolbar = new ToolbarRow();
            toolbar.AddField("Tìm phiếu / độc giả", txtTimPhieu);
            toolbar.AddField("Trạng thái", cboLocTrangThai);
            toolbar.AddActions(btnRefreshLoans, btnViewDetail);

            dgvPhieuMuon = Ui.Grid();
            dgvPhieuMuon.Columns.Add("MaPhieu", "Mã phiếu");
            dgvPhieuMuon.Columns["MaPhieu"].FillWeight = 55;
            dgvPhieuMuon.Columns.Add("DocGia", "Độc giả");
            dgvPhieuMuon.Columns["DocGia"].FillWeight = 130;
            dgvPhieuMuon.Columns.Add("NhanVien", "Người lập");
            dgvPhieuMuon.Columns.Add("NgayMuon", "Ngày mượn");
            dgvPhieuMuon.Columns.Add("HanTra", "Hạn trả");
            dgvPhieuMuon.Columns.Add("TrangThai", "Trạng thái");
            dgvPhieuMuon.CellDoubleClick += async (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvPhieuMuon.Rows[e.RowIndex].Tag is int loanId)
                    await OpenNhanTraDialogAsync(loanId);
            };

            var card = new CardPanel("Sổ phiếu mượn", autoHeight: false);
            card.AddRow(toolbar);
            card.AddRow(dgvPhieuMuon, fill: true);
            return card;
        }
    }
}
