/*
* CHƯƠNG TRÌNH ĐỔI SANG GIỜ PHÚT GIÂY
* Tác giả : Nguyen Van Hien Nhan - 3123411207
* Ngày viết: 12/09/2026
*
* Phát biểu đề bài: Một thiết bị hoạt động được t giây. 
Hãy viết chương trình chuyển số giây đó dưới dạng số giờ, số phút và số giây.
* Ý tưởng: 
*  - Một giờ có 3600 giây. Lấy tổng số giây chia cho 3600 để lấy số giờ.
*  - Số giây còn dư sau khi lấy giờ là (t % 3600). Lấy phần này chia nguyên cho 60 để ra số phút.
*  - Số giây lẻ còn lại cuối cùng chính là (t % 60).
* Mã giả:
*  Begin
*      Input t
*      If t >= 0 then
*          h = t / 3600
*          m = (t % 3600) / 60
*          s = t % 60
*          Print h, m, s
*      Else
*          Print "Vui long nhap so giay hop le."
*      End If
*  End
*/

using System;

class BT4B1
{
    static void Main()
    {
        Console.Write("Nhap vao tong so giay: ");
        int t;
        
        // Kiểm tra đầu vào hợp lệ
        if (int.TryParse(Console.ReadLine(), out t) && t >= 0)
        {
            int h = t / 3600;
            int m = (t % 3600) / 60;
            int s = t % 60;

            Console.WriteLine($"{t} giay co dang {h}:{m}:{s}");
        }
        else
        {
            Console.WriteLine("Vui long nhap so giay hop le.");
        }
    }
}