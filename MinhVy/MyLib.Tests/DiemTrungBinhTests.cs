/*
* KIỂM THỬ TỰ ĐỘNG BÀI ĐIỂM TRUNG BÌNH
* Tác giả : Võ Minh Vỹ
* Ngày viết: 15/09/2026
*/

using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class DiemTrungBinhTests
    {
        [Theory]
        [InlineData(7, 6, 8, 6.67)]       // Test case mẫu đề bài
        [InlineData(9, 9, 9, 9.00)]      // Test loại Giỏi
        [InlineData(5, 5, 5, 5.00)]      // Test loại Trung bình
        [InlineData(3, 4, 2, 3.33)]      // Test loại Yếu
        public void Test_TinhDTB_Va_XepLoai(double toan, double ly, double hoa, double expectedDTB)
        {
            double actualDTB = Buoi10_TrungBinhCongLib.tinhTrungBinhCong(new[] { toan, ly, hoa });
           

            Assert.Equal(expectedDTB, actualDTB, 2); // Làm tròn 2 chữ số thập phân để so sánh
            
        }
    }
}