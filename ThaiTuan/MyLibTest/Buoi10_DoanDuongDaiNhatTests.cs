using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi10_DoanDuongDaiNhatTests
    {
        [Fact]
        public void Test_TimDoanDuongDaiNhat_ViDuTrongDeBai()
        {
            // Arrange
            double[] daySo = { -2, 5, 3, 4, -6, 2, -1 };
            string mongDoi = "Day so co 7 phan tu: -2 5 3 4 -6 2 -1\n" +
                              "Doan duong [1, 3] dai nhat: 5 3 4";

            // Act
            string thucTe = Buoi10_DoanDuongDaiNhatLib.TimDoanDuongDaiNhat(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TimDoanDuongDaiNhat_KhongCoSoDuong()
        {
            // Arrange
            double[] daySo = { -1, -2, -3 };
            string mongDoi = "Day so co 3 phan tu: -1 -2 -3\n" +
                              "Khong co doan so duong nao trong day so.";

            // Act
            string thucTe = Buoi10_DoanDuongDaiNhatLib.TimDoanDuongDaiNhat(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TimDoanDuongDaiNhat_TatCaDeuDuong()
        {
            // Arrange
            double[] daySo = { 1, 2, 3 };
            string mongDoi = "Day so co 3 phan tu: 1 2 3\n" +
                              "Doan duong [0, 2] dai nhat: 1 2 3";

            // Act
            string thucTe = Buoi10_DoanDuongDaiNhatLib.TimDoanDuongDaiNhat(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TimDoanDuongDaiNhat_NhieuDoanBangDoDai_LayDoanDauTien()
        {
            // Arrange: co 2 doan duong dai bang nhau (do dai 2): [0,1] va [3,4]
            double[] daySo = { 1, 2, -1, 3, 4 };
            string mongDoi = "Day so co 5 phan tu: 1 2 -1 3 4\n" +
                              "Doan duong [0, 1] dai nhat: 1 2";

            // Act
            string thucTe = Buoi10_DoanDuongDaiNhatLib.TimDoanDuongDaiNhat(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TimDoanDuongDaiNhat_DaySoRong()
        {
            // Arrange
            double[] daySo = { };

            // Act
            string thucTe = Buoi10_DoanDuongDaiNhatLib.TimDoanDuongDaiNhat(daySo);

            // Assert
            Assert.Equal("Day so rong.", thucTe);
        }
    }
}
