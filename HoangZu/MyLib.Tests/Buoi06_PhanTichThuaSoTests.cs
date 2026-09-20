using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi06_PhanTichThuaSoTests
    {
        // Kiểm tra quá trình phân tích số nguyên hợp lệ
        [Theory]
        [InlineData(2, "2")]                     // Số nguyên tố nhỏ nhất
        [InlineData(12, "2^2 x 3")]              // Có cả số mũ và không mũ
        [InlineData(13, "13")]                   // Số nguyên tố
        [InlineData(60, "2^2 x 3 x 5")]          // Hợp số có 3 thừa số
        [InlineData(100, "2^2 x 5^2")]           // Cả 2 thừa số đều có mũ
        [InlineData(97, "97")]                   // Số nguyên tố lớn
        public void Test_PhanTich_TraVeChuoiDung(int n, string expected)
        {
            string actual = Buoi06_PhanTichThuaSoLib.PhanTich(n);
            Assert.Equal(expected, actual);
        }

        // Kiểm tra khả năng bắt lỗi khi nhập sai dữ liệu
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-5)]
        public void Test_PhanTich_SoNhoHonHoacBang1_NemNgoaiLe(int n)
        {
            // Bắt lỗi ArgumentException đúng như đã thiết lập trong thư viện
            Assert.Throws<ArgumentException>(() => Buoi06_PhanTichThuaSoLib.PhanTich(n));
        }
    }
}