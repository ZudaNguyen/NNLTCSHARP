using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B11Tests
    {
        [Fact]
        public void Test_DaoNguocDaySo_MauCuaDeBai()
        {
            // Arrange
            int[] arr = { 2, 4, 3, 4, 2, 8, 2, 5 };
            int[] mongDoi = { 5, 2, 8, 2, 4, 3, 4, 2 };

            // Act
            int[] thucTe = B4B11.DaoNguocDaySo(arr);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_DaoNguocDaySo_DoanMotPhanTu()
        {
            int[] arr = { 9 };
            int[] mongDoi = { 9 };
            Assert.Equal(mongDoi, B4B11.DaoNguocDaySo(arr));
        }

        [Fact]
        public void Test_DaoNguocDaySo_MangRong()
        {
            int[] arr = new int[0];
            int[] mongDoi = new int[0];
            Assert.Equal(mongDoi, B4B11.DaoNguocDaySo(arr));
        }
    }
}