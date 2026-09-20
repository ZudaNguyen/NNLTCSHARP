using Xunit;
using MyLib;

namespace MyLibTest
{
    public class Buoi08_DocSoTiTests
    {
        [Fact]
        public void Test_DocSo_ViDuTrongDeBai()
        {
            // Arrange
            long n = 15708051006;
            string mongDoi = "So 15,708,051,006 doc thanh: [muoi lam ti] [bay tram le tam trieu] [le nam muoi mot ngan] [le sau]";

            // Act
            string thucTe = Buoi08_DocSoTiLib.DocSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_DocSo_SoDonGian()
        {
            // Arrange: 5 -> chi co nhom don vi, la nhom dau tien nen khong co "le"
            long n = 5;
            string mongDoi = "So 5 doc thanh: [nam]";

            // Act
            string thucTe = Buoi08_DocSoTiLib.DocSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_DocSo_BoQuaNhomBangKhong()
        {
            // Arrange: 1000006 = 1 trieu, 000 ngan (bo qua), 006 don vi
            long n = 1000006;
            string mongDoi = "So 1,000,006 doc thanh: [mot trieu] [le sau]";

            // Act
            string thucTe = Buoi08_DocSoTiLib.DocSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_DocSo_TronChuc()
        {
            // Arrange: 110 -> mot tram muoi
            long n = 110;
            string mongDoi = "So 110 doc thanh: [mot tram muoi]";

            // Act
            string thucTe = Buoi08_DocSoTiLib.DocSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Fact]
        public void Test_DocSo_ToiDa12ChuSo()
        {
            // Arrange: 999,999,999,999 - so lon nhat hop le (12 chu so 9)
            long n = 999999999999;
            string mongDoi = "So 999,999,999,999 doc thanh: [chin tram chin muoi chin ti] [chin tram chin muoi chin trieu] [chin tram chin muoi chin ngan] [chin tram chin muoi chin]";

            // Act
            string thucTe = Buoi08_DocSoTiLib.DocSo(n);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(1000000000000)]
        public void Test_DocSo_SoKhongHopLe(long n)
        {
            // Act
            string thucTe = Buoi08_DocSoTiLib.DocSo(n);

            // Assert
            Assert.Equal("So khong hop le, vui long nhap so nguyen duong co toi da 12 chu so.", thucTe);
        }
    }
}
