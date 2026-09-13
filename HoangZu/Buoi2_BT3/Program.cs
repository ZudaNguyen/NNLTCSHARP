/*
* CHƯƠNG TRÌNH TÍNH TOÁN BIỂU THỨC
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 09/09/2026
*
* Phát biểu đề bài: Cho số thực x. Tính giá trị của biểu thức f(x) = 1 + 2x + 3x^2 - 4x^3 chỉ dùng phép cộng, trừ, nhân và không quá 8 phép toán.
* Ý tưởng: Áp dụng lược đồ Horner để nhóm x làm nhân tử chung. Rút gọn công thức thành f(x) = 1 + x * (2 + x * (3 - 4 * x)) để tối ưu số lượng tính toán xuống chỉ còn 6 phép toán.
* Mã giả:
*   Nhập biến x kiểu số thực
*   ket_qua = 1 + x * (2 + x * (3 - 4 * x))
*   In kết quả
*/
using System;

namespace BT3_Buoi2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Moi ban nhap so thuc x: ");
            
            // Sử dụng double.TryParse để đảm bảo an toàn nếu người dùng nhập lỗi
            if (double.TryParse(Console.ReadLine(), out double x))
            {
                // Tính toán dựa trên lược đồ Horner (6 phép toán)
                double result = 1 + x * (2 + x * (3 - 4 * x));

                // In kết quả
                Console.WriteLine($"f({x}) = {result}");
            }
            else
            {
                Console.WriteLine("Du lieu nhap khong phai la so thuc hop le.");
            }
        }
    }
}
