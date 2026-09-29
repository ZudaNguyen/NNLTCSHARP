using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi06_LietKeSoHoanHaoTests
    {
        [Fact]
        public void Test_LietKeSoHoanHaoGiamDan_ViDuTrongDeBai()
        {
            // Arrange
            int n = 10;
            string mongDoi = "Cac so hoan hao trong doan [1, 10]: 6";

            // Act
            string thucTe = Buoi06_LietKeSoHoanHaoLib.LietKeSoHoanHaoGiamDan(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_LietKeSoHoanHaoGiamDan_NhieuSoHoanHao_SapXepGiamDan()
        {
            // Arrange: co 2 so hoan hao trong [1, 28] la 6 va 28
            int n = 28;
            string mongDoi = "Cac so hoan hao trong doan [1, 28]: 28 6";

            // Act
            string thucTe = Buoi06_LietKeSoHoanHaoLib.LietKeSoHoanHaoGiamDan(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_LietKeSoHoanHaoGiamDan_BaSoHoanHao()
        {
            // Arrange: co 3 so hoan hao trong [1, 496] la 6, 28, 496
            int n = 496;
            string mongDoi = "Cac so hoan hao trong doan [1, 496]: 496 28 6";

            // Act
            string thucTe = Buoi06_LietKeSoHoanHaoLib.LietKeSoHoanHaoGiamDan(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_LietKeSoHoanHaoGiamDan_KhongCoSoHoanHao()
        {
            // Arrange: [1, 5] khong co so hoan hao nao (6 la so hoan hao nho nhat)
            int n = 5;
            string mongDoi = "Khong co so hoan hao nao trong doan [1, 5].";

            // Act
            string thucTe = Buoi06_LietKeSoHoanHaoLib.LietKeSoHoanHaoGiamDan(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Test_LietKeSoHoanHaoGiamDan_NKhongHopLe(int n)
        {
            // Act
            string thucTe = Buoi06_LietKeSoHoanHaoLib.LietKeSoHoanHaoGiamDan(n);

            // Assert
            Assert.Equal("So n khong hop le, vui long nhap so nguyen duong.", thucTe);
        }
    }
}
