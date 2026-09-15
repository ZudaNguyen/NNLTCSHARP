using Xunit;
using MyLib;
using System.Runtime.InteropServices;

namespace MyLib.Tests
{
    [Trait("Buoi", "03")]
    public class GiaTriHamSo1Tests
    {
        [Theory]
        [InlineData(-2, 0)]
        [InlineData(0, 0)]
        [InlineData(0.5, 0.5)]
        [InlineData(1, 1)]
        [InlineData(2, 16)]
        public void KiemTra_TinhF1(double x, double expected)
        {
            GiaTriHamSo1Lib lib = new GiaTriHamSo1Lib();
            Assert.Equal(expected, lib.TinhF1(x));
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(2, 17)]
        [InlineData(3, 0.038461538461538464)]
        public void KiemTra_TinhF2(double x, double expected)
        {
            GiaTriHamSo1Lib lib = new GiaTriHamSo1Lib();
            Assert.Equal(expected, lib.TinhF2(x));
        }

    }
}