using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.UnitTests.Services;

public sealed class YeuCauMuonServiceTests
{
    private static YeuCauMuon Service_Request(int maYeuCau = 1, string trangThai = YeuCauMuonTrangThai.ChoDuyet) => new()
    {
        MaYeuCau = maYeuCau,
        MaDocGia = 2,
        MaSach = 1,
        SoLuong = 1,
        NgayTao = new DateTime(2026, 10, 29, 9, 0, 0),
        TrangThai = trangThai,
        IdempotencyKey = Guid.NewGuid(),
        DocGia = new DocGia { MaDocGia = 2, HoTen = "Độc Giả Kiểm Thử" },
        Sach = new Sach { MaSach = 1, TenSach = "Clean Code", ISBN = "9780306406157" }
    };

    [Fact]
    public async Task ApproveAsync_TaoPhieuMuon_14Ngay_VaDanhDauDaDuyet()
    {
        var request = Service_Request();
        var repository = new FakeRepository(request);
        var loans = new FakeMuonTraService(maPhieu: 7);
        var service = new YeuCauMuonService(repository, loans);

        var result = await service.ApproveAsync(1, "Trần Thị Thủ Thư", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(YeuCauMuonTrangThai.DaDuyet, result!.TrangThai);
        Assert.Equal(7, result.MaPhieu);
        Assert.Equal("PM-000007", result.SoPhieu);
        Assert.Equal("Trần Thị Thủ Thư", result.NguoiDuyet);

        var sent = Assert.Single(loans.Created);
        Assert.Equal(2, sent.MaDocGia);
        Assert.Equal(14, (sent.HanTra.Date - sent.NgayMuon.Date).Days);
        Assert.Equal("Trần Thị Thủ Thư", sent.NguoiLap);
        Assert.Equal(request.IdempotencyKey, sent.IdempotencyKey);
        Assert.Equal(1, Assert.Single(sent.ChiTiet).MaSach);
    }

    [Fact]
    public async Task ApproveAsync_LanHai_KhongTaoPhieuMoi()
    {
        var request = Service_Request(trangThai: YeuCauMuonTrangThai.DaDuyet);
        request.MaPhieu = 7;
        var repository = new FakeRepository(request);
        var loans = new FakeMuonTraService(maPhieu: 8);
        var service = new YeuCauMuonService(repository, loans);

        var result = await service.ApproveAsync(1, "Thủ thư", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(7, result!.MaPhieu);
        Assert.Empty(loans.Created);
    }

    [Theory]
    [InlineData(YeuCauMuonTrangThai.TuChoi)]
    [InlineData(YeuCauMuonTrangThai.DaHuy)]
    public async Task ApproveAsync_KhongChoDuyet_NemBorrowingRuleException(string trangThai)
    {
        var service = new YeuCauMuonService(
            new FakeRepository(Service_Request(trangThai: trangThai)),
            new FakeMuonTraService());

        await Assert.ThrowsAsync<BorrowingRuleException>(
            () => service.ApproveAsync(1, "Thủ thư", CancellationToken.None));
    }

    [Fact]
    public async Task ApproveAsync_KhongTimThay_TraNull()
    {
        var service = new YeuCauMuonService(new FakeRepository(), new FakeMuonTraService());

        Assert.Null(await service.ApproveAsync(99, "Thủ thư", CancellationToken.None));
    }

    [Fact]
    public async Task RejectAsync_MacDinhLyDo_KhiDeTrong()
    {
        var service = new YeuCauMuonService(
            new FakeRepository(Service_Request()),
            new FakeMuonTraService());

        var result = await service.RejectAsync(1, "Thủ thư", "   ", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(YeuCauMuonTrangThai.TuChoi, result!.TrangThai);
        Assert.Equal("Thủ thư từ chối yêu cầu.", result.LyDoTuChoi);
    }

    [Fact]
    public async Task RejectAsync_GiuNguyenLyDoDoNguoiDungNhap()
    {
        var service = new YeuCauMuonService(
            new FakeRepository(Service_Request()),
            new FakeMuonTraService());

        var result = await service.RejectAsync(1, "Thủ thư", "Sách đang bảo trì.", CancellationToken.None);

        Assert.Equal("Sách đang bảo trì.", result!.LyDoTuChoi);
    }

    [Fact]
    public async Task CancelAsync_ChiHuyDuocYeuCauDangCho()
    {
        var service = new YeuCauMuonService(
            new FakeRepository(Service_Request()),
            new FakeMuonTraService());

        var result = await service.CancelAsync(2, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(YeuCauMuonTrangThai.DaHuy, result!.TrangThai);
    }

    [Fact]
    public async Task CancelAsync_KhongPhaiCuaDocGia_TraNull()
    {
        var service = new YeuCauMuonService(
            new FakeRepository(Service_Request()),
            new FakeMuonTraService());

        Assert.Null(await service.CancelAsync(99, 1, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_TraVeYeuCauDangChoDuyet()
    {
        var repository = new FakeRepository();
        var service = new YeuCauMuonService(repository, new FakeMuonTraService());

        var result = await service.CreateAsync(
            2, new TaoYeuCauMuonDto { MaSach = 1, SoLuong = 1, GhiChu = " Ghi chú " }, CancellationToken.None);

        Assert.Equal(YeuCauMuonTrangThai.ChoDuyet, result.TrangThai);
        Assert.Equal("Độc Giả Kiểm Thử", result.TenDocGia);
        Assert.Equal("Clean Code", result.TenSach);
        Assert.Equal("Ghi chú", result.GhiChu);
        Assert.True(result.CoTheHuy);
    }

    private sealed class FakeRepository(params YeuCauMuon[] seed) : IYeuCauMuonRepository
    {
        private readonly List<YeuCauMuon> _items = [.. seed];

        public Task<IReadOnlyList<YeuCauMuon>> GetForReaderAsync(
            int readerId, string? trangThai, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<YeuCauMuon>>(
                _items.Where(x => x.MaDocGia == readerId &&
                                  (trangThai is null || x.TrangThai == trangThai)).ToList());

        public Task<YeuCauMuon?> GetByIdForReaderAsync(
            int readerId, int maYeuCau, CancellationToken cancellationToken) =>
            Task.FromResult(_items.FirstOrDefault(x => x.MaYeuCau == maYeuCau && x.MaDocGia == readerId));

        public Task<YeuCauMuon?> GetByIdAsync(int maYeuCau, CancellationToken cancellationToken) =>
            Task.FromResult(_items.FirstOrDefault(x => x.MaYeuCau == maYeuCau));

        public Task<IReadOnlyList<YeuCauMuon>> SearchAsync(
            string? search, string? trangThai, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<YeuCauMuon>>(
                _items.Where(x => trangThai is null || x.TrangThai == trangThai).ToList());

        public Task<YeuCauMuon> CreateAsync(
            int readerId, TaoYeuCauMuonDto request, Guid idempotencyKey, CancellationToken cancellationToken)
        {
            var entity = new YeuCauMuon
            {
                MaYeuCau = _items.Count + 1,
                MaDocGia = readerId,
                MaSach = request.MaSach,
                SoLuong = request.SoLuong,
                NgayTao = new DateTime(2026, 10, 29, 10, 0, 0),
                TrangThai = YeuCauMuonTrangThai.ChoDuyet,
                GhiChu = string.IsNullOrWhiteSpace(request.GhiChu) ? null : request.GhiChu.Trim(),
                IdempotencyKey = idempotencyKey,
                DocGia = new DocGia { MaDocGia = readerId, HoTen = "Độc Giả Kiểm Thử" },
                Sach = new Sach { MaSach = request.MaSach, TenSach = "Clean Code", ISBN = "9780306406157" }
            };
            _items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<YeuCauMuon?> CancelAsync(int readerId, int maYeuCau, CancellationToken cancellationToken)
        {
            var entity = _items.FirstOrDefault(x => x.MaYeuCau == maYeuCau && x.MaDocGia == readerId);
            if (entity is null) return Task.FromResult<YeuCauMuon?>(null);
            if (entity.TrangThai != YeuCauMuonTrangThai.ChoDuyet)
                throw new BorrowingRuleException("Chỉ có thể hủy yêu cầu đang chờ duyệt.");

            entity.TrangThai = YeuCauMuonTrangThai.DaHuy;
            entity.NgayDuyet = new DateTime(2026, 10, 29, 11, 0, 0);
            return Task.FromResult<YeuCauMuon?>(entity);
        }

        public Task<YeuCauMuon> MarkApprovedAsync(
            int maYeuCau, string? nguoiDuyet, int maPhieu, CancellationToken cancellationToken)
        {
            var entity = _items.First(x => x.MaYeuCau == maYeuCau);
            if (entity.TrangThai == YeuCauMuonTrangThai.ChoDuyet)
            {
                entity.TrangThai = YeuCauMuonTrangThai.DaDuyet;
                entity.MaPhieu = maPhieu;
                entity.NguoiDuyet = nguoiDuyet;
                entity.NgayDuyet = new DateTime(2026, 10, 29, 11, 0, 0);
            }

            return Task.FromResult(entity);
        }

        public Task<YeuCauMuon?> MarkRejectedAsync(
            int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken)
        {
            var entity = _items.FirstOrDefault(x => x.MaYeuCau == maYeuCau);
            if (entity is null) return Task.FromResult<YeuCauMuon?>(null);

            entity.TrangThai = YeuCauMuonTrangThai.TuChoi;
            entity.NguoiDuyet = nguoiDuyet;
            entity.NgayDuyet = new DateTime(2026, 10, 29, 11, 0, 0);
            entity.LyDoTuChoi = string.IsNullOrWhiteSpace(lyDo)
                ? "Thủ thư từ chối yêu cầu."
                : lyDo.Trim();
            return Task.FromResult<YeuCauMuon?>(entity);
        }
    }

    private sealed class FakeMuonTraService(int maPhieu = 1) : IMuonTraService
    {
        public List<TaoMuonTraDto> Created { get; } = [];

        public Task<MuonTraChiTietDto> CreateAsync(TaoMuonTraDto request, CancellationToken cancellationToken)
        {
            Created.Add(request);
            return Task.FromResult(new MuonTraChiTietDto
            {
                MaPhieu = maPhieu,
                MaDocGia = request.MaDocGia,
                NgayMuon = request.NgayMuon,
                HanTra = request.HanTra,
                NguoiLap = request.NguoiLap,
                TrangThai = "Đang mượn",
                SoLuongConMuon = request.ChiTiet.Sum(x => x.SoLuong)
            });
        }

        public Task<IReadOnlyList<MuonTraDto>> SearchAsync(
            string? search, string? trangThai, string? loaiNgay, DateTime? tuNgay, DateTime? denNgay,
            bool chiQuaHan, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MuonTraDto>>([]);

        public Task<IReadOnlyList<MuonTraDto>> GetForReaderAsync(
            int readerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MuonTraDto>>([]);

        public Task<MuonTraChiTietDto?> GetByIdForReaderAsync(
            int readerId, int maPhieu, CancellationToken cancellationToken) =>
            Task.FromResult<MuonTraChiTietDto?>(null);

        public Task<MuonTraChiTietDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<MuonTraChiTietDto?>(null);

        public Task<IReadOnlyList<SachMuonOptionDto>> GetSachConHangAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<SachMuonOptionDto>>([]);

        public Task<IReadOnlyList<QuaHanDto>> GetQuaHanAsync(
            string? search, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<QuaHanDto>>([]);

        public Task<MuonTraChiTietDto?> TraChiTietAsync(
            int maPhieu, int maChiTiet, TraChiTietDto request, CancellationToken cancellationToken) =>
            Task.FromResult<MuonTraChiTietDto?>(null);

        public Task<TienPhatDto?> GetTienPhatAsync(int maPhieu, CancellationToken cancellationToken) =>
            Task.FromResult<TienPhatDto?>(null);

        public Task<TienPhatDto?> GetTienPhatForReaderAsync(
            int readerId, int maPhieu, CancellationToken cancellationToken) =>
            Task.FromResult<TienPhatDto?>(null);

        public Task<TienPhatDto?> UpdateTienPhatAsync(
            int maPhieu, CapNhatTienPhatDto request, CancellationToken cancellationToken) =>
            Task.FromResult<TienPhatDto?>(null);

        public Task<KhoanThuTienPhatDto> ThuTienPhatAsync(
            int maPhieu, ThuTienPhatDto request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<CauHinhTienPhatDto> GetCauHinhTienPhatAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<CauHinhTienPhatDto> SetCauHinhTienPhatAsync(
            decimal mucPhatMoiNgay, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
