/*
* CHUONG TRINH LIET KE SO HOAN HAO
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Nhap so nguyen duong n. Hay liet ke cac so hoan hao trong
* doan [1, n] theo thu tu giam dan.
*
* Y tuong:
*  - Duyet tung so i tu 1 den n, kiem tra i co phai la so hoan hao khong
*    (tong cac uoc so thuc su cua i bang chinh i).
*  - Gom cac so hoan hao tim duoc vao danh sach (dang tang dan do duyet tu 1 den n).
*  - Dao nguoc danh sach de duoc thu tu giam dan, roi in ra.
*
* Ma gia:
*  Begin
*      Input n
*      ds = danh sach rong
*      For i = 1 to n Do
*          If i la so hoan hao Then them i vao ds
*      End For
*      Dao nguoc ds
*      Print ds
*  End
*/

using System;
using MyLib;

class Buoi6_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so n: ");
        int n = int.Parse(Console.ReadLine()!.Trim());

        string ketQua = Buoi06_LietKeSoHoanHaoLib.LietKeSoHoanHaoGiamDan(n);
        Console.WriteLine(ketQua);
    }
}
