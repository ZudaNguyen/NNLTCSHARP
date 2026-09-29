using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi12_CotTongLonNhatTests
    {
        [Fact]
        public void Test_HienThiKetQua_ViDuTrongDeBai()
        {
            // Arrange
            double[,] matran =
            {
                { 1, 11, 9 },
                { 5, 5, 15 },
                { 3, 16, 8 }
            };
            string mongDoi = "Cac cot co tong lon nhat: 1 2";

            // Act
            string thucTe = Buoi12_CotTongLonNhatLib.HienThiKetQua(matran);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_LietKeCotTongLonNhat_ChiMotCotDuyNhat()
        {
            // Arrange: cot0 = 1+3=4, cot1 = 2+4=6 -> chi cot 1 lon nhat
            double[,] matran =
            {
                { 1, 2 },
                { 3, 4 }
            };
            int[] mongDoi = { 1 };

            // Act
            int[] thucTe = Buoi12_CotTongLonNhatLib.LietKeCotTongLonNhat(matran);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_LietKeCotTongLonNhat_TatCaCacCotBangNhau()
        {
            // Arrange: cot0 = cot1 = 2
            double[,] matran =
            {
                { 1, 1 },
                { 1, 1 }
            };
            int[] mongDoi = { 0, 1 };

            // Act
            int[] thucTe = Buoi12_CotTongLonNhatLib.LietKeCotTongLonNhat(matran);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_LietKeCotTongLonNhat_ChiMotDong()
        {
            // Arrange: n = 1 dong, cot 1 va 2 co gia tri lon nhat bang nhau
            double[,] matran =
            {
                { 5, 10, 10 }
            };
            int[] mongDoi = { 1, 2 };

            // Act
            int[] thucTe = Buoi12_CotTongLonNhatLib.LietKeCotTongLonNhat(matran);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}
