using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi11_TronDayTests
    {
        [Fact]
        public void Test_TronHaiDaySoTangDan_ViDuTrongDeBai()
        {
            // Arrange
            int[] a = { 1, 5, 8, 10 };
            int[] b = { 2, 3, 7, 9 };
            int[] mongDoi = { 1, 2, 3, 5, 7, 8, 9, 10 };

            // Act
            int[] thucTe = Buoi11_TronDayLib.TronHaiDaySoTangDan(a, b);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TronHaiDaySoTangDan_CoPhanTuTrungNhau()
        {
            // Arrange
            int[] a = { 1, 3, 3, 5 };
            int[] b = { 2, 3, 4 };
            int[] mongDoi = { 1, 2, 3, 3, 3, 4, 5 };

            // Act
            int[] thucTe = Buoi11_TronDayLib.TronHaiDaySoTangDan(a, b);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TronHaiDaySoTangDan_MotDayRong()
        {
            // Arrange
            int[] a = { };
            int[] b = { 1, 2, 3 };
            int[] mongDoi = { 1, 2, 3 };

            // Act
            int[] thucTe = Buoi11_TronDayLib.TronHaiDaySoTangDan(a, b);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_TronHaiDaySoTangDan_CoSoAm()
        {
            // Arrange
            int[] a = { -5, -1, 3 };
            int[] b = { -3, 0, 2 };
            int[] mongDoi = { -5, -3, -1, 0, 2, 3 };

            // Act
            int[] thucTe = Buoi11_TronDayLib.TronHaiDaySoTangDan(a, b);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_HienThiDaySo_DungDinhDang()
        {
            // Arrange
            int[] daySo = { 1, 5, 8, 10 };
            string mongDoi = "Day so co 4 phan tu: 1 5 8 10";

            // Act
            string thucTe = Buoi11_TronDayLib.HienThiDaySo(daySo);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}
