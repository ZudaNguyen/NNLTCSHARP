using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class TienNuocTests
    {
        [Theory]
        // 1 nguoi, 3m3 (muc 1): 3 * 4400 * 1.15 = 15180
        [InlineData(10, 13, 1, 15180)]

        // 1 nguoi, 5m3 (muc 2): (4*4400 + 1*8300) * 1.15 = 25900 * 1.15 = 29785
        [InlineData(10, 15, 1, 29785)]

        // 1 nguuoi, 10m3 (muc 3): (4*4400 +2*8300 + 4*10500) * 1.15 = 76200 * 1.15 = 87630
        [InlineData(10, 20, 1, 87630)]

        // 2 nguoi, 10m3 (muc 2 cua ho 2 nguoi: limit1=8, limit2=12)
        // (8*4400 + 2*8300) * 1.15 = 51800 * 1.15 = 59570
        [InlineData(10, 20, 2, 59570)]
        public void KiemTra_TinhTienNuoc(double cu, double moi, int soNguoi, double expected)
        {
            TienNuocLib lib = new TienNuocLib();
            Assert.Equal(expected, lib.TinhTienNuoc(cu, moi, soNguoi), 0);
        }
    }
}