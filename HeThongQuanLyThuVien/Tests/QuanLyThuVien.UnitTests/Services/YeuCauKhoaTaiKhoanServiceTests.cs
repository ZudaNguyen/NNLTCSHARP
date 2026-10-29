using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.UnitTests.Services;

public sealed class YeuCauKhoaTaiKhoanServiceTests
{
    private const string ActiveStatus = "Hoạt động";
    private const string LockedStatus = "Bị khóa";
    private const string LockedCardStatus = "Khóa thẻ";

    private static TaiKhoan ReaderAccount(int maTaiKhoan = 5, int maDocGia = 3, string role = AccountRoles.Reader) => new()
    {
        MaTaiKhoan = maTaiKhoan,
        TenDangNhap = "docgia05",
        TenHienThi = "Độc Giả Kiểm Thử",
        VaiTro = role,
        TrangThai = ActiveStatus,
        MaDocGia = role == AccountRoles.Reader ? maDocGia : null,
        DocGia = role == AccountRoles.Reader
            ? new DocGia { MaDocGia = maDocGia, HoTen = "Độc Giả Kiểm Thử", TrangThai = ActiveStatus }
            : null
    };

    private static YeuCauKhoaTaiKhoan PendingRequest(
        int maYeuCau = 1,
        TaiKhoan? account = null,
        string status = YeuCauKhoaTaiKhoanTrangThai.ChoDuyet) => new()
    {
        MaYeuCau = maYeuCau,
        MaTaiKhoan = account?.MaTaiKhoan ?? 5,
        MaNhanVien = 2,
        LyDo = "Độc giả làm mất sách và chưa bồi thường.",
        ThoiHanKhoaNgay = 30,
        TrangThai = status,
        NgayTao = new DateTime(2026, 10, 29, 9, 0, 0),
        IdempotencyKey = Guid.NewGuid(),
        TaiKhoan = account ?? ReaderAccount(),
        NhanVien = new NhanVien { MaNhanVien = 2, HoTen = "Thủ Thư Kiểm Thử" }
    };

