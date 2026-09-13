/*
* CHƯƠNG TRÌNH THÊM CHẴN LẺ
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 12/09/2026
*
* Phát biểu đề bài: Cho mảng n số nguyên (n <= 500). Thêm 1 giá trị lẻ vào giữa 2 phần tử chẵn liên tiếp để tạo thành mảng chẵn, lẻ.
* Ý tưởng:
*   - Dựa vào ví dụ, giá trị lẻ được chèn vào chính là giá trị của phần tử chẵn đứng trước cộng thêm 1 (ví dụ giữa 4 và 2 thì chèn 5, giữa 8 và 2 thì chèn 9).
*   - Sử dụng List<int> để lưu kết quả vì mảng sau khi thêm sẽ bị thay đổi kích thước linh hoạt.
*   - Duyệt mảng ban đầu, đưa từng phần tử vào danh sách kết quả. 
*   - Ngay lúc duyệt, nếu phát hiện phần tử hiện tại và kế tiếp đều chia hết cho 2 (số chẵn), ta chèn thêm giá trị (a[i] + 1) vào danh sách kết quả.
*
* Mã giả:
*   Nhập số nguyên n
*   Nhập dãy n phần tử vào mảng a
*   Khởi tạo danh sách ketQua
*   Cho i chạy từ 0 đến n - 1 thực hiện:
*       Thêm a[i] vào ketQua
*       Nếu (i < n - 1) và (a[i] % 2 == 0) và (a[i+1] % 2 == 0) thì:
*           Thêm (a[i] + 1) vào ketQua
*   In "Mang truoc them:" và các phần tử của mảng a
*   In "Mang sau khi them:" và các phần tử của ketQua
*/

using System;
using System.Collections.Generic;

namespace Buoi11_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("// Nhap day so");
            
            // Nhập n ẩn để bắt đúng logic, hoặc người dùng có thể nhập số phần tử
            Console.Write("Nhap so luong phan tu: ");
            int n = int.Parse(Console.ReadLine()!);

            // Nhập mảng trên cùng 1 dòng
            Console.Write($"Nhap {n} phan tu: ");
            string[] parts = Console.ReadLine()!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < n)
            {
                Console.WriteLine("Chua nhap du so luong phan tu.");
                return;
            }

            int[] a = new int[n];
            for (int i = 0; i < n; i++)
            {
                a[i] = int.Parse(parts[i]);
            }

            // Dùng List để xử lý việc thêm phần tử dễ dàng hơn mảng tĩnh
            List<int> ketQua = new List<int>();

            for (int i = 0; i < n; i++)
            {
                ketQua.Add(a[i]);

                // Xét 2 phần tử kề nhau, nếu cùng chẵn thì thêm số lẻ (a[i] + 1) vào giữa
                if (i < n - 1 && a[i] % 2 == 0 && a[i + 1] % 2 == 0)
                {
                    ketQua.Add(a[i] + 1);
                }
            }

            // In định dạng hiển thị kết quả y hệt yêu cầu đề bài
            Console.WriteLine("* Mang truoc them:");
            Console.WriteLine($"Day so co {n} phan tu: {string.Join(" ", a)}");

            Console.WriteLine("* Mang sau khi them:");
            Console.WriteLine($"Day so co {ketQua.Count} phan tu: {string.Join(" ", ketQua)}");
        }
    }
}