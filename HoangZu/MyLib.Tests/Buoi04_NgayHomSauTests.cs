using System;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class Buoi04_NgayHomSauTests
    {
        // Kiểm tra các trường hợp ngày tháng hợp lệ
        [Theory]
        [InlineData(15, 9, 2026, 16, 9, 2026)]      // Ngày bình thường ở giữa tháng
        [InlineData(31, 1, 2026, 1, 2, 2026)]       // Đổi tháng (tháng có 31 ngày)
        [InlineData(30, 4, 2026, 1, 5, 2026)]       // Đổi tháng (tháng có 30 ngày)
        [InlineData(28, 2, 2024, 29, 2, 2024)]      // Tháng 2 của năm nhuận (2024)
        [InlineData(28, 2, 2023, 1, 3, 2023)]       // Tháng 2 của năm KHÔNG nhuận (2023)
        [InlineData(31, 12, 2026, 1, 1, 2027)]      // Chuyển năm
        public void Test_TimNgayHomSau_TraVeKetQuaDung(int d, int m, int y, int expD, int expM, int expY)
        {
            var actual = Buoi04_NgayHomSauLib.TimNgayHomSau(d, m, y);
            
            // C# cho phép so sánh thẳng 2 Tuple với nhau
            Assert.Equal((expD, expM, expY), actual);
        }

        // Kiểm tra khả năng bắt lỗi khi đưa vào ngày không tồn tại
        [Theory]
        [InlineData(30, 2, 2024)] // Tháng 2 năm nhuận chỉ có 29 ngày
        [InlineData(29, 2, 2023)] // Tháng 2 năm thường chỉ có 28 ngày
        [InlineData(31, 4, 2026)] // Tháng 4 chỉ có 30 ngày
        [InlineData(15, 13, 2026)] // Tháng 13 không tồn tại
        public void Test_TimNgayHomSau_NgaySai_NemNgoaiLe(int d, int m, int y)
        {
            // Assert.Throws sẽ đánh tick xanh nếu hàm quăng ra lỗi ArgumentOutOfRangeException như dự kiến
            Assert.Throws<ArgumentOutOfRangeException>(() => Buoi04_NgayHomSauLib.TimNgayHomSau(d, m, y));
        }
    }
}