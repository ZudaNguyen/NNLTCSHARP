using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi09_DaySoChanTests
    {
        [Fact]
        public void Test_ThongKeDaySo_ViDuTrongDeBai()
        {
            // Arrange
            int[] daySo = { 2, 0, 5, 8, 7 };
            string mongDoi = "Day so co 5 phan tu 2 0 5 8 7\n" +
                              "* Day so chan\n" +
                              "Day so co 3 so chan: 2 0 8 voi tong la 10, trung binh cong la 3.33.";

            // Act
            string thucTe = Buoi09_DaySoChanLib.ThongKeDaySo(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_ThongKeDaySo_KhongCoSoChan()
        {
            // Arrange
            int[] daySo = { 1, 3, 5 };
            string mongDoi = "Day so co 3 phan tu 1 3 5\n" +
                              "* Day so chan\n" +
                              "Day so khong co so chan nao.";

            // Act
            string thucTe = Buoi09_DaySoChanLib.ThongKeDaySo(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_ThongKeDaySo_TatCaDeuChan()
        {
            // Arrange
            int[] daySo = { 2, 4, 6 };
            string mongDoi = "Day so co 3 phan tu 2 4 6\n" +
                              "* Day so chan\n" +
                              "Day so co 3 so chan: 2 4 6 voi tong la 12, trung binh cong la 4.00.";

            // Act
            string thucTe = Buoi09_DaySoChanLib.ThongKeDaySo(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_ThongKeDaySo_CoSoAm()
        {
            // Arrange: -4 va -2 la so chan am
            int[] daySo = { -4, -3, -2 };
            string mongDoi = "Day so co 3 phan tu -4 -3 -2\n" +
                              "* Day so chan\n" +
                              "Day so co 2 so chan: -4 -2 voi tong la -6, trung binh cong la -3.00.";

            // Act
            string thucTe = Buoi09_DaySoChanLib.ThongKeDaySo(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_ThongKeDaySo_DaySoRong()
        {
            // Arrange
            int[] daySo = { };

            // Act
            string thucTe = Buoi09_DaySoChanLib.ThongKeDaySo(daySo);

            // Assert
            Assert.Equal("Day so rong.", thucTe);
        }
    }
}
