/*
* CHƯƠNG TRÌNH KIỂM TRA SỐ HOÀN HẢO
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 15/09/2026
*
* Phát biểu đề bài: Số tự nhiên n gọi là số hoàn hảo nếu tổng các ước tự nhiên của nó không kể chính nó bằng n. Ví
dụ: 6 là số hoàn hảo vì 6 = 1 + 2 + 3. Cho số tự nhiên n. Hãy kiểm tra n có là số hoàn hảo.
* Ý tưởng: 
*  - Khởi tạo tổng sum = 0 và một danh sách để chứa các ước số.
*  - Duyệt i = 1 đến n/2. Nếu n chia hết cho i thì + i vào sum và thêm i vào danh sách.
*  - Cuối cùng, so sánh sum với n. Nếu bằng nhau, kết hợp danh sách ước số thành chuỗi để in ra.
* Mã giả:
*  Begin
*      Input n
*      If n <= 1 Then 
*          Print "khong la so hoan hao" 
*          Exit 
*      End If
*      sum = 0
*      For i = 1 to n/2 Do
*          If n % i == 0 Then
*              sum = sum + i
*              Lưu i
*          End If
*      End For
*      If sum == n Then
*          Print n = (các ước số) la so hoan hao.
*      Else
*          Print n khong la so hoan hao.
*      End If
*  End
*/

using System;

class BT4B6
{
    static void Main()
    {
        Console.Write("Moi ban nhap n: ");
        
        if (int.TryParse(Console.ReadLine(), out int n))
        {
            string ketQua = MyLib.B4B6.KiemTraSoHoanHao(n);
            Console.WriteLine(ketQua);
        }
        else
        {
            Console.WriteLine("Vui long nhap mot so nguyen.");
        }
    }
}