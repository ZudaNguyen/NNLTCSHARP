using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi10_DayDanDauTests
    {
        // Kiểm tra dãy đan dấu hợp lệ
        [Fact]
        public void Test_KiemTraDanDau_DayHopLe_TraVeTrue()
        {
            double[] arr = { -2, 5, -3, 4, -6, 2, -1 };
            bool result = Buoi10_DayDanDauLib.KiemTraDanDau(arr);
            Assert.True(result);
        }

        // Kiểm tra dãy bị hỏng do có 2 số dương liền kề
        [Fact]
        public void Test_KiemTraDanDau_HaiSoDuongLienTiep_TraVeFalse()
        {
            double[] arr = { -2, 5, 3, -4 };
            bool result = Buoi10_DayDanDauLib.KiemTraDanDau(arr);
            Assert.False(result);
        }

        // Kiểm tra dãy bị hỏng do có 2 số âm liền kề
        [Fact]
        public void Test_KiemTraDanDau_HaiSoAmLienTiep_TraVeFalse()
        {
            double[] arr = { 2, -5, -3, 4 };
            bool result = Buoi10_DayDanDauLib.KiemTraDanDau(arr);
            Assert.False(result);
        }

        // Kiểm tra dãy bị hỏng do chứa số 0 (số 0 không mang dấu)
        [Fact]
        public void Test_KiemTraDanDau_ChuaSoKhong_TraVeFalse()
        {
            double[] arr = { -2, 0, 3 };
            bool result = Buoi10_DayDanDauLib.KiemTraDanDau(arr);
            Assert.False(result);
        }

        // Kiểm tra bắt lỗi: mảng vượt quá giới hạn 500 phần tử
        [Fact]
        public void Test_KiemTraDanDau_VuotQuaGioiHan_NemNgoaiLe()
        {
            double[] arr = new double[501];
            Assert.Throws<ArgumentException>(() => Buoi10_DayDanDauLib.KiemTraDanDau(arr));
        }
    }
}