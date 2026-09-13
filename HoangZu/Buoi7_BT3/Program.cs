/*
* CHƯƠNG TRÌNH TÍNH SỐ PI
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập vào sai số epsilon. Tính số pi theo chuỗi đan dấu cho đến khi phần tử 4/(2n+1) < epsilon.
* Ý tưởng: 
*   - Sử dụng vòng lặp while với điều kiện dừng là khi giá trị tuyệt đối của phần tử hiện tại (4.0 / (2n + 1)) nhỏ hơn epsilon.
*   - Ở mỗi bước lặp, cộng/trừ phần tử đó vào tổng và đổi dấu cho phần tử tiếp theo.
*   - Sai số thực tế được tính bằng độ lệch giữa số Pi vừa tính và hằng số Math.PI của hệ thống.
*
* Mã giả:
*   Nhập độ sai số epsilon
*   Khởi tạo pi = 0.0
*   Khởi tạo n = 0
*   Khởi tạo term = 4.0 / (2 * n + 1)
*   Khởi tạo sign = 1.0
*   Trong khi (term >= epsilon) thực hiện:
*       pi = pi + sign * term
*       sign = -sign
*       n = n + 1
*       term = 4.0 / (2 * n + 1)
*   saiSoThucTe = Giá trị tuyệt đối của (Số Pi chuẩn - pi)
*   In ra kết quả pi và saiSoThucTe
*/

using System;
using System.Globalization;

namespace Buoi7_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            // Nhập độ sai số epsilon
            Console.Write("Moi ban nhap do sai so epsilon: ");
            // Dùng dấu ! để tránh cảnh báo null và InvariantCulture để nhận diện dấu chấm thập phân
            double epsilon = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

            double pi = 0.0;
            int n = 0;
            double term = 4.0 / (2 * n + 1);
            double sign = 1.0;

            // Vòng lặp chạy chừng nào phần tử hiện tại vẫn còn lớn hơn hoặc bằng epsilon
            while (term >= epsilon)
            {
                // Cộng/trừ phần tử vào pi
                pi += sign * term;
                
                // Đổi dấu cho vòng lặp tiếp theo (1 thành -1, -1 thành 1)
                sign = -sign;
                
                // Tăng n lên và tính giá trị phần tử mới
                n++;
                term = 4.0 / (2 * n + 1);
            }

            // Tính sai số thực tế so với số Pi chuẩn trong thư viện Toán học của C#
            double saiSoThucTe = Math.Abs(Math.PI - pi);

            // In kết quả theo định dạng yêu cầu của đề bài
            Console.WriteLine($"So pi tinh den do chinh xac {epsilon} la {pi}.");
            Console.WriteLine($"Do sai so thuc te la {saiSoThucTe}.");
        }
    }
}