using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B5
    {
        [Theory]
        [InlineData(1.0, 2)]  // 1 + 1/2 = 1.5 > 1.0 (n=2)
        [InlineData(1.5, 3)]  // 1 + 1/2 + 1/3 ≈ 1.833 > 1.5 (n=3)
        [InlineData(2.0, 4)]  // 1 + 1/2 + 1/3 + 1/4 ≈ 2.083 > 2.0 (n=4)
        [InlineData(0.5, 1)]  // 1 > 0.5 (n=1)
        [InlineData(0.0, 1)]  // 1 > 0.0 (n=1)
        [InlineData(-1.0, 1)] // 1 > -1.0 (n=1)
        public void Test_TimBeNhatThoa_NhieuTruongHop(double a, int mongDoi)
        {
            // Act
            int thucTe = MyLib.B4B5.TimBeNhatThoa(a);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}