using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi04_TinhChatSoTests
    {
        [Fact]
        public void Test_PhanTichTinhChatSo_ViDuTrongDeBai()
        {
            // Arrange
            int n = 1221;
            string mongDoi = "1221 la so doi xung.\n1221 khong co 3 chu so giong nhau.\n1221 khong co 4 chu so khac nhau.";

            // Act
            string thucTe = Buoi04_TinhChatSoLib.PhanTichTinhChatSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_PhanTichTinhChatSo_BonChuSoKhacNhau()
        {
            // Arrange: 1234 co 4 chu so khac nhau, khong doi xung, khong co 3 chu so giong nhau
            int n = 1234;
            string mongDoi = "1234 khong la so doi xung.\n1234 khong co 3 chu so giong nhau.\n1234 co 4 chu so khac nhau.";

            // Act
            string thucTe = Buoi04_TinhChatSoLib.PhanTichTinhChatSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_PhanTichTinhChatSo_CoBaChuSoGiongNhau()
        {
            // Arrange: 1112 co 3 chu so 1 giong nhau, khong doi xung, khong du 4 chu so khac nhau
            int n = 1112;
            string mongDoi = "1112 khong la so doi xung.\n1112 co 3 chu so giong nhau.\n1112 khong co 4 chu so khac nhau.";

            // Act
            string thucTe = Buoi04_TinhChatSoLib.PhanTichTinhChatSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_PhanTichTinhChatSo_BonChuSoGiongNhauVaDoiXung()
        {
            // Arrange: 1111 vua doi xung vua co 3 (thuc te 4) chu so giong nhau
            int n = 1111;
            string mongDoi = "1111 la so doi xung.\n1111 co 3 chu so giong nhau.\n1111 khong co 4 chu so khac nhau.";

            // Act
            string thucTe = Buoi04_TinhChatSoLib.PhanTichTinhChatSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(10000)]
        [InlineData(-5)]
        public void Test_PhanTichTinhChatSo_SoKhongCoBonChuSo_TraVeThongBaoLoi(int n)
        {
            // Act
            string thucTe = Buoi04_TinhChatSoLib.PhanTichTinhChatSo(n);

            // Assert
            Assert.Equal("So khong hop le, vui long nhap so tu nhien co 4 chu so.", thucTe);
        }
    }
}
