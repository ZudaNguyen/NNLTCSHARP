/*
* CHUONG TRINH LIET KE COT CO TONG LON NHAT
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Cho ma tran A chua cac so thuc co n dong va m cot (1 <= n, m <= 100).
* Hay liet ke cac cot co tong cac gia tri lon nhat.
*
* Y tuong:
*  - Nhap n, m va tung phan tu cua ma tran.
*  - Tinh tong tung cot bang cach cong don cac phan tu theo cung chi so cot, duyet qua tat ca dong.
*  - Tim gia tri tong lon nhat trong cac tong cot.
*  - Liet ke tat ca cac chi so cot co tong bang voi tong lon nhat do (co the co nhieu cot).
*
* Ma gia:
*  Begin
*      Input n, m, ma tran A
*      For j = 0 to m-1 Do
*          tongCot[j] = tong cac A[i, j] voi i = 0..n-1
*      End For
*      tongLonNhat = gia tri lon nhat trong tongCot
*      For j = 0 to m-1 Do
*          If tongCot[j] == tongLonNhat Then in chi so cot j
*      End For
*  End
*/

using System;
using System.Globalization;
using MyLib;

class Buoi12_BT5
{
    static void Main()
    {
        Console.Write("Nhap so dong n: ");
        int n = int.Parse(Console.ReadLine()!.Trim());

        Console.Write("Nhap so cot m: ");
        int m = int.Parse(Console.ReadLine()!.Trim());

        double[,] matran = new double[n, m];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                Console.Write($"Phan tu [{i},{j}]: ");
                matran[i, j] = double.Parse(Console.ReadLine()!.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);
            }
        }

        Console.WriteLine($"Mang co {n} dong x {m} cot:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                Console.Write($"{matran[i, j],5}");
            }
            Console.WriteLine();
        }

        Console.WriteLine(Buoi12_CotTongLonNhatLib.HienThiKetQua(matran));
    }
}
