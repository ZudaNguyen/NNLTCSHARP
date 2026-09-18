using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi08_RutGonPhanSoTests
    {
        // Kiểm tra các trường hợp rút gọn thành công
        [Theory]
        [InlineData(12, 18, "2/3")]    // Rút gọn bình thường
        [InlineData(7, 3, "7/3")]      // Phân số đã tối giản sẵn
        [InlineData(4, 2, "2")]        // Rút gọn ra số nguyên (mẫu số = 1)
        [InlineData(2, -3, "-2/3")]    // Mẫu số âm, dấu trừ tự chuyển lên tử
        [InlineData(-4, -6, "2/3")]    // Cả tử và mẫu đều âm (triệt tiêu dấu)
        [InlineData(0, 5, "0")]        // Tử số bằng 0
        public void Test_RutGon_TraVeChuoiDung(int tuSo, int mauSo, string expected)
        {
            string actual = Buoi08_RutGonPhanSoLib.RutGon(tuSo, mauSo);
            Assert.Equal(expected, actual);
        }

        // Kiểm tra khả năng bắt lỗi khi nhập mẫu số = 0
        [Fact]
        public void Test_RutGon_MauSoBang0_NemNgoaiLe()
        {
            // Kiểm tra xem hàm có ném ra lỗi ArgumentException như thiết kế hay không
            Assert.Throws<ArgumentException>(() => Buoi08_RutGonPhanSoLib.RutGon(5, 0));
        }
    }
}