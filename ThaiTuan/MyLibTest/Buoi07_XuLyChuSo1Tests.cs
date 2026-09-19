using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi07_XuLyChuSo1Tests
    {
        [Fact]
        public void Test_XuLyChuSo_ViDuTrongDeBai()
        {
            // Arrange
            long n = 182345;
            int k = 2;
            string mongDoi = "Chu so o vi tri 2 (tinh tu trai) cua 182345 la 8.\n" +
                              "Tong cac chu so le cua 182345 la 9.\n" +
                              "182345 co chu so le nho nhat la 1 va lon nhat la 5.";

            // Act
            string thucTe = Buoi07_XuLyChuSo1Lib.XuLyChuSo(n, k);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_XuLyChuSo_ViTriKKhongHopLe()
        {
            // Arrange: 123 chi co 3 chu so, k = 5 la khong hop le
            long n = 123;
            int k = 5;
            string mongDoi = "Vi tri k = 5 khong hop le (so 123 co 3 chu so).\n" +
                              "Tong cac chu so le cua 123 la 4.\n" +
                              "123 co chu so le nho nhat la 1 va lon nhat la 3.";

            // Act
            string thucTe = Buoi07_XuLyChuSo1Lib.XuLyChuSo(n, k);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_XuLyChuSo_KhongCoChuSoLe()
        {
            // Arrange: 2468 toan chu so chan
            long n = 2468;
            int k = 1;
            string mongDoi = "Chu so o vi tri 1 (tinh tu trai) cua 2468 la 2.\n" +
                              "Tong cac chu so le cua 2468 la 0.\n" +
                              "2468 khong co chu so le nao.";

            // Act
            string thucTe = Buoi07_XuLyChuSo1Lib.XuLyChuSo(n, k);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_XuLyChuSo_SoAm()
        {
            // Arrange: so am van xu ly chu so nhu gia tri tuyet doi
            long n = -182345;
            int k = 2;
            string mongDoi = "Chu so o vi tri 2 (tinh tu trai) cua -182345 la 8.\n" +
                              "Tong cac chu so le cua -182345 la 9.\n" +
                              "-182345 co chu so le nho nhat la 1 va lon nhat la 5.";

            // Act
            string thucTe = Buoi07_XuLyChuSo1Lib.XuLyChuSo(n, k);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}
