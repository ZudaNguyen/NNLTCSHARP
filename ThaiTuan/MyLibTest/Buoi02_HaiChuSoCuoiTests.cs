using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi02_HaiChuSoCuoiTests
    {
        [Theory]
        [InlineData(2, "2^278 co 2 chu so cuoi cung la 44.")]     // Vi du trong de bai
        [InlineData(0, "0^278 co 2 chu so cuoi cung la 00.")]     // n = 0
        [InlineData(1, "1^278 co 2 chu so cuoi cung la 01.")]     // n = 1
        [InlineData(5, "5^278 co 2 chu so cuoi cung la 25.")]     // 5^k (k>=2) luon co 2 chu so cuoi la 25
        [InlineData(10, "10^278 co 2 chu so cuoi cung la 00.")]   // Boi cua 10 tu binh phuong tro di co 2 chu so cuoi la 00
        [InlineData(-2, "-2^278 co 2 chu so cuoi cung la 44.")]   // So mu chan nen ket qua giong 2^278
        [InlineData(3, "3^278 co 2 chu so cuoi cung la 89.")]
        public void Test_TinhHaiChuSoCuoi_TraVeKetQuaDung(long n, string expected)
        {
            // Act
            string actual = Buoi02_HaiChuSoCuoiLib.TinhHaiChuSoCuoi(n);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
