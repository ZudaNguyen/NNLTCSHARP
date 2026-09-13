/*
* CHƯƠNG TRÌNH TÌM NGÀY HÔM SAU (Sử dụng DateTime)
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập vào ngày, tháng, năm. Tìm ngày liền sau đó.
* Ý tưởng:
*   - Sử dụng cấu trúc DateTime có sẵn của C# để quản lý thời gian.
*   - Dùng phương thức AddDays(1) để tự động tính toán ngày hôm sau mà không cần bận tâm đến năm nhuận hay số ngày tối đa của tháng.
*/

using System;

namespace Buoi4_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Moi ban nhap ngay, thang, nam: ");
            string[] parts = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
            {
                Console.WriteLine("Vui long nhap du ngay, thang, nam cach nhau boi dau cach.");
                return;
            }

            int day = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int year = int.Parse(parts[2]);

            try
            {
                // Khởi tạo đối tượng ngày tháng
                DateTime date = new DateTime(year, month, day);
                
                // Tự động tính ngày hôm sau
                DateTime nextDate = date.AddDays(1);

                // In kết quả
                Console.WriteLine($"Ngay sau ngay {day}/{month}/{year} la ngay {nextDate.Day}/{nextDate.Month}/{nextDate.Year}.");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Bắt lỗi nếu người dùng nhập ngày không tồn tại (ví dụ 30/2/2024)
                Console.WriteLine("Ngay thang nam nhap vao khong hop le.");
            }
        }
    }
}