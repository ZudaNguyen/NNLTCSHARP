using System;

namespace MyLib
{
    public class TimeConverterLib
    {
        // logic xử lý đổi giờ, phút, giây từ tổng số giây
        public string ConvertSecondsToTime(int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                throw new ArgumentException("Số giây không hợp lệ");
            }

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            return $"{hours}:{minutes:D2}:{seconds:D2}";
        }
    }
}