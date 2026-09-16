using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B8
    {
        [Theory]
        [InlineData(13324, "Chu so 1 xuat hien 1 lan.\nChu so 2 xuat hien 1 lan.\nChu so 3 xuat hien 2 lan.\nChu so 4 xuat hien 1 lan.")]
        [InlineData(505, "Chu so 0 xuat hien 1 lan.\nChu so 5 xuat hien 2 lan.")]
        [InlineData(9999, "Chu so 9 xuat hien 4 lan.")]
        [InlineData(-5, "Vui long nhap so nguyen duong.")]
        [InlineData(0, "Vui long nhap so nguyen duong.")]
        public void Test_ThongKeChuSo_NhieuTruongHop(int n, string mongDoi)
        {
            string thucTe = MyLib.B4B8.ThongKeChuSo(n);

            Assert.Equal(mongDoi, thucTe);
        }
    }
}