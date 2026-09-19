/*
* CHUONG TRINH XAC DINH TINH CHAT CUA SO CO 4 CHU SO
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Cho so tu nhien n co 4 chu so. Hay xac dinh:
*  (a) cach bieu dien n co doi xung khong?
*  (b) n co 3 chu so giong nhau khong?
*  (c) n co 4 chu so khac nhau khong?
* (Luu y: sap xep cac chu so)
*
* Y tuong:
*  - Tach 4 chu so cua n: hang nghin, hang tram, hang chuc, hang don vi.
*  - Doi xung: chu so dau = chu so cuoi va chu so thu 2 = chu so thu 3.
*  - Sap xep 4 chu so, roi duyet de tim so lan lap lai lien tiep lon nhat
*    (>=3 la co 3 chu so giong nhau, khong co lap nao la 4 chu so khac nhau).
*
* Ma gia:
*  Begin
*      Input n
*      Tach chu so d1, d2, d3, d4
*      doiXung = (d1 == d4) and (d2 == d3)
*      Sap xep [d1,d2,d3,d4]
*      Dem so lan lap lai lien tiep toi da
*      coBaGiongNhau = (so lan lap >= 3)
*      coBonKhacNhau = (khong co chu so nao lap lai)
*      Print ket qua (a), (b), (c)
*  End
*/

using System;
using MyLib;

class Buoi4_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so n: ");
        int n = int.Parse(Console.ReadLine()!.Trim());

        string ketQua = Buoi04_TinhChatSoLib.PhanTichTinhChatSo(n);
        Console.WriteLine(ketQua);
    }
}
