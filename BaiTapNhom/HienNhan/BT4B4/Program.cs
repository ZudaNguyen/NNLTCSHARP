/*
* CHƯƠNG TRÌNH PHÂN LOẠI TAM GIÁC
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 13/09/2026
*
* Phát biểu đề bài: Nhập vào ba số thực a, b, c. Hỏi ba số đó có tạo thành một tam giác không? Nếu có, nó tạo thành tam giác gì?
* Ý tưởng: 
*  - Điều kiện tam giác: a, b, c > 0 và tổng 2 cạnh bất kỳ lớn hơn cạnh còn lại.
*  - Tam giác đều: 3 cạnh bằng nhau (so sánh qua sai số epsilon).
*  - Tam giác cân: 2 cạnh bằng nhau.
*  - Tam giác vuông: Thỏa định lý Pytago (a^2 + b^2 = c^2).
* Mã giả:
*  Begin
*      Input a, b, c
*      If a, b, c tao thanh tam giac Then
*          Determine type (Deu, Vuong Can, Vuong, Can, Thuong)
*          Print result
*      Else
*          Print khong tao thanh tam giac
*      End If
*  End
*/

using System;
using System.Globalization;

class BT4B4
{
    static void Main()
    {
        Console.Write("Moi ban nhap ba so thuc a, b, c: ");
        string input = Console.ReadLine();
        
        string[] parts = input?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts != null && parts.Length == 3)
        {
            // Sử dụng NumberStyles và CultureInfo để an toàn với cả dấu chấm và phẩy khi nhập
            if (double.TryParse(parts[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double a) &&
                double.TryParse(parts[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double b) &&
                double.TryParse(parts[2].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double c))
            {
                string ketQua = MyLib.B4B4.PhanLoaiTamGiac(a, b, c);
                Console.WriteLine(ketQua);
            }
            else
            {
                Console.WriteLine("Vui long nhap so thuc hop le.");
            }
        }
        else
        {
            Console.WriteLine("Vui long nhap dung 3 so, cach nhau boi khoang trang.");
        }
    }
}