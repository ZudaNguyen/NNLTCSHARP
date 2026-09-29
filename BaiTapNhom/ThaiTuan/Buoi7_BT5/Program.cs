/*
* CHUONG TRINH XU LY CHU SO (1)
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Nhap so nguyen n. Hay:
*  (a) cho biet chu so thu k (tinh tu trai sang)
*  (b) tinh tong cac chu so le
*  (c) tim chu so le nho nhat va lon nhat
*
* Y tuong:
*  - Doi n sang chuoi (lay tri tuyet doi) de de truy cap tung chu so theo vi tri.
*  - Chu so thu k chinh la ky tu tai vi tri (k - 1) trong chuoi.
*  - Duyet qua tung chu so, neu la chu so le thi cong don vao tong, dong thoi
*    cap nhat gia tri nho nhat va lon nhat.
*
* Ma gia:
*  Begin
*      Input n, k
*      chuoiSo = chuoi cac chu so cua |n|
*      Neu k hop le Then in chu so tai vi tri k
*      For moi chu so trong chuoiSo Do
*          If chu so le Then cong vao tong, cap nhat min/max
*      End For
*      In tong cac chu so le, chu so le nho nhat va lon nhat
*  End
*/

using System;
using MyLib;

class Buoi7_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so nguyen n: ");
        long n = long.Parse(Console.ReadLine()!.Trim());

        Console.Write("Moi ban nhap vi tri k: ");
        int k = int.Parse(Console.ReadLine()!.Trim());

        string ketQua = Buoi07_XuLyChuSo1Lib.XuLyChuSo(n, k);
        Console.WriteLine(ketQua);
    }
}
