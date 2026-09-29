/*
* CHUONG TRINH TIM HAI CHU SO CUOI CUNG CUA n^278
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Nhap vao mot so nguyen n. Hay tinh toan hai chu so cuoi cung cua n^278.
*
* Y tuong:
*  - n^278 la mot so rat lon, khong the tinh truc tiep bang kieu long/double (se bi tran so).
*  - Chi can biet 2 chu so cuoi cung, tuc la can tinh n^278 mod 100.
*  - Dua n ve trong khoang [0, 99] truoc (xu ly ca truong hop n am).
*  - Nhan lien tiep va lay mod 100 sau moi buoc de gia tri luon nho, khong bi tran so.
*
* Ma gia:
*  Begin
*      Input n
*      coSo = ((n mod 100) + 100) mod 100
*      ketQua = 1
*      For i = 1 to 278 Do
*          ketQua = (ketQua * coSo) mod 100
*      End For
*      Print n, "^278 co 2 chu so cuoi cung la", ketQua
*  End
*/

using System;
using MyLib;

class Buoi2_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so nguyen n: ");
        long n = long.Parse(Console.ReadLine()!.Trim());

        string ketQua = Buoi02_HaiChuSoCuoiLib.TinhHaiChuSoCuoi(n);
        Console.WriteLine(ketQua);
    }
}
