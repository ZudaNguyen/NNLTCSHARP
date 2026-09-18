/*
* CHƯƠNG TRÌNH LIỆT KÊ DÒNG TĂNG DẦN
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 16/09/2026
*
* Phát biểu đề bài: Cho ma trận A (n dòng x m cột). Liệt kê các dòng mà giá trị tạo thành dãy tăng.
* Ý tưởng: 
*  - Duyệt qua từng dòng i của ma trận.
*  - Đặt cờ laDayTang = true. Duyệt cột j từ 1 đến m-1. Nếu A[i, j] <= A[i, j-1] thì gán laDayTang = false và break.
*  - Sau khi kiểm tra xong 1 dòng, nếu laDayTang vẫn true thì in ra dòng đó.
* Mã giả:
*  Begin
*      Input n, m, matrix[n,m]
*      For i = 0 To n - 1 Do
*          isIncreasing = true
*          For j = 1 To m - 1 Do
*              If matrix[i, j] <= matrix[i, j - 1] Then
*                  isIncreasing = false
*                  Break
*              End If
*          End For
*          If isIncreasing Then Print i
*      End For
*  End
*/

using System;

class BT4B12
{
    static void Main()
    {
        Console.WriteLine("// Nhap A");
        Console.Write("Moi nhap so dong n: ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n < 1 || n > 100) return;

        Console.Write("Moi nhap so cot m: ");
        if (!int.TryParse(Console.ReadLine(), out int m) || m < 1 || m > 100) return;

        int[,] maTran = new int[n, m];

        Console.WriteLine($"Mang co {n} dong x {m} cot (Nhap cac phan tu moi dong cach nhau khoang trang):");
        for (int i = 0; i < n; i++)
        {
            string input = Console.ReadLine();
            string[] parts = input?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts == null || parts.Length < m)
            {
                Console.WriteLine($"Vui long nhap du {m} phan tu cho dong {i}.");
                i--; // Lùi lại để nhập lại dòng này
                continue;
            }

            for (int j = 0; j < m; j++)
            {
                int.TryParse(parts[j], out maTran[i, j]);
            }
        }

        // Gọi hàm xử lý từ thư viện
        int[] ketQua = MyLib.B4B12.DongTangDan(maTran);

        if (ketQua.Length > 0)
        {
            Console.WriteLine("Cac dong tao thanh day tang: " + string.Join(" ", ketQua));
        }
        else
        {
            Console.WriteLine("Khong co dong nao tao thanh day tang.");
        }
    }
}