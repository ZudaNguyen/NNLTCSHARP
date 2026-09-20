/*
* CHUONG TRINH THONG KE DAY SO CHAN
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Cho day so nguyen co n phan tu (n <= 500). Hay liet ke so luong,
* tong va trung binh cong cac phan tu SO CHAN co trong day so.
*
* Y tuong:
*  - Nhap n va n phan tu cua day so.
*  - In lai day so vua nhap.
*  - Duyet day so, gom cac phan tu chan (chia het cho 2) vao mot danh sach rieng.
*  - Neu co it nhat 1 so chan: tinh tong va trung binh cong (lam tron 2 chu so thap phan).
*  - Neu khong co so chan nao: thong bao rieng.
*
* Ma gia:
*  Begin
*      Input n
*      For i = 0 to n-1 Do Input a[i]
*      In day so
*      dsChan = danh sach rong
*      For moi phan tu x trong day so Do
*          If x chia het cho 2 Then them x vao dsChan
*      End For
*      If dsChan rong Then in "khong co so chan"
*      Else tinh tong, trung binh cong roi in ket qua
*  End
*/

using System;
using MyLib;

class Buoi9_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so luong phan tu: ");
        int n = int.Parse(Console.ReadLine()!.Trim());

        if (n <= 0 || n > 500)
        {
            Console.WriteLine("So luong phan tu khong hop le (phai tu 1 den 500).");
            return;
        }

        int[] daySo = new int[n];
        for (int i = 0; i < n; i++)
        {
            Console.Write($"Phan tu {i}: ");
            daySo[i] = int.Parse(Console.ReadLine()!.Trim());
        }

        string ketQua = Buoi09_DaySoChanLib.ThongKeDaySo(daySo);
        Console.WriteLine(ketQua);
    }
}
