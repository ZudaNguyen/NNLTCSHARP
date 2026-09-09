/*
* CHƯƠNG TRÌNH ĐỔI THỜI GIAN SANG GIÂY
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 8/9/2026
*
* Phát biểu đề bài: Chuyển đổi thời gian gồm h (giờ), m (phút), s (giây) sang tổng số giây.
* Ý tưởng: Áp dụng công thức quy đổi: Tổng giây = (h * 3600) + (m * 60) + s.
* Mã giả:
*   Nhập h, m, s
*   tong_giay = h * 3600 + m * 60 + s
*   In kết quả tong_giay
*/
using System;

namespace Baitap3
{
    class Program
    {
        static void Main(string[] args)
        {
            // Nhập số giờ
            Console.Write("Nhap so gio: ");
            int h = int.Parse(Console.ReadLine());

            // Nhập số phút
            Console.Write("Nhap so phut: ");
            int m = int.Parse(Console.ReadLine());

            // Nhập số giây
            Console.Write("Nhap so giay: ");
            int s = int.Parse(Console.ReadLine());

            // Tính tổng số giây
            // Sử dụng kiểu long và thêm hậu tố 'L' để tránh tràn số nếu số giờ nhập vào quá lớn
            long tong_giay = h * 3600L + m * 60L + s;

            // In ra kết quả sử dụng String Interpolation ($"...")
            Console.WriteLine($"Tong so giay cua {h}:{m}:{s} la {tong_giay} giay");
        }
    }
}
