/*
* CHUONG TRINH TINH DIEN TICH TAM GIAC
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Cho 3 so thuc a, b, c la do dai 3 canh cua mot tam giac.
* Hay tinh dien tich tam giac nay theo cong thuc Heron:
*   S = can(p(p-a)(p-b)(p-c))  voi  p = (a+b+c)/2
* In ket qua voi 2 so le thap phan.
*
* Y tuong:
*  - Nhap 3 canh a, b, c.
*  - Kiem tra 3 canh co tao thanh tam giac hop le khong (moi canh > 0 va tong 2 canh > canh con lai).
*  - Neu hop le: tinh nua chu vi p, roi tinh dien tich S theo cong thuc Heron.
*  - In S lam tron 2 chu so thap phan.
*
* Ma gia:
*  Begin
*      Input a, b, c
*      If a, b, c khong tao thanh tam giac Then
*          Print "khong hop le"
*      Else
*          p = (a + b + c) / 2
*          S = sqrt(p * (p-a) * (p-b) * (p-c))
*          Print S (2 chu so thap phan)
*      End If
*  End
*/

using System;
using System.Globalization;
using MyLib;

class Buoi1_BT5
{
    static void Main()
    {
        Console.Write("Nhap do dai canh a: ");
        double a = double.Parse(Console.ReadLine()!.Replace(',', '.'), CultureInfo.InvariantCulture);

        Console.Write("Nhap do dai canh b: ");
        double b = double.Parse(Console.ReadLine()!.Replace(',', '.'), CultureInfo.InvariantCulture);

        Console.Write("Nhap do dai canh c: ");
        double c = double.Parse(Console.ReadLine()!.Replace(',', '.'), CultureInfo.InvariantCulture);

        string ketQua = Buoi01_TinhDienTichTamGiacLib.TinhDienTichTamGiac(a, b, c);
        Console.WriteLine(ketQua);
    }
}
