/*
* CHƯƠNG TRÌNH TÌM QUÍ CỦA THÁNG
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 13/09/2026
*
* Phát biểu đề bài: Nhập vào tháng. Hãy cho biết tháng đó thuộc quí nào?
* Ý tưởng: 
*  - Một năm có 4 quí, mỗi quí gồm 3 tháng.
*  - Áp dụng công thức toán học: qui = (tháng - 1) / 3 + 1 để tìm ra quí tương ứng.
* Mã giả:
*  Begin
*      Input thang
*      If thang >= 1 and thang <= 12 then
*          qui = (thang - 1) / 3 + 1
*          Print "Thang ", thang, " thuoc qui ", qui, "."
*      Else
*          Print "Thang khong hop le."
*      End If
*  End
*/

using System;

class BT4B2
{
    static void Main()
    {
        Console.Write("Moi ban nhap thang: ");
        
        if (int.TryParse(Console.ReadLine(), out int thang))
        {
            if (thang >= 1 && thang <= 12)
            {
                int qui = (thang - 1) / 3 + 1;
                Console.WriteLine($"Thang {thang} thuoc qui {qui}.");
            }
            else
            {
                Console.WriteLine("Thang khong hop le.");
            }
        }
        else
        {
            Console.WriteLine("Vui long nhap mot so nguyen.");
        }
    }
}