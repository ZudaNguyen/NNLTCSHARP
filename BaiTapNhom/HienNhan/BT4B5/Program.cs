/*
* CHƯƠNG TRÌNH TÌM SỐ NGUYÊN DƯƠNG N BÉ NHẤT
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 15/09/2026
*
* Phát biểu đề bài: Nhập vào số thực a. Hãy tìm số nguyên dương n bé nhất thỏa: 1 + 1/2 + 1/3 + ... + 1/n > a
* Ý tưởng: 
*  - Khởi tạo tổng S = 0 và n = 0.
*  - Sử dụng vòng lặp while, điều kiện lặp là S <= a.
*  - Trong mỗi bước lặp, tăng n lên 1 đơn vị và cộng thêm 1/n vào tổng S.
*  - Vòng lặp kết thúc khi S > a, giá trị n lúc này chính là kết quả cần tìm.
* Mã giả:
*  Begin
*      Input a
*      S = 0
*      n = 0
*      While S <= a do
*          n = n + 1
*          S = S + 1.0 / n
*      End While
*      Print n
*  End
*/

using System;

class BT4B5
{
    static void Main()
    {
        Console.Write("Moi ban nhap vao so thuc a: ");
        
        if (double.TryParse(Console.ReadLine(), out double a))
        {
            // Gọi hàm xử lý từ thư viện
            int n = MyLib.B4B5.TimBeNhatThoa(a);
            Console.WriteLine($"So nguyen duong n be nhat thoa dieu kien la: {n}");
        }
        else
        {
            Console.WriteLine("Vui long nhap mot so thuc hop le.");
        }
    }
}