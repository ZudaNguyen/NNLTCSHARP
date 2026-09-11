/*
* CHƯƠNG TRÌNH TÍNH TỔNG PHÂN SỐ (SAI SỐ)
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập số thực a và epsilon (<=0.1). Tính S = 1/a + 1/(a+1) + ... dừng khi 1/(a+n) < epsilon.
* Ý tưởng: 
*   - Sử dụng vòng lặp while.
*   - Ở mỗi bước lặp, kiểm tra phần tử hiện tại có >= epsilon hay không. Nếu có thì cộng dồn vào tổng S và tăng n lên 1.
*   - Vòng lặp tự động dừng khi phần tử hiện tại nhỏ hơn epsilon.
*
* Mã giả:
*   Nhập số thực a và số thực epsilon
*   Khởi tạo S = 0
*   Khởi tạo n = 0
*   Khởi tạo term = 1 / a
*   Trong khi (term >= epsilon) thực hiện:
*       S = S + term
*       n = n + 1
*       term = 1 / (a + n)
*   In ra giá trị của S
*/

using System;
using System.Globalization;

namespace Buoi5_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sử dụng InvariantCulture để đảm bảo đọc đúng dấu chấm thập phân khi nhập số
            Console.Write("Moi ban nhap so a: ");
            double a = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

            Console.Write("Moi ban nhap sai so epsilon (<=0.1): ");
            double epsilon = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

            double S = 0.0;
            int n = 0;
            double term = 1.0 / a;

            // Vòng lặp chạy chừng nào phần tử cộng vào vẫn còn lớn hơn hoặc bằng epsilon
            while (term >= epsilon)
            {
                S += term;
                n++;
                term = 1.0 / (a + n);
            }

            // In kết quả theo đúng định dạng mẫu
            Console.WriteLine($"Gia tri cua bieu thuc S(a = {a}, epsilon = {epsilon}) = {S}.");
        }
    }
}
