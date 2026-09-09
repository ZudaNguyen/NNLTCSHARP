using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class LuyThuaNhanhTests
    {
        [Theory]
        [InlineData(2, 4, 32, 131072)]   // Test với a = 2
        [InlineData(1, 1, 1, 1)]         // Test với a = 1
        [InlineData(0, 0, 0, 0)]         // Test với a = 0
        public void KiemTra_TinhLuyThuaNhanh(double a, double expectedA2, double expectedA5, double expectedA17)
        {
            LuyThuaNhanhLib lib = new LuyThuaNhanhLib();
            
            // Lấy kết quả từ hàm
            var result = lib.TinhLuyThua(a);

            // So sánh từng giá trị
            Assert.Equal(expectedA2, result.a2);
            Assert.Equal(expectedA5, result.a5);
            Assert.Equal(expectedA17, result.a17);
        }
    }
}