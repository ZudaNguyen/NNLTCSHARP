/*
* KIỂM THỬ TỰ ĐỘNG BÀI TỔNG CHẴN LẺ MẢNG 2 CHIỀU
* Tác giả : Võ Minh Vỹ
* Ngày viết: 18/09/2026
*/

using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class TongChanLeMang2CTests
    {
        [Fact]
        public void Test_TinhTongChanLe_MauDeBai()
        {
            int[,] matrix = new int[,]
            {
                { 2, 4, 5 },
                { 8, 10, 15 }
            };

            var (actualChan, actualLe) = TongChanLeMang2CLib.TinhTongChanLe(matrix);

            Assert.Equal(24, actualChan);
            Assert.Equal(20, actualLe);
        }
    }
}