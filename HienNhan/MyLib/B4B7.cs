namespace MyLib
{
    public class B4B7
    {
        public static String NgayTruoc(int day, int month, int year)
        {
            if (year < 1 || month < 1 || month > 12 || day < 1 || day > 31)
            {
                return "Ngày không hợp lệ";
            }

            int prevDay = day;
            int prevMonth = month;  
            int prevYear = year;

            if (day > 1)
            {
                prevDay = day - 1;
            }
            else
            {
                if (month == 1)
                {
                    prevDay = 31;
                    prevMonth = 12;
                    prevYear = year - 1;
                }
                else{
                   prevMonth = month - 1;
                   prevDay = SoNgayTrongThang(prevMonth, year); 
                }
            }

            return $"Ngay truoc ngay {day}/{month}/{year} la ngay {prevDay}/{prevMonth}/{prevYear}.";  
                  }
            private static int SoNgayTrongThang(int month, int year)
        {
            switch (month)
            {
                case 4: case 6: case 9: case 11:
                    return 30;
                case 2:
                    // Kiểm tra năm nhuận
                    bool laNamNhuan = (year % 400 == 0) || (year % 4 == 0 && year % 100 != 0);
                    return laNamNhuan ? 29 : 28;
                default:
                    return 31;
            }
        }


    }


}