/*
* CHUONG TRINH TRON HAI DAY SO TANG DAN
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Cho so nguyen n, m (n, m <= 500), day n so nguyen a0..a(n-1)
* tang dan va day m so nguyen b0..b(m-1) tang dan. Hay tron 2 day tren thanh day c
* tang dan (khong dung thuat toan sap xep).
*
* Y tuong:
*  - Dung 2 con tro i, j lan luot duyet day A va day B (ca hai da tang dan san).
*  - Moi buoc, so sanh a[i] va b[j], phan tu nao nho hon (hoac bang) thi dua vao
*    day C truoc, roi tang con tro tuong ung len 1.
*  - Khi mot trong hai day da duyet het, chep not phan con lai cua day kia vao C.
*  - Cach nay chi duyet 1 lan qua ca 2 day (O(n+m)), khong can sap xep lai.
*
* Ma gia:
*  Begin
*      Input n, day A (tang dan)
*      Input m, day B (tang dan)
*      i = 0, j = 0, k = 0
*      While i < n And j < m Do
*          If a[i] <= b[j] Then c[k] = a[i], i++
*          Else c[k] = b[j], j++
*          k++
*      End While
*      Chep phan con lai cua A hoac B vao C
*      In day A, day B, day C
*  End
*/

using System;
using MyLib;

class Buoi11_BT5
{
    static void Main()
    {
        Console.WriteLine("* Day A:");
        int[] a = NhapDaySo();
        Console.WriteLine(Buoi11_TronDayLib.HienThiDaySo(a));

        Console.WriteLine("* Day B:");
        int[] b = NhapDaySo();
        Console.WriteLine(Buoi11_TronDayLib.HienThiDaySo(b));

        int[] c = Buoi11_TronDayLib.TronHaiDaySoTangDan(a, b);

        Console.WriteLine();
        Console.WriteLine("* Day C:");
        Console.WriteLine(Buoi11_TronDayLib.HienThiDaySo(c));
    }

    static int[] NhapDaySo()
    {
        Console.Write("Nhap so luong phan tu: ");
        int soLuong = int.Parse(Console.ReadLine()!.Trim());

        int[] daySo = new int[soLuong];
        for (int i = 0; i < soLuong; i++)
        {
            Console.Write($"Phan tu {i}: ");
            daySo[i] = int.Parse(Console.ReadLine()!.Trim());
        }

        return daySo;
    }
}
