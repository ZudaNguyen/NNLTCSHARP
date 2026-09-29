/*
* CHUONG TRINH THUC DON TINH DIEN TICH
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Viet chuong trinh hien thi thuc don gom 4 lua chon:
*   1. Tinh dien tich tam giac
*   2. Tinh dien tich hinh chu nhat
*   3. Tinh dien tich hinh tron
*   4. Thoat
* Hien thi ket qua chuc nang nguoi dung chon lua, dung cho nguoi dung nhap phim bat ky
* va xoa man hinh de hien thi lai thuc don. Chuong trinh thoat khi nguoi dung chon Thoat.
*
* Y tuong:
*  - Dung vong lap while(true) de lap lai viec hien thi thuc don.
*  - Doc lua chon cua nguoi dung.
*  - Neu chon 4 thi thoat vong lap (ket thuc chuong trinh) ngay, khong cho nhan phim.
*  - Nguoc lai, in thong bao tuong ung, cho nguoi dung nhan phim bat ky, roi xoa man hinh
*    va lap lai tu dau.
*
* Ma gia:
*  Begin
*      Repeat
*          Hien thi thuc don
*          Input luaChon
*          If luaChon = 4 Then Exit Repeat
*          Print thong bao tuong ung voi luaChon
*          Cho nhan phim bat ky
*          Xoa man hinh
*      Until luaChon = 4
*  End
*/

using System;
using System.IO;
using MyLib;

class Buoi5_BT5
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine(Buoi05_ThucDonLib.HienThiThucDon());
            Console.Write("Moi ban chon chuc nang [1, 2, 3, 4]: ");
            int.TryParse(Console.ReadLine(), out int luaChon);

            Console.WriteLine("-------------------------------------------");

            if (luaChon == 4)
            {
                break;
            }

            Console.WriteLine(Buoi05_ThucDonLib.LayThongBaoChucNang(luaChon));
            Console.WriteLine();
            Console.Write("Ban nhan phim bat ky de tiep tuc ...");

            ChoNhanPhimBatKy();
            XoaManHinh();
        }
    }

    // Doi nguoi dung nhan mot phim bat ky. Neu console khong ho tro doc phim
    // (vi du dau vao bi redirect khi chay tu dong), quay ve doc mot dong Enter.
    static void ChoNhanPhimBatKy()
    {
        try
        {
            Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            Console.ReadLine();
        }
    }

    // Xoa man hinh console. Bo qua neu dau ra dang bi redirect (khong co console that).
    static void XoaManHinh()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
        }
    }
}
