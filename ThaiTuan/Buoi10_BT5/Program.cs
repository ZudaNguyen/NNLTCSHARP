/*
* CHUONG TRINH TIM DOAN DUONG DAI NHAT
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Cho day so thuc co n phan tu (n <= 500). Hay xac dinh doan
* co chua toan so duong dai nhat co trong day so.
*
* Y tuong:
*  - Nhap n va n phan tu cua day so (so thuc).
*  - Duyet day so, giu mot doan hien tai gom cac so duong lien tiep.
*  - Khi gap so <= 0 thi doan hien tai bi ngat (do dai ve 0).
*  - Moi khi do dai doan hien tai vuot qua ky luc, cap nhat vi tri dau/cuoi tot nhat.
*  - In doan duong dai nhat tim duoc (neu co nhieu doan bang nhau, lay doan dau tien).
*
* Ma gia:
*  Begin
*      Input n
*      For i = 0 to n-1 Do Input a[i]
*      doDaiHienTai = 0, doDaiToiDa = 0
*      For i = 0 to n-1 Do
*          If a[i] > 0 Then
*              doDaiHienTai = doDaiHienTai + 1
*              If doDaiHienTai > doDaiToiDa Then cap nhat vi tri dau/cuoi tot nhat
*          Else
*              doDaiHienTai = 0
*      End For
*      In doan duong dai nhat
*  End
*/

using System;
using System.Globalization;
using MyLib;

class Buoi10_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so luong phan tu n: ");
        int n = int.Parse(Console.ReadLine()!.Trim());

        if (n <= 0 || n > 500)
        {
            Console.WriteLine("So luong phan tu khong hop le (phai tu 1 den 500).");
            return;
        }

        double[] daySo = new double[n];
        for (int i = 0; i < n; i++)
        {
            Console.Write($"Phan tu {i}: ");
            daySo[i] = double.Parse(Console.ReadLine()!.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        string ketQua = Buoi10_DoanDuongDaiNhatLib.TimDoanDuongDaiNhat(daySo);
        Console.WriteLine(ketQua);
    }
}
