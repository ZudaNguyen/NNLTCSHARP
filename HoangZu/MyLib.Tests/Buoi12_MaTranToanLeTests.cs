using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi12_MaTranToanLeTests
    {
        // Kiểm tra ma trận chứa toàn số lẻ (bao gồm cả số âm lẻ)
        [Fact]
        public void Test_KiemTraToanLe_MaTranHopLe_TraVeTrue()
        {
            int[,] a = {
                { 1, -3, 5 },
                { 7, 9, 11 }
            };
            Assert.True(Buoi12_MaTranToanLeLib.KiemTraToanLe(a, 2, 3));
        }

        // Kiểm tra ma trận có lẫn số chẵn
        [Fact]
        public void Test_KiemTraToanLe_CoSoChan_TraVeFalse()
        {
            int[,] a = {
                { 1, 3, 5 },
                { 7, 8, 11 } // 8 là số chẵn
            };
            Assert.False(Buoi12_MaTranToanLeLib.KiemTraToanLe(a, 2, 3));
        }

        // Kiểm tra ma trận có chứa số 0 (0 chia hết cho 2 nên được tính là chẵn)
        [Fact]
        public void Test_KiemTraToanLe_CoSoKhong_TraVeFalse()
        {
            int[,] a = {
                { 1, 3 },
                { 0, 7 } 
            };
            Assert.False(Buoi12_MaTranToanLeLib.KiemTraToanLe(a, 2, 2));
        }

        // Kiểm tra chặn lỗi khi nhập số dòng nhỏ hơn 1
        [Fact]
        public void Test_KiemTraToanLe_KichThuocNhoHon1_NemNgoaiLe()
        {
            int[,] a = new int[0, 5];
            Assert.Throws<ArgumentException>(() => Buoi12_MaTranToanLeLib.KiemTraToanLe(a, 0, 5));
        }

        // Kiểm tra chặn lỗi khi nhập số cột lớn hơn 100
        [Fact]
        public void Test_KiemTraToanLe_KichThuocLonHon100_NemNgoaiLe()
        {
            int[,] a = new int[5, 101];
            Assert.Throws<ArgumentException>(() => Buoi12_MaTranToanLeLib.KiemTraToanLe(a, 5, 101));
        }
    }
}