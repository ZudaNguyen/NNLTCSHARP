using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi07_TinhPiTests
    {
        [Theory]
        // epsilon = 1.0 => dừng ở n = 2 vì term = 4/5 = 0.8 < 1.0
        // Tổng: 4/1 - 4/3 = 8/3 ~ 2.66666...
        [InlineData(1.0, 2.66667)]

        // epsilon = 0.5 => dừng ở n = 4 vì term = 4/9 = 0.444... < 0.5
        // Tổng: 4/1 - 4/3 + 4/5 - 4/7 = 304/105 ~ 2.89524
        [InlineData(0.5, 2.89524)]
        public void Test_TinhPi_EpsilonLon_VongLapDungChinhXac(double epsilon, double expected)
        {
            double actual = Buoi07_TinhPiLib.TinhPi(epsilon);
            
            // So sánh với độ lệch tối đa 5 chữ số thập phân
            Assert.Equal(expected, actual, 5);
        }

        [Fact]
        public void Test_TinhPi_EpsilonNho_GiaTriHoiTuGanSoPiChuan()
        {
            double epsilon = 0.0001;
            double actualPi = Buoi07_TinhPiLib.TinhPi(epsilon);
            
            // Độ lệch giữa số Pi vừa tính và hằng số chuẩn
            double saiSoThucTe = Math.Abs(Math.PI - actualPi);
            
            // Theo nguyên lý chuỗi Leibniz, sai số thực tế sẽ gần xấp xỉ với epsilon
            Assert.True(saiSoThucTe < epsilon * 1.5, $"Sai số thực tế ({saiSoThucTe}) vượt quá ngưỡng cho phép so với epsilon.");
        }
    }
}