using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "07")]
    public class KiemTraSoHoanHaoTests
    {
        [Theory]
        [InlineData(6, true)]   // 6 = 1 + 2 + 3
        [InlineData(28, true)]  // 28 = 1 + 2 + 4 + 7 + 14
        [InlineData(8, false)]  // 8 = 1 + 2 + 4 = 7 (Khác 8)
        [InlineData(12, false)] // 12 = 1 + 2 + 3 + 4 + 6 = 16 (Khác 12)
        [InlineData(1, false)]  // Số 1 không được tính là số hoàn hảo
        [InlineData(-6, false)] // Số âm không hợp lệ
        public void KiemTra_SoHoanHao(int n, bool expectedIsPerfect)
        {
            KiemTraSoHoanHaoLib lib = new KiemTraSoHoanHaoLib();
            
            // Gọi hàm và chỉ lấy ra biến IsPerfect để so sánh
            var result = lib.KiemTra(n);
            
            Assert.Equal(expectedIsPerfect, result.IsPerfect);
        }
    }
}