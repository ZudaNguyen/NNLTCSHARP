/*
* KIỂM THỬ TỰ ĐỘNG BÀI TÍNH TỔ HỢP C(K, N)
* Tác giả : Võ Minh Vỹ
* Ngày viết: 17/09/2026
*/

using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class GiaiThuaTests
    {
        [Theory]
        [InlineData(5, 10, 252)] // Test case mẫu trong đề bài: C(5, 10) = 252
        [InlineData(0, 5, 1)]    // Test biên k = 0
        [InlineData(5, 5, 1)]    // Test biên k = n
        [InlineData(2, 4, 6)]    // C(2, 4) = 6
        public void Test_TinhToHop(int k, int n, long expected)
        {
            long actual = GiaiThuaLib.TinhToHop(k, n);
            Assert.Equal(expected, actual);
        }
    }
}