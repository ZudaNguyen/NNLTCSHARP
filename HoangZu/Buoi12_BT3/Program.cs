/*
* CHƯƠNG TRÌNH KIỂM TRA MA TRẬN TOÀN LẺ
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 12/09/2026
*
* Phát biểu đề bài: Cho ma trận A chứa các số nguyên có n dòng và m cột (1 <= n, m <= 100). Kiểm tra xem ma trận có toàn lẻ không?
* Ý tưởng:
*   - Dùng một cờ (flag) isToanLe = true.
*   - Nhập từng dòng của ma trận. Duyệt qua từng phần tử, nếu phát hiện bất kỳ phần tử nào là số chẵn (chia hết cho 2) thì đổi cờ thành false.
*   - Dựa vào kết quả của cờ để in ra màn hình.
*
* Mã giả:
*   Nhập số dòng n, số cột m
*   Nếu (n và m không nằm trong khoảng 1 đến 100) thì báo lỗi
*   Khởi tạo isToanLe = true
*   Khởi tạo mảng 2 chiều a kích thước n x m
*   Cho i chạy từ 0 đến n - 1 thực hiện:
*       Nhập các phần tử trên dòng i
*       Cho j chạy từ 0 đến m - 1 thực hiện:
*           Nếu (a[i, j] % 2 == 0) thì:
*               isToanLe = false
*   Nếu (isToanLe == true) thì:
*       In "Mang A toan le!"
*   Ngược lại:
*       In "Mang A khong toan le!"
*/

using System;

namespace Buoi12_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("// Nhap A");
            
            // Nhập kích thước ma trận
            Console.Write("Nhap so dong n: ");
            int n = int.Parse(Console.ReadLine()!);
            
            Console.Write("Nhap so cot m: ");
            int m = int.Parse(Console.ReadLine()!);

            // Kiểm tra điều kiện đề bài
            if (n < 1 || n > 100 || m < 1 || m > 100)
            {
                Console.WriteLine("Kich thuoc ma tran khong hop le (1 <= n, m <= 100).");
                return;
            }

            Console.WriteLine($"Mang co {n} dong x {m} cot:");
            
            int[,] a = new int[n, m];
            bool isToanLe = true;

            // Nhập ma trận và kiểm tra tính toàn lẻ
            for (int i = 0; i < n; i++)
            {
                // Đọc cả 1 dòng chứa m số, tự động bỏ qua các khoảng trắng thừa
                string[] parts = Console.ReadLine()!.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                for (int j = 0; j < m; j++)
                {
                    a[i, j] = int.Parse(parts[j]);
                    
                    // Nếu số chia hết cho 2 (kể cả số âm hay dương) thì nó là chẵn
                    if (a[i, j] % 2 == 0)
                    {
                        isToanLe = false;
                    }
                }
            }

            // In kết luận theo định dạng yêu cầu
            if (isToanLe)
            {
                Console.WriteLine("Mang A toan le!");
            }
            else
            {
                Console.WriteLine("Mang A khong toan le!");
            }
        }
    }
}