using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "10")]
    public class KeNhauTraiDauTests
    {
        [Theory]
        [InlineData(new double[] { 2, 5, -3, -4, -6, 2, 1 }, 2)]  // Test case khớp với kết quả đề bài
        [InlineData(new double[] { -2, 5, -3, -4, -6, 2, 1 }, 3)] // Test case nếu số đầu là -2
        [InlineData(new double[] { 1, 2, 3, 4, 5 }, 0)]           // Không có số nào trái dấu
        [InlineData(new double[] { -1, 1, -1, 1 }, 3)]            // Trái dấu liên tục đan xen
        [InlineData(new double[] { 0, 5, -5, 0 }, 1)]             // Có số 0 (0 không mang dấu nên 0*x = 0, không bị tính)
        public void KiemTra_DemCapTraiDau(double[] arr, int expectedCount)
        {
            KeNhauTraiDauLib lib = new KeNhauTraiDauLib();
            Assert.Equal(expectedCount, lib.DemCapTraiDau(arr));
        }
    }
}