using System;

namespace MyLib
{
    public static class Buoi04_NgayHomSauLib
    {
        // Hàm trả về bộ 3 số: (Ngày, Tháng, Năm) của ngày hôm sau
        public static (int Ngay, int Thang, int Nam) TimNgayHomSau(int day, int month, int year)
        {
            DateTime date = new DateTime(year, month, day);
            DateTime nextDate = date.AddDays(1);
            return (nextDate.Day, nextDate.Month, nextDate.Year);
        }
    }
}