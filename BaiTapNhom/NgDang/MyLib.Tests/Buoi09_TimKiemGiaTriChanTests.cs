using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "09")]
    public class TimKiemGiaTriChanTests
    {
        [Fact]
        public void KiemTra_TestDeBai_BoQuaSoAm()
        {
            TimKiemGiaTriChanLib lib = new TimKiemGiaTriChanLib();
            int[] arr = { -2, 5, 8, 9, 16 };
            
            var result = lib.TimChanDauCuoi(arr);
            
            Assert.Equal(2, result.firstIndex);
            Assert.Equal(8, result.firstValue);
            Assert.Equal(4, result.lastIndex);
            Assert.Equal(16, result.lastValue);
        }

        [Fact]
        public void KiemTra_ChiCoMotSoChan()
        {
            TimKiemGiaTriChanLib lib = new TimKiemGiaTriChanLib();
            int[] arr = { 1, 3, 4, 7 }; // Số chẵn duy nhất là 4 ở vị trí 2
            
            var result = lib.TimChanDauCuoi(arr);
            
            // Nếu chỉ có 1 số, nó vừa là đầu tiên, vừa là cuối cùng
            Assert.Equal(2, result.firstIndex);
            Assert.Equal(4, result.firstValue);
            Assert.Equal(2, result.lastIndex);
            Assert.Equal(4, result.lastValue);
        }

        [Fact]
        public void KiemTra_KhongCoSoChanDuong()
        {
            TimKiemGiaTriChanLib lib = new TimKiemGiaTriChanLib();
            int[] arr = { 1, 3, -6, 7 }; // -6 là số chẵn âm, bị bỏ qua
            
            var result = lib.TimChanDauCuoi(arr);
            
            Assert.Equal(-1, result.firstIndex);
            Assert.Equal(-1, result.lastIndex);
        }
    }
}