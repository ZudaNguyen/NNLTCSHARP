using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi01_TinhDienTichTamGiacTests
    {
        [Theory]
        [InlineData(2, 4, 3, "Dien tich tam giac S = 2.90")]   // Vi du trong de bai
        [InlineData(2, 2, 2, "Dien tich tam giac S = 1.73")]   // Tam giac deu canh 2 -> S = can(3)
        [InlineData(3, 4, 5, "Dien tich tam giac S = 6.00")]   // Tam giac vuong 3-4-5
        [InlineData(1, 2, 100, "Ba canh khong tao thanh mot tam giac.")] // Bo ba khong hop le
        [InlineData(0, 1, 1, "Ba canh khong tao thanh mot tam giac.")]  // Canh bang 0
        public void Test_TinhDienTichTamGiac_TraVeKetQuaDung(double a, double b, double c, string expected)
        {
            // Act
            string actual = Buoi01_TinhDienTichTamGiacLib.TinhDienTichTamGiac(a, b, c);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
