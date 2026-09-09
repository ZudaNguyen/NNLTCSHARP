using System;
using Xunit;
using MyLib; // Bắt buộc phải có dòng này để test các hàm trong MyLib

namespace MyLib.Tests
{
    public class HinhTronTests
    {
        [Theory]
        [InlineData(5, 78.5, 31.4)]
        [InlineData(0, 0, 0)]
        [InlineData(2, 12.56, 12.56)]
        public void KiemTra_TinhDienTich_Va_ChuVi(double r, double expectedS, double expectedP)
        {
            HinhTronLib hinhTron = new HinhTronLib();

            double actualS = hinhTron.TinhDienTich(r);
            double actualP = hinhTron.TinhChuVi(r);

            double roundedActualS = Math.Round(actualS, 1);
            double roundedActualP = Math.Round(actualP, 1);
            
            double roundedExpectedS = Math.Round(expectedS, 1);
            double roundedExpectedP = Math.Round(expectedP, 1);

            Assert.Equal(roundedExpectedS, roundedActualS);
            Assert.Equal(roundedExpectedP, roundedActualP);
        }
    }
}