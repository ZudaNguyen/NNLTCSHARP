using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B12Tests
    {
        [Fact]
        public void Test_DongTangDan_MauCuaDeBai()
        {
            // Arrange
            int[,] maTran = {
                { 1, 7, 9 },
                { 5, 3, 15 },
                { 3, 4, 8 }
            };
            int[] mongDoi = { 0, 2 };

            // Act
            int[] thucTe = B4B12.DongTangDan(maTran);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_DongTangDan_KhongCoDongNao()
        {
            int[,] maTran = {
                { 5, 5, 5 },
                { 9, 8, 7 }
            };
            int[] mongDoi = new int[0]; // Mảng rỗng

            Assert.Equal(mongDoi, B4B12.DongTangDan(maTran));
        }

        [Fact]
        public void Test_DongTangDan_MaTranCoMotCot()
        {
            // Nếu chỉ có 1 phần tử trên mỗi dòng, nó mặc nhiên được tính là dãy tăng
            int[,] maTran = { { 5 }, { 2 }, { 9 } };
            int[] mongDoi = { 0, 1, 2 };

            Assert.Equal(mongDoi, B4B12.DongTangDan(maTran));
        }
    }
}