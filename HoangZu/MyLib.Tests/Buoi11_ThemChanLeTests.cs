using System;
using System.Collections.Generic;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi11_ThemChanLeTests
    {
        // Kiểm tra mảng có các phần tử chẵn liên tiếp (cần chèn thêm)
        [Fact]
        public void Test_ThemChanLe_CoChanLienTiep_ChenThanhCong()
        {
            int[] arr = { 2, 4, 3, 4, 2, 8, 2, 5 };
            // Giữa 2 và 4 chèn 3; Giữa 4 và 2 chèn 5; Giữa 2 và 8 chèn 3; Giữa 8 và 2 chèn 9.
            List<int> expected = new List<int> { 2, 3, 4, 3, 4, 5, 2, 3, 8, 9, 2, 5 };
            
            List<int> actual = Buoi11_ThemChanLeLib.ThemChanLe(arr);
            Assert.Equal(expected, actual);
        }

        // Kiểm tra mảng không có phần tử chẵn liên tiếp (giữ nguyên)
        [Fact]
        public void Test_ThemChanLe_KhongCoChanLienTiep_GiuNguyen()
        {
            int[] arr = { 2, 3, 4, 5, 6 };
            List<int> expected = new List<int> { 2, 3, 4, 5, 6 };
            
            List<int> actual = Buoi11_ThemChanLeLib.ThemChanLe(arr);
            Assert.Equal(expected, actual);
        }

        // Kiểm tra mảng toàn số lẻ (giữ nguyên)
        [Fact]
        public void Test_ThemChanLe_ToanSoLe_GiuNguyen()
        {
            int[] arr = { 1, 3, 5, 7, 9 };
            List<int> expected = new List<int> { 1, 3, 5, 7, 9 };
            
            List<int> actual = Buoi11_ThemChanLeLib.ThemChanLe(arr);
            Assert.Equal(expected, actual);
        }

        // Kiểm tra lỗi vượt quá 500 phần tử
        [Fact]
        public void Test_ThemChanLe_VuotQua500PhanTu_NemNgoaiLe()
        {
            int[] arr = new int[501];
            Assert.Throws<ArgumentException>(() => Buoi11_ThemChanLeLib.ThemChanLe(arr));
        }
    }
}