    [Fact]
    public async Task ApproveAsync_KhoaTaiKhoanVaThe_ThuHoiMoiPhien()
    {
        var repository = new FakeRepository(PendingRequest());
        var service = new YeuCauKhoaTaiKhoanService(repository);

        var result = await service.ApproveAsync(1, "Quản Trị Kiểm Thử", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(YeuCauKhoaTaiKhoanTrangThai.DaDuyet, result!.TrangThai);
        Assert.Equal("Quản Trị Kiểm Thử", result.NguoiDuyet);

        var account = repository.Accounts.Single();
        Assert.Equal(LockedStatus, account.TrangThai);
        Assert.Equal(LockedCardStatus, account.DocGia!.TrangThai);
        Assert.Equal(2, repository.RevokedSessions);
    }

    [Fact]
    public async Task ApproveAsync_DuyetLanHai_KhongThuHoiPhienThem()
    {
        var repository = new FakeRepository(PendingRequest());
        var service = new YeuCauKhoaTaiKhoanService(repository);

        await service.ApproveAsync(1, "Quản Trị", CancellationToken.None);
        var second = await service.ApproveAsync(1, "Quản Trị", CancellationToken.None);

        Assert.NotNull(second);
        Assert.Equal(YeuCauKhoaTaiKhoanTrangThai.DaDuyet, second!.TrangThai);
        Assert.Equal(2, repository.RevokedSessions);
    }

    [Theory]
    [InlineData(YeuCauKhoaTaiKhoanTrangThai.TuChoi)]
    [InlineData(YeuCauKhoaTaiKhoanTrangThai.DaHuy)]
    public async Task ApproveAsync_KhongChoDuyet_NemAccountLockRequestException(string status)
    {
        var service = new YeuCauKhoaTaiKhoanService(
            new FakeRepository(PendingRequest(status: status)));

        await Assert.ThrowsAsync<AccountLockRequestException>(
            () => service.ApproveAsync(1, "Quản Trị", CancellationToken.None));
    }

    [Fact]
    public async Task ApproveAsync_TaiKhoanKhongPhaiDocGia_NemAccountLockRequestException()
    {
        var service = new YeuCauKhoaTaiKhoanService(
            new FakeRepository(PendingRequest(account: ReaderAccount(role: AccountRoles.Librarian))));

        await Assert.ThrowsAsync<AccountLockRequestException>(
            () => service.ApproveAsync(1, "Quản Trị", CancellationToken.None));
    }

    [Fact]
    public async Task ApproveAsync_KhongTimThay_TraNull()
    {
        var service = new YeuCauKhoaTaiKhoanService(new FakeRepository());

        Assert.Null(await service.ApproveAsync(99, "Quản Trị", CancellationToken.None));
    }

    [Fact]
    public async Task RejectAsync_KhongNhapLyDo_DungLyDoMacDinh()
    {
        var repository = new FakeRepository(PendingRequest());
        var service = new YeuCauKhoaTaiKhoanService(repository);

        var result = await service.RejectAsync(1, "Quản Trị", "   ", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(YeuCauKhoaTaiKhoanTrangThai.TuChoi, result!.TrangThai);
        Assert.Equal("Admin từ chối yêu cầu khóa tài khoản.", result.LyDoTuChoi);
        Assert.Equal(ActiveStatus, repository.Accounts.Single().TrangThai);
        Assert.Equal(0, repository.RevokedSessions);
    }

    [Fact]
    public async Task RejectAsync_GiuNguyenLyDoAdminNhap()
    {
        var service = new YeuCauKhoaTaiKhoanService(new FakeRepository(PendingRequest()));

        var result = await service.RejectAsync(1, "Quản Trị", "Hồ sơ chưa đủ căn cứ.", CancellationToken.None);

        Assert.Equal("Hồ sơ chưa đủ căn cứ.", result!.LyDoTuChoi);
    }

    [Fact]
    public async Task CreateAsync_TaoYeuCauChoDuyet_DungNhanVienGui()
    {
        var account = ReaderAccount();
        var repository = new FakeRepository([account], []);
        var service = new YeuCauKhoaTaiKhoanService(repository);

        var result = await service.CreateAsync(2, new TaoYeuCauKhoaTaiKhoanDto
        {
            MaTaiKhoan = account.MaTaiKhoan,
            LyDo = "  Độc giả vi phạm nội quy thư viện.  ",
            ThoiHanKhoaNgay = 15
        }, CancellationToken.None);

        Assert.Equal(YeuCauKhoaTaiKhoanTrangThai.ChoDuyet, result.TrangThai);
        Assert.Equal("Độc giả vi phạm nội quy thư viện.", result.LyDo);
        Assert.Equal(15, result.ThoiHanKhoaNgay);
        Assert.Equal(2, result.MaNhanVien);
        Assert.Equal("Thủ Thư Kiểm Thử", result.TenNhanVien);
        Assert.True(result.CoTheHuy);
    }

    [Fact]
    public async Task CreateAsync_TrungYeuCauDangCho_NemConflict()
    {
        var account = ReaderAccount();
        var repository = new FakeRepository([account], [PendingRequest(account: account)]);
        var service = new YeuCauKhoaTaiKhoanService(repository);

        await Assert.ThrowsAsync<AccountLockRequestConflictException>(
            () => service.CreateAsync(2, new TaoYeuCauKhoaTaiKhoanDto
            {
                MaTaiKhoan = account.MaTaiKhoan,
                LyDo = "Độc giả vi phạm nội quy thư viện."
            }, CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(366)]
    public async Task CreateAsync_ThoiHanNgoaiPhamVi_NemAccountLockRequestException(int days)
    {
        var service = new YeuCauKhoaTaiKhoanService(new FakeRepository([ReaderAccount()], []));

        await Assert.ThrowsAsync<AccountLockRequestException>(
            () => service.CreateAsync(2, new TaoYeuCauKhoaTaiKhoanDto
            {
                MaTaiKhoan = 5,
                LyDo = "Độc giả vi phạm nội quy thư viện.",
                ThoiHanKhoaNgay = days
            }, CancellationToken.None));
    }

    [Fact]
    public async Task CancelAsync_DungNguoiGui_ChuyenDaHuy()
    {
        var service = new YeuCauKhoaTaiKhoanService(new FakeRepository(PendingRequest()));

        var result = await service.CancelAsync(2, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(YeuCauKhoaTaiKhoanTrangThai.DaHuy, result!.TrangThai);
        Assert.False(result.CoTheHuy);
    }

    [Fact]
    public async Task CancelAsync_KhacNguoiGui_TraNull()
    {
        var repository = new FakeRepository(PendingRequest());
        var service = new YeuCauKhoaTaiKhoanService(repository);

        Assert.Null(await service.CancelAsync(99, 1, CancellationToken.None));
        Assert.Equal(YeuCauKhoaTaiKhoanTrangThai.ChoDuyet, repository.Items.Single().TrangThai);
    }

    [Fact]
    public async Task SearchAsync_ThuThu_ChiTimTheoNhanVienCuaMinh()
    {
        var repository = new FakeRepository(PendingRequest());
        var service = new YeuCauKhoaTaiKhoanService(repository);

        await service.SearchAsync(2, null, null, CancellationToken.None);

        Assert.Equal(2, repository.LastSearchEmployeeId);
    }

    [Fact]
    public async Task SearchAsync_Admin_TruyenNullDeXemTatCa()
    {
        var repository = new FakeRepository(PendingRequest());
        var service = new YeuCauKhoaTaiKhoanService(repository);

        await service.SearchAsync(null, null, null, CancellationToken.None);

        Assert.Null(repository.LastSearchEmployeeId);
    }

    private sealed class FakeRepository(params YeuCauKhoaTaiKhoan[] seed) : IYeuCauKhoaTaiKhoanRepository
    {
        private readonly List<YeuCauKhoaTaiKhoan> _items = [.. seed];
        private readonly List<TaiKhoan> _accounts =
            [.. seed.Where(x => x.TaiKhoan is not null).Select(x => x.TaiKhoan)];

        public FakeRepository(TaiKhoan[] accounts, YeuCauKhoaTaiKhoan[] requests) : this(requests)
        {
            _accounts.AddRange(accounts.Where(account => _accounts.All(x => x.MaTaiKhoan != account.MaTaiKhoan)));
        }

        public IReadOnlyList<YeuCauKhoaTaiKhoan> Items => _items;
        public IReadOnlyList<TaiKhoan> Accounts => _accounts;
        public int RevokedSessions { get; private set; }
        public int? LastSearchEmployeeId { get; private set; }

        public Task<IReadOnlyList<YeuCauKhoaTaiKhoan>> SearchAsync(
            int? maNhanVien, string? trangThai, string? search, CancellationToken cancellationToken)
        {
            LastSearchEmployeeId = maNhanVien;
            return Task.FromResult<IReadOnlyList<YeuCauKhoaTaiKhoan>>(
                _items.Where(x => maNhanVien is null || x.MaNhanVien == maNhanVien)
                      .Where(x => trangThai is null || x.TrangThai == trangThai)
                      .ToList());
        }

        public Task<YeuCauKhoaTaiKhoan?> GetByIdAsync(int maYeuCau, CancellationToken cancellationToken) =>
            Task.FromResult(_items.FirstOrDefault(x => x.MaYeuCau == maYeuCau));

        public Task<IReadOnlyList<TaiKhoanDocGiaOptionDto>> FindReaderAccountsAsync(
            string? search, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaiKhoanDocGiaOptionDto>>(
                _accounts.Where(x => x.VaiTro == AccountRoles.Reader)
                    .Select(x => new TaiKhoanDocGiaOptionDto
                    {
                        MaTaiKhoan = x.MaTaiKhoan,
                        TenDangNhap = x.TenDangNhap,
                        TenHienThi = x.TenHienThi,
                        MaDocGia = x.MaDocGia,
                        TenDocGia = x.DocGia?.HoTen,
                        TrangThaiTaiKhoan = x.TrangThai,
                        TrangThaiDocGia = x.DocGia?.TrangThai
                    }).ToList());

        public Task<YeuCauKhoaTaiKhoan> CreateAsync(
            int maNhanVien, TaoYeuCauKhoaTaiKhoanDto request, Guid idempotencyKey,
            CancellationToken cancellationToken)
        {
            if (_items.FirstOrDefault(x => x.IdempotencyKey == idempotencyKey) is { } existing)
                return Task.FromResult(existing);

            var account = _accounts.FirstOrDefault(x => x.MaTaiKhoan == request.MaTaiKhoan)
                ?? throw new AccountLockRequestException("Không tìm thấy tài khoản cần khóa.");
            if (account.VaiTro != AccountRoles.Reader)
                throw new AccountLockRequestException("Chỉ được gửi yêu cầu khóa tài khoản độc giả.");
            if (account.TrangThai != ActiveStatus)
                throw new AccountLockRequestConflictException("Tài khoản này đã bị khóa.");
            if (account.DocGia is null)
                throw new AccountLockRequestException("Tài khoản độc giả chưa liên kết hồ sơ độc giả.");
            if (account.DocGia.TrangThai != ActiveStatus)
                throw new AccountLockRequestConflictException("Thẻ độc giả này đã bị khóa.");
            if (_items.Any(x => x.MaTaiKhoan == request.MaTaiKhoan
                                && x.TrangThai == YeuCauKhoaTaiKhoanTrangThai.ChoDuyet))
                throw new AccountLockRequestConflictException(
                    "Tài khoản này đã có một yêu cầu khóa đang chờ duyệt.");

            var entity = new YeuCauKhoaTaiKhoan
            {
                MaYeuCau = _items.Count + 1,
                MaTaiKhoan = request.MaTaiKhoan,
                MaNhanVien = maNhanVien,
                LyDo = request.LyDo.Trim(),
                ThoiHanKhoaNgay = request.ThoiHanKhoaNgay,
                TrangThai = YeuCauKhoaTaiKhoanTrangThai.ChoDuyet,
                NgayTao = new DateTime(2026, 10, 29, 10, 0, 0),
                IdempotencyKey = idempotencyKey,
                TaiKhoan = account,
                NhanVien = new NhanVien { MaNhanVien = maNhanVien, HoTen = "Thủ Thư Kiểm Thử" }
            };
            _items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<YeuCauKhoaTaiKhoan?> CancelAsync(
            int maNhanVien, int maYeuCau, CancellationToken cancellationToken)
        {
            var entity = _items.FirstOrDefault(
                x => x.MaYeuCau == maYeuCau && x.MaNhanVien == maNhanVien);
            if (entity is null) return Task.FromResult<YeuCauKhoaTaiKhoan?>(null);
            if (entity.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
                throw new AccountLockRequestException("Chỉ có thể hủy yêu cầu đang chờ duyệt.");

            entity.TrangThai = YeuCauKhoaTaiKhoanTrangThai.DaHuy;
            entity.NgayDuyet = new DateTime(2026, 10, 29, 11, 0, 0);
            return Task.FromResult<YeuCauKhoaTaiKhoan?>(entity);
        }

        public Task<YeuCauKhoaTaiKhoan?> ApproveAsync(
            int maYeuCau, string? nguoiDuyet, CancellationToken cancellationToken)
        {
            var entity = _items.FirstOrDefault(x => x.MaYeuCau == maYeuCau);
            if (entity is null) return Task.FromResult<YeuCauKhoaTaiKhoan?>(null);
            if (entity.TrangThai == YeuCauKhoaTaiKhoanTrangThai.DaDuyet)
                return Task.FromResult<YeuCauKhoaTaiKhoan?>(entity);
            if (entity.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
                throw new AccountLockRequestException(
                    $"Yêu cầu đang ở trạng thái '{entity.TrangThai}' nên không thể duyệt.");

            var account = entity.TaiKhoan
                ?? throw new AccountLockRequestException("Không tìm thấy tài khoản cần khóa.");
            if (account.VaiTro != AccountRoles.Reader)
                throw new AccountLockRequestException("Chỉ được khóa tài khoản độc giả.");

            account.TrangThai = LockedStatus;
            if (account.DocGia is not null) account.DocGia.TrangThai = LockedCardStatus;
            entity.TrangThai = YeuCauKhoaTaiKhoanTrangThai.DaDuyet;
            entity.NguoiDuyet = string.IsNullOrWhiteSpace(nguoiDuyet) ? null : nguoiDuyet.Trim();
            entity.NgayDuyet = new DateTime(2026, 10, 29, 11, 0, 0);
            RevokedSessions += 2;
            return Task.FromResult<YeuCauKhoaTaiKhoan?>(entity);
        }

        public Task<YeuCauKhoaTaiKhoan?> RejectAsync(
            int maYeuCau, string? nguoiDuyet, string? lyDo, CancellationToken cancellationToken)
        {
            var entity = _items.FirstOrDefault(x => x.MaYeuCau == maYeuCau);
            if (entity is null) return Task.FromResult<YeuCauKhoaTaiKhoan?>(null);
            if (entity.TrangThai == YeuCauKhoaTaiKhoanTrangThai.TuChoi)
                return Task.FromResult<YeuCauKhoaTaiKhoan?>(entity);
            if (entity.TrangThai != YeuCauKhoaTaiKhoanTrangThai.ChoDuyet)
                throw new AccountLockRequestException(
                    $"Yêu cầu đang ở trạng thái '{entity.TrangThai}' nên không thể từ chối.");

            entity.TrangThai = YeuCauKhoaTaiKhoanTrangThai.TuChoi;
            entity.NguoiDuyet = string.IsNullOrWhiteSpace(nguoiDuyet) ? null : nguoiDuyet.Trim();
            entity.NgayDuyet = new DateTime(2026, 10, 29, 11, 0, 0);
            entity.LyDoTuChoi = string.IsNullOrWhiteSpace(lyDo)
                ? "Admin từ chối yêu cầu khóa tài khoản."
                : lyDo.Trim();
            return Task.FromResult<YeuCauKhoaTaiKhoan?>(entity);
        }
    }
}
