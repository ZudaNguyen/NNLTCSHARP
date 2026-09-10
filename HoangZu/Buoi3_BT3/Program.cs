/*
* CHƯƠNG TRÌNH GIẢI PHƯƠNG TRÌNH BẬC 2
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập 3 số thực a, b, c. Tìm nghiệm của phương trình ax^2 + bx + c = 0.
* Ý tưởng: 
*   - Nếu a = 0: trở thành phương trình bậc nhất bx + c = 0.
*   - Nếu a != 0: tính biệt thức delta = b^2 - 4ac.
*     + delta < 0: vô nghiệm.
*     + delta == 0: nghiệm kép x1 = x2 = -b / (2a).
*     + delta > 0: 2 nghiệm phân biệt x1 = (-b + sqrt(delta)) / (2a), x2 = (-b - sqrt(delta)) / (2a).
* Mã giả:
*   Nhập a, b, c
*   Biện luận theo a và delta
*   In kết quả theo định dạng yêu cầu
*/

using System;
using System.Globalization;

namespace Buoi3_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Moi ban nhap he so a, b, c: ");
            string input = Console.ReadLine()!;
            
            // Tách các giá trị dựa trên khoảng trắng
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
            {
                Console.WriteLine("Vui long nhap du 3 he so cach nhau boi dau cach.");
                return;
            }

            // Dùng InvariantCulture để nhận diện đúng dấu chấm thập phân '.'
            double a = double.Parse(parts[0], CultureInfo.InvariantCulture);
            double b = double.Parse(parts[1], CultureInfo.InvariantCulture);
            double c = double.Parse(parts[2], CultureInfo.InvariantCulture);

            if (a == 0)
            {
                // Biện luận trường hợp suy biến về bậc nhất: bx + c = 0
                if (b == 0)
                {
                    if (c == 0)
                        Console.WriteLine("Phuong trinh co vo so nghiem.");
                    else
                        Console.WriteLine("Phuong trinh vo nghiem.");
                }
                else
                {
                    double x = -c / b;
                    Console.WriteLine($"Phuong trinh co 1 nghiem, x = {x:F2}");
                }
            }
            else
            {
                // Phương trình bậc 2 thực sự
                double delta = b * b - 4 * a * c;

                if (delta < 0)
                {
                    Console.WriteLine($"Phuong trinh bac 2 {a}x^2 + {b}x + {c} = 0 vo nghiem.");
                }
                else if (delta == 0)
                {
                    double x = -b / (2 * a);
                    Console.WriteLine($"Phuong trinh bac 2 {a}x^2 + {b}x + {c} = 0 co: 1 nghiem kep, x1 = x2 = {x:F2}");
                }
                else
                {
                    double sqrtDelta = Math.Sqrt(delta);
                    double x1 = (-b + sqrtDelta) / (2 * a);
                    double x2 = (-b - sqrtDelta) / (2 * a);

                    Console.WriteLine($"Phuong trinh bac 2 {a}x^2 + {b}x + {c} = 0 co: 2   nghiem, x1 = {x1:F2}, x2 = {x2:F2}.");
                }
            }
        }
    }
}
