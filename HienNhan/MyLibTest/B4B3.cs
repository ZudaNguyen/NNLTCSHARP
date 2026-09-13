using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B3
    {
        [Theory]
        [InlineData(1, "Tieng anh cua thang 1 la January.")]
        [InlineData(5, "Tieng anh cua thang 5 la May.")]
        [InlineData(12, "Tieng anh cua thang 12 la December.")]
        [InlineData(0, "Thang khong hop le.")]
        [InlineData(13, "Thang khong hop le.")]
        public void Test_ThangTiengAnh_NhieuTruongHop(int thang, string mongDoi)
        {
            string thucTe = MyLib.B4B3.ThangTiengAnh(thang);

            Assert.Equal(mongDoi, thucTe);
        }
    }
}