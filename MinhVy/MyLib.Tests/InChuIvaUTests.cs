/*
* KIỂM THỬ TỰ ĐỘNG BÀI IN CHỮ I VÀ CHỮ U
* Tác giả : Võ Minh Vỹ
* Ngày viết: 16/09/2026
*/

using System.Collections.Generic;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class InChuIvaUTests
    {
        [Fact]
        public void Test_TaoHinhChuIVaU_Height5()
        {
            int h = 5;
            List<string> actual = InChuIvaULib.TaoHinhChuIVaU(h);

            Assert.Equal(5, actual.Count);
            Assert.Equal("* * * * *   *       *", actual[0]);
            Assert.Equal("    *       *       *", actual[1]);
            Assert.Equal("    *       *       *", actual[2]);
            Assert.Equal("    *       *       *", actual[3]);
            Assert.Equal("* * * * *   * * * * *", actual[4]);
        }
    }
}