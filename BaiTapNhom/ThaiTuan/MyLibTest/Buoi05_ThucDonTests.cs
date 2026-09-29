using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi05_ThucDonTests
    {
        [Fact]
        public void Test_HienThiThucDon_DungNoiDung()
        {
            // Arrange
            string mongDoi = "THUC DON\n" +
                              "1. Tinh dien tich tam giac\n" +
                              "2. Tinh dien tich hinh chu nhat\n" +
                              "3. Tinh dien tich hinh tron\n" +
                              "4. Thoat\n" +
                              "-------------------------------------------";

            // Act
            string thucTe = Buoi05_ThucDonLib.HienThiThucDon();

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Theory]
        [InlineData(1, "Ban moi vua chon chuc nang tinh dien tich tam giac.")]
        [InlineData(2, "Ban moi vua chon chuc nang tinh dien tich hinh chu nhat.")]
        [InlineData(3, "Ban moi vua chon chuc nang tinh dien tich hinh tron.")]
        [InlineData(0, "Lua chon khong hop le, vui long chon lai [1, 2, 3, 4].")]
        [InlineData(5, "Lua chon khong hop le, vui long chon lai [1, 2, 3, 4].")]
        [InlineData(-1, "Lua chon khong hop le, vui long chon lai [1, 2, 3, 4].")]
        public void Test_LayThongBaoChucNang_TraVeKetQuaDung(int luaChon, string expected)
        {
            // Act
            string actual = Buoi05_ThucDonLib.LayThongBaoChucNang(luaChon);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
