using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B6Tests
    {
        [Theory]
        [InlineData(6, "6 = 1 + 2 + 3 la so hoan hao.")]
        [InlineData(28, "28 = 1 + 2 + 4 + 7 + 14 la so hoan hao.")]
        [InlineData(8, "8 khong la so hoan hao.")]
        [InlineData(12, "12 khong la so hoan hao.")]
        [InlineData(1, "1 khong la so hoan hao.")]
        [InlineData(-6, "-6 khong la so hoan hao.")]
        public void Test_KiemTraSoHoanHao_NhieuTruongHop(int n, string mongDoi)
        {
            // Act
            string thucTe = B4B6.KiemTraSoHoanHao(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}