using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "11")]
    public class XoaChanTests
    {
        [Theory]
        [InlineData(new int[] { 2, 4, 3, 4, 2, 8, 2, 5 }, new int[] { 3, 5 })] // Giống ảnh mẫu đề bài
        [InlineData(new int[] { 2, 4, 6, 8, 0 }, new int[] { })]                 // Toàn số chẵn (0 cũng là chẵn) => Mảng rỗng
        [InlineData(new int[] { 1, 3, 5, 7 }, new int[] { 1, 3, 5, 7 })]         // Toàn số lẻ => Giữ nguyên
        [InlineData(new int[] { -2, -3, 4, -5 }, new int[] { -3, -5 })]          // Test với số âm
        public void KiemTra_XoaPhanTuChan(int[] input, int[] expected)
        {
            XoaChanLib lib = new XoaChanLib();
            int[] result = lib.XoaPhanTuChan(input);
            
            // Xunit có thể so sánh trực tiếp 2 mảng xem chúng có các phần tử giống hệt nhau không
            Assert.Equal(expected, result); 
        }
    }
}