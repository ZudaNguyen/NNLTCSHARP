using Xunit;
using MyLib;

namespace MyLib.Tests
{
    [Trait("Buoi", "12")]
    public class DemSoTrenBienMang2CTests
    {
        [Fact]
        public void KiemTra_MaTranDeBai()
        {
            DemSoTrenBienMang2CLib lib = new DemSoTrenBienMang2CLib();
            
            // Khởi tạo ma trận 2 chiều giống trong hình đề bài
            double[,] matrix = new double[,]
            {
                {  1.5, -3.2,  4.8,  0   },
                { -1.8,  3.9,  2.7,  4.5 },
                {  3.3,  9.6, -2.8, -4.9 }
            };

            var result = lib.Dem(matrix);

            // Kiểm tra: 5 dương, 4 âm, 1 không
            Assert.Equal(5, result.pos);
            Assert.Equal(4, result.neg); 
            Assert.Equal(1, result.zero);
        }

        [Fact]
        public void KiemTra_MaTran1x1()
        {
            DemSoTrenBienMang2CLib lib = new DemSoTrenBienMang2CLib();
            double[,] matrix = new double[,] { { -5.5 } }; // Chỉ có 1 phần tử trên biên

            var result = lib.Dem(matrix);

            Assert.Equal(0, result.pos);
            Assert.Equal(1, result.neg);
            Assert.Equal(0, result.zero);
        }
    }
}