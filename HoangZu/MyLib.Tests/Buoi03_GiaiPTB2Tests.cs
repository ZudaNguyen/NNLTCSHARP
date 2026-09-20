using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi03_GiaiPTB2Tests
    {
        // Kiểm tra nhanh các trường hợp trả về đúng SỐ LƯỢNG nghiệm
        [Theory]
        [InlineData(0, 0, 0, -1)]  // 0x + 0 = 0 -> Vô số nghiệm
        [InlineData(0, 0, 5, 0)]   // 0x + 5 = 0 -> Vô nghiệm
        [InlineData(0, 2, -4, 1)]  // 2x - 4 = 0 -> 1 nghiệm (x = 2)
        [InlineData(1, 2, 5, 0)]   // x^2 + 2x + 5 = 0 -> Vô nghiệm (Delta < 0)
        [InlineData(1, -2, 1, 1)]  // x^2 - 2x + 1 = 0 -> Nghiệm kép (x = 1)
        [InlineData(1, -3, 2, 2)]  // x^2 - 3x + 2 = 0 -> 2 nghiệm phân biệt
        public void Test_GiaiPhuongTrinh_TraVeDungSoNghiem(double a, double b, double c, int expectedSoNghiem)
        {
            // Dùng dấu _ để bỏ qua việc lấy giá trị x1, x2 vì ở hàm này ta chỉ quan tâm số lượng nghiệm
            int actualSoNghiem = Buoi03_GiaiPTB2Lib.GiaiPhuongTrinh(a, b, c, out _, out _);
            Assert.Equal(expectedSoNghiem, actualSoNghiem);
        }

        // Kiểm tra chi tiết GIÁ TRỊ nghiệm khi a = 0 (bậc 1)
        [Fact]
        public void Test_GiaiPhuongTrinh_NghiemBacNhat()
        {
            int soNghiem = Buoi03_GiaiPTB2Lib.GiaiPhuongTrinh(0, 2, -4, out double x1, out double x2);
            Assert.Equal(1, soNghiem);
            Assert.Equal(2.0, x1, 5); // x = 2
        }

        // Kiểm tra chi tiết GIÁ TRỊ nghiệm kép
        [Fact]
        public void Test_GiaiPhuongTrinh_NghiemKep()
        {
            int soNghiem = Buoi03_GiaiPTB2Lib.GiaiPhuongTrinh(1, -2, 1, out double x1, out double x2);
            Assert.Equal(1, soNghiem);
            Assert.Equal(1.0, x1, 5); // x1 = 1
            Assert.Equal(1.0, x2, 5); // x2 = 1
        }

        // Kiểm tra chi tiết GIÁ TRỊ 2 nghiệm phân biệt
        [Fact]
        public void Test_GiaiPhuongTrinh_HaiNghiemPhanBiet()
        {
            int soNghiem = Buoi03_GiaiPTB2Lib.GiaiPhuongTrinh(1, -3, 2, out double x1, out double x2);
            Assert.Equal(2, soNghiem);
            
            // Nghiệm sẽ là 2 và 1, kiểm tra xem x1, x2 có chứa đủ 2 giá trị này không (không quan trọng thứ tự)
            bool coNghiem1 = Math.Abs(x1 - 1.0) < 0.00001 || Math.Abs(x2 - 1.0) < 0.00001;
            bool coNghiem2 = Math.Abs(x1 - 2.0) < 0.00001 || Math.Abs(x2 - 2.0) < 0.00001;
            
            Assert.True(coNghiem1 && coNghiem2);
        }
    }
}