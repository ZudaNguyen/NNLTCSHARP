using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B4Tests
    {
        [Theory]
        [InlineData(5, 3, 4, "Ba so (5, 3, 4) tao thanh duoc tam giac.\nTam giac tao thanh la tam giac vuong.")]
        [InlineData(3, 3, 3, "Ba so (3, 3, 3) tao thanh duoc tam giac.\nTam giac tao thanh la tam giac deu.")]
        [InlineData(3, 4, 3, "Ba so (3, 4, 3) tao thanh duoc tam giac.\nTam giac tao thanh la tam giac can.")]
        [InlineData(2, 4, 7, "Ba so (2, 4, 7) khong tao thanh tam giac.")]
        [InlineData(0, 5, 5, "Ba so (0, 5, 5) khong tao thanh tam giac.")]
        public void Test_PhanLoaiTamGiac_NhieuTruongHop(double a, double b, double c, string mongDoi)
        {
            // Act
            string thucTe = B4B4.PhanLoaiTamGiac(a, b, c);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}