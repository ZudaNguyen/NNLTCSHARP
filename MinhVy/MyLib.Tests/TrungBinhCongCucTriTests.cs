/*
* KIỂM THỬ TỰ ĐỘNG BÀI TÍNH TỔNG PHẦN TỬ CỰC TRỊ
* Tác giả : Võ Minh Vỹ
* Ngày viết: 18/09/2026
*/

using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class TrungBinhCongCucTriTests
    {
        [Fact]
        public void Test_TinhTongCucTri_DiemCucTri()
        {
            // Dãy mẫu: 5 (cực đại) và 2 (cực đại)
            double[] input = new double[] { -2, 5, -3, -4, -6, 2, 1 };
            
            // Nếu chỉ xét cực đại 5 + 2 = 7
            // Nếu xét cả cực tiểu 5 + (-6) + 2 = 1
            double actual = Buoi10_TrungBinhCongLib.tinhTrungBinhCong(input);

            Assert.True(actual == 7 || actual == 1);
        }
    }
}