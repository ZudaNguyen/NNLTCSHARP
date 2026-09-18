/*
* CHƯƠNG TRÌNH ĐỌC THÁNG TIẾNG ANH
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 13/09/2026
*
* Phát biểu đề bài: Viết chương trình nhập vào tháng từ 1 đến 12. Cho biết tên gọi tiếng Anh của tháng vừa nhập.
* Ý tưởng: Dùng cấu trúc rẽ nhánh switch-case để kiểm tra giá trị tháng nhập vào và in ra chuỗi tiếng Anh tương ứng.
* Mã giả:
*  Begin
*      Input thang
*      Switch (thang)
*          case 1: Print "January"
*          case 2: Print "February"
*          ...
*          case 12: Print "December"
*          default: Print "Thang khong hop le."
*  End
*/

using System;

class BT4B3
{
    static void Main()
    {
        Console.Write("Moi ban nhap vao thang: ");
        
        if (int.TryParse(Console.ReadLine(), out int thang))
        {
            switch (thang)
            {
                case 1: Console.WriteLine("Tieng anh cua thang 1 la January."); break;
                case 2: Console.WriteLine("Tieng anh cua thang 2 la February."); break;
                case 3: Console.WriteLine("Tieng anh cua thang 3 la March."); break;
                case 4: Console.WriteLine("Tieng anh cua thang 4 la April."); break;
                case 5: Console.WriteLine("Tieng anh cua thang 5 la May."); break;
                case 6: Console.WriteLine("Tieng anh cua thang 6 la June."); break;
                case 7: Console.WriteLine("Tieng anh cua thang 7 la July."); break;
                case 8: Console.WriteLine("Tieng anh cua thang 8 la August."); break;
                case 9: Console.WriteLine("Tieng anh cua thang 9 la September."); break;
                case 10: Console.WriteLine("Tieng anh cua thang 10 la October."); break;
                case 11: Console.WriteLine("Tieng anh cua thang 11 la November."); break;
                case 12: Console.WriteLine("Tieng anh cua thang 12 la December."); break;
                default: Console.WriteLine("Thang khong hop le."); break;
            }
        }
        else
        {
            Console.WriteLine("Vui long nhap mot so nguyen.");
        }
    }
}