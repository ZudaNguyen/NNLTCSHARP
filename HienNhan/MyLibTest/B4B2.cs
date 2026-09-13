using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B2
    {
        [Theory]
        [InlineData(3, "Thang 3 thuoc qui 1.")]
        [InlineData(5, "Thang 5 thuoc qui 2.")]
        [InlineData(9, "Thang 9 thuoc qui 3.")]
        [InlineData(12, "Thang 12 thuoc qui 4.")]
        [InlineData(0, "Thang khong hop le.")]
        [InlineData(13, "Thang khong hop le.")]
        public void Test_TimQui_NhieuTruongHop(int thang, string mongDoi)
        {
            // Act
            string thucTe = MyLib.B4B2.TimQui(thang);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}