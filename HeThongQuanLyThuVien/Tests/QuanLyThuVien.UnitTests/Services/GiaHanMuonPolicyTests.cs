using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.UnitTests.Services;

public sealed class GiaHanMuonPolicyTests
{
    private static readonly DateTime HomNay = new(2026, 10, 29);

    [Fact]
    public void KiemTra_PhieuDangMuon_ChoPhepGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(
            soLuongConMuon: 2, hanTra: HomNay.AddDays(7), soLanGiaHan: 0, conNo: 0, homNay: HomNay);

        Assert.Null(error);
    }

    [Fact]
    public void KiemTra_HanTraHomNay_VanChoPhepGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(1, HomNay, 1, 0, HomNay);

        Assert.Null(error);
    }

    [Fact]
    public void KiemTra_PhieuDaHoanTat_KhongChoGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(
            soLuongConMuon: 0, hanTra: HomNay.AddDays(3), soLanGiaHan: 0, conNo: 0, homNay: HomNay);

        Assert.Equal("Phiếu mượn đã hoàn tất, không thể gia hạn.", error);
    }

    [Fact]
    public void KiemTra_PhieuQuaHan_KhongChoGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(
            soLuongConMuon: 1, hanTra: HomNay.AddDays(-1), soLanGiaHan: 0, conNo: 0, homNay: HomNay);

        Assert.Contains("đã quá hạn", error);
    }

    [Fact]
    public void KiemTra_DaGiaHanDuSoLan_KhongChoGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(
            soLuongConMuon: 1, hanTra: HomNay.AddDays(7), soLanGiaHan: GiaHanMuonPolicy.SoLanToiDa,
            conNo: 0, homNay: HomNay);

        Assert.Contains("đủ 2 lần", error);
    }

    [Fact]
    public void KiemTra_ConNoTienPhat_KhongChoGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(
            soLuongConMuon: 1, hanTra: HomNay.AddDays(7), soLanGiaHan: 1, conNo: 15000, homNay: HomNay);

        Assert.Contains("chưa thanh toán", error);
    }

    [Fact]
    public void KiemTra_CoPhatChuaXacDinh_KhongChoGiaHan()
    {
        var error = GiaHanMuonPolicy.KiemTra(
            soLuongConMuon: 1, hanTra: HomNay.AddDays(7), soLanGiaHan: 0, conNo: null, homNay: HomNay);

        Assert.Contains("chưa xác định", error);
    }

    [Fact]
    public void TinhConNo_TruSoTienDaThu()
    {
        var lichSuTra = new List<ChiTietTraSach>
        {
            new() { SoNgayTre = 3, SoTienPhatPhaiThu = 30000, CacKhoanThu = new List<ThuTienPhat> { new() { SoTien = 10000 } } },
            new() { SoNgayTre = 0, SoTienPhatPhaiThu = 0 },
            new() { SoNgayTre = 2, SoTienPhatPhaiThu = 10000, CacKhoanThu = new List<ThuTienPhat> { new() { SoTien = 10000 } } }
        };

        var conNo = GiaHanMuonPolicy.TinhConNo(lichSuTra);

        Assert.Equal(20000, conNo);
    }

    [Fact]
    public void TinhConNo_CoPhatChuaXacDinh_TraVeNull()
    {
        var lichSuTra = new List<ChiTietTraSach>
        {
            new() { SoNgayTre = 2, SoTienPhatPhaiThu = null }
        };

        Assert.Null(GiaHanMuonPolicy.TinhConNo(lichSuTra));
    }
}
