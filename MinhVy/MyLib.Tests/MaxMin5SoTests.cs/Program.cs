/*
* KIỂM THỬ TỰ ĐỘNG BÀI MAX MIN 5 SỐ
* Tác giả : Võ Minh Vỹ
* Ngày viết: 15/09/2026
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