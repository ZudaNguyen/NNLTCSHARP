using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi03_SoNgayThangKeNhuanTests
    {
        [Theory]
        [InlineData(1, 2015, "Thang 1 trong nam 2015 co 31 ngay.")]   // Vi du trong de bai
        [InlineData(4, 2023, "Thang 4 trong nam 2023 co 30 ngay.")]   // Thang co 30 ngay
        [InlineData(2, 2015, "Thang 2 trong nam 2015 co 28 ngay.")]   // Nam khong nhuan
        [InlineData(2, 2016, "Thang 2 trong nam 2016 co 29 ngay.")]   // Nam nhuan (chia het cho 4)
        [InlineData(2, 1900, "Thang 2 trong nam 1900 co 28 ngay.")]   // Chia het cho 100 nhung khong chia het cho 400 -> khong nhuan
        [InlineData(2, 2000, "Thang 2 trong nam 2000 co 29 ngay.")]   // Chia het cho 400 -> nhuan
        [InlineData(13, 2024, "Thang khong hop le.")]                 // Thang khong hop le
        public void Test_TinhSoNgayTrongThang_TraVeKetQuaDung(int thang, int nam, string expected)
        {
            // Act
            string actual = Buoi03_SoNgayThangKeNhuanLib.TinhSoNgayTrongThang(thang, nam);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
