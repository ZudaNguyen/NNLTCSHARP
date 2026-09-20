using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi01_DoiThoiGianTests
    {
        [Theory]
        [InlineData(1, 0, 0, 3600)]    // 1 giờ = 3600 giây
        [InlineData(0, 1, 0, 60)]      // 1 phút = 60 giây
        [InlineData(0, 0, 1, 1)]       // 1 giây = 1 giây
        [InlineData(1, 1, 1, 3661)]    // 1 giờ 1 phút 1 giây
        [InlineData(0, 0, 0, 0)]       // Tất cả đều 0
        [InlineData(2, 30, 45, 9045)]  // Giá trị ngẫu nhiên
        public void Test_DoiSangGiay_TraVeKetQuaDung(int h, int m, int s, long expected)
        {
            // Gọi hàm từ thư viện
            long actual = Buoi01_DoiThoiGianLib.DoiSangGiay(h, m, s);
            
            // So sánh kết quả
            Assert.Equal(expected, actual);
        }
    }
}