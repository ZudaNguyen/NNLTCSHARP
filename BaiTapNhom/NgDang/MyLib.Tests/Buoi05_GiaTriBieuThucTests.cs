using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "05")]
    public class GiaTriBieuThucTests
    {
        [Theory]
        [InlineData(1, 1.0)]          // S1(1) = 1/1 = 1
        [InlineData(2, 1.5)]          // S1(2) = 1 + 1/2 = 1.5
        [InlineData(3, 1.83333)]      // S1(3) = 1 + 1/2 + 1/3 = 1.83333...
        public void KiemTra_TinhS1(int n, double expected)
        {
            GiaTriBieuThucLib lib = new GiaTriBieuThucLib();
            // So sánh làm tròn 5 chữ số thập phân
            Assert.Equal(expected, lib.TinhS1(n), 5); 
        }

        [Theory]
        [InlineData(1, 1.0)]          // S2(1) = 1/1 = 1
        [InlineData(2, -0.5)]         // S2(2) = 1/2 - 1/1 = -0.5
        [InlineData(3, 0.83333)]      // S2(3) = 1/3 - 1/2 + 1/1 = 0.83333...
        public void KiemTra_TinhS2(int n, double expected)
        {
            GiaTriBieuThucLib lib = new GiaTriBieuThucLib();
            Assert.Equal(expected, lib.TinhS2(n), 5); 
        }
    }
}