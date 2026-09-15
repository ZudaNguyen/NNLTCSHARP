using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B7Tests
    {
        [Theory]
        [InlineData(1, 2, 2015, "Ngay truoc ngay 1/2/2015 la ngay 31/1/2015.")]
        [InlineData(1, 1, 2024, "Ngay truoc ngay 1/1/2024 la ngay 31/12/2023.")]
        [InlineData(15, 5, 2026, "Ngay truoc ngay 15/5/2026 la ngay 14/5/2026.")] 
        [InlineData(1, 3, 2024, "Ngay truoc ngay 1/3/2024 la ngay 29/2/2024.")] 
        [InlineData(1, 3, 2023, "Ngay truoc ngay 1/3/2023 la ngay 28/2/2023.")] // Đầu tháng 3 năm không nhuận
        [InlineData(32, 1, 2023, "Ngày không hợp lệ")] // Ngày sai
        public void Test_NgayTruoc_NhieuTruongHop(int d, int m, int y, string mongDoi)
        {
            // Act
            string thucTe = B4B7.NgayTruoc(d, m, y);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}