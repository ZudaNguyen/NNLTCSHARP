using System;
using Xunit;
using MyLib; // Bắt buộc phải có dòng này để lấy code từ folder MyLib sang

namespace MyLibTest
{
    public class TimeConverterTests
    {
        [Theory]
        [InlineData(4810, "1:20:10")]
        [InlineData(3600, "1:00:00")]
        [InlineData(65, "0:01:05")]
        [InlineData(0, "0:00:00")]
        public void KiemTra_ChuyenDoiGiay(int inputSeconds, string expectedTime)
        {
            // Khởi tạo đối tượng từ class thư viện
            TimeConverterLib converter = new TimeConverterLib();

            // Gọi hàm tính toán
            string actualTime = converter.ConvertSecondsToTime(inputSeconds);

            // So sánh kết quả thực tế với kết quả mong đợi
            Assert.Equal(expectedTime, actualTime);
        }
    }
}