using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "06")]
    public class XuLyChuSo1Tests
    {
        [Theory]
        [InlineData(182345, 2, 8)] // Test case trong đề bài
        [InlineData(182345, 1, 1)] // Kí tự đầu tiên
        [InlineData(182345, 6, 5)] // Kí tự cuối cùng
        [InlineData(182345, 10, -1)] // Vị trí không tồn tại
        public void KiemTra_TimChuSoThuK(int n, int k, int expected)
        {
            XuLyChuSo1Lib lib = new XuLyChuSo1Lib();
            Assert.Equal(expected, lib.TimChuSoThuK(n, k));
        }

        [Theory]
        [InlineData(182345, 9)] // 1 + 3 + 5 = 9 (Test case đề bài)
        [InlineData(2468, 0)]   // Không có số lẻ
        [InlineData(1357, 16)]  // Toàn số lẻ: 1+3+5+7 = 16
        public void KiemTra_TongChuSoLe(int n, int expected)
        {
            XuLyChuSo1Lib lib = new XuLyChuSo1Lib();
            Assert.Equal(expected, lib.TongChuSoLe(n));
        }

        [Theory]
        [InlineData(182345, 1, 5)] // Test case đề bài
        [InlineData(7893, 3, 9)]   // Nhiều số lẻ ngẫu nhiên
        [InlineData(2468, -1, -1)] // Không có số lẻ nào
        public void KiemTra_TimChuSoLeMinMax(int n, int expectedMin, int expectedMax)
        {
            XuLyChuSo1Lib lib = new XuLyChuSo1Lib();
            var result = lib.TimChuSoLeMinMax(n);
            
            Assert.Equal(expectedMin, result.min);
            Assert.Equal(expectedMax, result.max);
        }
    }
}