using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B10Tests
    {
        [Fact]
        public void Test_TimDoan_MauCuaDeBai()
        {
            // Trúng khớp với ví dụ của đề
            double[] arr = { -2, 5, -3, 4, -6, 2, -1 };
            double k = 0;
            string mongDoi = "Doan [0, 5] dai nhat co tong bang 0: -2 5 -3 4 -6 2";

            Assert.Equal(mongDoi, B4B10.TimDoan(arr, k));
        }

        [Fact]
        public void Test_TimDoan_KhongTonTai()
        {
            // Trường hợp không có đoạn nào thỏa mãn
            double[] arr = { 1, 2, 3, 4 };
            double k = 100;
            string mongDoi = "Khong tim thay doan nao co tong bang 100.";

            Assert.Equal(mongDoi, B4B10.TimDoan(arr, k));
        }

        [Fact]
        public void Test_TimDoan_DoanMotPhanTu()
        {
            // Trường hợp đoạn dài nhất chỉ có 1 phần tử
            double[] arr = { 1.5, 3.2, 5.0 };
            double k = 3.2;
            string mongDoi = "Doan [1, 1] dai nhat co tong bang 3.2: 3.2";

            Assert.Equal(mongDoi, B4B10.TimDoan(arr, k));
        }
    }
}