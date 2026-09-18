/*
* THƯ VIỆN TÍNH TỔNG SỐ CHẴN LẺ TRONG MA TRẬN 2 CHIỀU
* Tác giả : Võ Minh Vỹ
* Ngày viết: 18/09/2026
*
* Phát biểu đề bài:
*   Cho ma trận A chứa các số nguyên n dòng x m cột (1 <= n, m <= 100).
*   Tính tổng các số chẵn và tổng các số lẻ có trong ma trận A.
*
* Ý tưởng:
*   Duyệt qua từng dòng r và từng cột c của ma trận 2 chiều matrix[r, c].
*   Nếu giá trị % 2 == 0 thì cộng tích lũy vào tongChan, ngược lại cộng vào tongLe.
*   Trả về tuple (tongChan, tongLe).
*
* Mã giả:
*   Hàm TinhTongChanLe(matrix):
*     tongChan = 0, tongLe = 0
*     Lặp r từ 0 đến n - 1:
*       Lặp c từ 0 đến m - 1:
*         v = matrix[r, c]
*         Nếu v % 2 == 0 -> tongChan = tongChan + v
*         Ngược lại -> tongLe = tongLe + v
*     Trả về (tongChan, tongLe)
*/
using System;
using MyLib;

namespace Buoi12_BT1
{
    public class TongChanLeMang2C
    {
        public static void Main()
        {
            Console.WriteLine("// Nhap A");
            Console.Write("Moi ban nhap so dong n va so cot m: ");
            string[] sizeInput = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int n = int.Parse(sizeInput[0]);
            int m = int.Parse(sizeInput[1]);

            int[,] matrix = new int[n, m];

            Console.WriteLine($"Mang co {n} dong x {m} cot:");
            for (int r = 0; r < n; r++)
            {
                string[] rowInput = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int c = 0; c < m; c++)
                {
                    matrix[r, c] = int.Parse(rowInput[c]);
                }
            }

            // Gọi logic tính toán từ MyLib
            var (tongChan, tongLe) = TongChanLeMang2CLib.TinhTongChanLe(matrix);

            // In theo đúng mẫu đề bài yêu cầu
            Console.WriteLine($"Tong cac so chan la {tongChan} va tong so le la {tongLe}.");
        }
    }
}
