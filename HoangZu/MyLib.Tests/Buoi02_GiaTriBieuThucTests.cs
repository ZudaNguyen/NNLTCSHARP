using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi02_GiaTriBieuThucTests
    {
        [Theory]
        [InlineData(0, 1)]         // f(0) = 1 + 0 + 0 - 0 = 1
        [InlineData(1, 2)]         // f(1) = 1 + 2(1) + 3(1) - 4(1) = 2
        [InlineData(-1, 6)]        // f(-1) = 1 - 2 + 3 - (-4) = 6
        [InlineData(0.5, 2.25)]    // f(0.5) = 1 + 1 + 0.75 - 0.5 = 2.25
        [InlineData(-0.5, 1.25)]   // f(-0.5) = 1 - 1 + 0.75 - (-0.5) = 1.25
        public void Test_TinhGiaTri_TraVeKetQuaDung(double x, double expected)
        {
            // Gọi hàm từ thư viện
            double actual = Buoi02_GiaTriBieuThucLib.TinhGiaTri(x);
            
            // So sánh kết quả (dùng overload có sai số 5 chữ số thập phân vì đây là số thực double)
            Assert.Equal(expected, actual, 5);
        }
    }
}