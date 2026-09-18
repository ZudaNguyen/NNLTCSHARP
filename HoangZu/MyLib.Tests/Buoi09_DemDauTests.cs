using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi09_DemDauTests
    {
        // Kiểm tra mảng có đủ cả số âm, số không, số dương
        [Fact]
        public void Test_Dem_DayDuCacLoaiSo_TraVeDung()
        {
            double[] arr = { -2.5, 0, 3.14, -1.0, 0, 5.5, 0 };
            
            // Kỳ vọng: 2 âm, 3 không, 2 dương
            var actual = Buoi09_DemDauLib.Dem(arr);
            Assert.Equal((2, 3, 2), actual);
        }

        // Kiểm tra mảng chỉ toàn số dương
        [Fact]
        public void Test_Dem_ToanSoDuong_TraVeDung()
        {
            double[] arr = { 1, 2, 3, 4, 5 };
            
            // Kỳ vọng: 0 âm, 0 không, 5 dương
            var actual = Buoi09_DemDauLib.Dem(arr);
            Assert.Equal((0, 0, 5), actual);
        }

        // Kiểm tra mảng chỉ toàn số 0
        [Fact]
        public void Test_Dem_ToanSoKhong_TraVeDung()
        {
            double[] arr = { 0, 0, 0, 0 };
            
            // Kỳ vọng: 0 âm, 4 không, 0 dương
            var actual = Buoi09_DemDauLib.Dem(arr);
            Assert.Equal((0, 4, 0), actual);
        }

        // Kiểm tra bắt lỗi: mảng không có phần tử nào (n = 0)
        [Fact]
        public void Test_Dem_SoLuongPhanTuBang0_NemNgoaiLe()
        {
            double[] arr = new double[0];
            Assert.Throws<ArgumentException>(() => Buoi09_DemDauLib.Dem(arr));
        }

        // Kiểm tra bắt lỗi: mảng vượt quá giới hạn 500 phần tử (n = 501)
        [Fact]
        public void Test_Dem_SoLuongPhanTuVuot500_NemNgoaiLe()
        {
            double[] arr = new double[501]; // Khởi tạo mảng có 501 phần tử
            Assert.Throws<ArgumentException>(() => Buoi09_DemDauLib.Dem(arr));
        }
    }
}