/*
* CHƯƠNG TRÌNH KIỂM TRA DÃY ĐAN DẤU
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 11/09/2026
*
* Phát biểu đề bài: Cho dãy số thực có n phần tử (n <= 500). Hãy xác định dãy có tính chất các phần tử kề nhau là trái dấu nhau hay không?
* Ý tưởng:
*   - Nhập n và đọc toàn bộ dãy số trên cùng một dòng.
*   - Khởi tạo một cờ (flag) isDanDau = true.
*   - Duyệt vòng lặp từ đầu đến phần tử kề cuối (n - 1).
*   - Xét tích 2 phần tử kề nhau: Nếu arr[i] * arr[i+1] >= 0 (cùng dấu hoặc có số 0) thì dãy vi phạm tính đan dấu. Đổi cờ thành false và break ngay để tối ưu.
*   - Dựa vào kết quả cờ để in thông báo.
*
* Mã giả:
*   Nhập số nguyên n
*   Nhập dãy n phần tử vào mảng arr
*   isDanDau = true
*   Cho i chạy từ 0 đến n - 2:
*       Nếu (arr[i] * arr[i + 1] >= 0) thì:
*           isDanDau = false
*           Thoát vòng lặp (break)
*   Nếu (isDanDau == true) thì:
*       In "Day so tren la day dan dau."
*   Ngược lại:
*       In "Day so tren khong phai la day dan dau."
*/

using System;
using System.Globalization;

namespace DayDanDau
{
    class Program
    {
        static void Main(string[] args)
        {
            // Nhập n trước để biết độ dài dãy
            Console.Write("Nhap so luong phan tu n: ");
            int n = int.Parse(Console.ReadLine()!);

            if (n <= 0 || n > 500)
            {
                Console.WriteLine("So luong phan tu phai > 0 va <= 500.");
                return;
            }

            // In định dạng giống hệt hình ảnh mẫu
            Console.WriteLine("// Nhap day so");
            Console.Write($"Day so co {n} phan tu: ");
            
            // Đọc toàn bộ các số cách nhau bởi khoảng trắng trên 1 dòng
            string[] parts = Console.ReadLine()!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < n)
            {
                Console.WriteLine("Ban chua nhap du so luong phan tu.");
                return;
            }

            double[] arr = new double[n];
            for (int i = 0; i < n; i++)
            {
                arr[i] = double.Parse(parts[i], CultureInfo.InvariantCulture);
            }

            // Thuật toán kiểm tra đan dấu
            bool isDanDau = true;
            for (int i = 0; i < n - 1; i++)
            {
                // Nếu 2 số liền kề nhân nhau ra số dương (cùng dấu) hoặc bằng 0 (không có dấu) -> Sai
                if (arr[i] * arr[i + 1] >= 0)
                {
                    isDanDau = false;
                    break;
                }
            }

            // In kết luận
            Console.WriteLine("======================================");
            if (isDanDau)
            {
                Console.WriteLine("Day so tren la day dan dau.");
            }
            else
            {
                Console.WriteLine("Day so tren khong phai la day dan dau.");
            }
        }
    }
}