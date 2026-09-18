using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi05_TongSaiSoTests
    {
        [Theory]
        // n=0 (1/2=0.5), n=1 (1/3=0.333...), n=2 (1/4=0.25). Tổng = 1.08333...
        [InlineData(2, 0.25, 1.08333)] 
        
        // n=0 (1/1=1), n=1 (1/2=0.5). Tổng = 1.5
        [InlineData(1, 0.5, 1.5)]      
        
        // n=0 (1/10=0.1). Tổng = 0.1. Đến n=1 (1/11 < 0.1) thì dừng
        [InlineData(10, 0.1, 0.1)]     
        
        // Ngay từ term đầu tiên (1/5 = 0.2) đã nhỏ hơn epsilon (0.5), vòng lặp không chạy
        [InlineData(5, 0.5, 0.0)]      
        public void Test_TinhTongSaiSo_TraVeKetQuaDung(double a, double epsilon, double expected)
        {
            double actual = Buoi05_TongSaiSoLib.TinhTongSaiSo(a, epsilon);
            
            // So sánh kết quả, cho phép chênh lệch tối đa ở 5 chữ số thập phân
            Assert.Equal(expected, actual, 5); 
        }
    }
}