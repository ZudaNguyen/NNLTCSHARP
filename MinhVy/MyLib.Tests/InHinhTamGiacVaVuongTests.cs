/*
* KIỂM THỬ TỰ ĐỘNG BÀI IN 3 HÌNH TAM GIÁC
* Tác giả : Võ Minh Vỹ
* Ngày viết: 17/09/2026
*/

using System.Collections.Generic;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class InHinhTamGiacVaVuongTests
    {
        [Fact]
        public void Test_TaoTamGiacRong_Height3()
        {
            int h = 3;
            List<string> actual = InHinhTamGiacVaVuongLib.TaoTamGiacRong(h);

            Assert.Equal(3, actual.Count);
            Assert.Equal("    *", actual[0]);
            Assert.Equal("  *   *", actual[1]);
            Assert.Equal("* * * * *", actual[2]);
        }
    }
}