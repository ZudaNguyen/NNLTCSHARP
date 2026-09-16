/*
* THƯ VIỆN TÌM MAX MIN CỦA 5 SỐ
* Tác giả : Võ Minh Vỹ
* Ngày viết: 15/09/2026
*
* Phát biểu đề bài:
*   Viết các phương thức nhận vào 5 số thực a, b, c, d, e 
*   và trả về giá trị lớn nhất (Max), nhỏ nhất (Min).
*
* Ý tưởng:
*   Sử dụng hàm Math.Max và Math.Min lồng nhau để so sánh 5 số mà không làm thay đổi biến gốc.
*
* Mã giả:
*   Hàm TimMax(a, b, c, d, e):
*     Trả về Math.Max(a, Math.Max(b, Math.Max(c, Math.Max(d, e))))
*   Hàm TimMin(a, b, c, d, e):
*     Trả về Math.Min(a, Math.Min(b, Math.Min(c, Math.Min(d, e))))
*/

using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class MaxMin5SoTests
    {
        [Theory]
        [InlineData(1, 5, 4, 2, 8, 8, 1)]        // Test case trong đề bài: Max = 8, Min = 1
        [InlineData(-10, -5, 0, 100, 50, 100, -10)] // Test số âm và số lớn
        [InlineData(7, 7, 7, 7, 7, 7, 7)]           // Test 5 số bằng nhau
        public void Test_TimMaxMin(double a, double b, double c, double d, double e, double expectedMax, double expectedMin)
        {
            double actualMax = MaxMin5SoLib.TimMax(a, b, c, d, e);
            double actualMin = MaxMin5SoLib.TimMin(a, b, c, d, e);

            Assert.Equal(expectedMax, actualMax);
            Assert.Equal(expectedMin, actualMin);
        }
    }
}