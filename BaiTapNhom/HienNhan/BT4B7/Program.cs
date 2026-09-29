/*
* CHƯƠNG TRÌNH TÌM NGÀY TRƯỚC ĐÓ
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 15/09/2026
*
* Phát biểu đề bài: Nhập vào ngày, tháng, năm. Hỏi ngày trước đó là ngày nào?
* Ý tưởng: 
*  - Nếu ngày > 1: Ngày trước đó là ngày - 1, giữ nguyên tháng, năm.
*  - Nếu ngày = 1: 
*      + Nếu tháng = 1: Ngày trước đó là 31/12 của năm - 1.
*      + Nếu tháng > 1: Ngày trước đó là ngày cuối cùng của tháng trước, giữ nguyên năm. Cần kết hợp kiểm tra năm nhuận cho tháng 2.
* Mã giả:
*  Begin
*      Input d, m, y
*      If d > 1 Then
*          prevD = d - 1, prevM = m, prevY = y
*      Else
*          If m == 1 Then
*              prevD = 31, prevM = 12, prevY = y - 1
*          Else
*              prevM = m - 1
*              prevD = DaysInMonth(prevM, y)
*              prevY = y
*          End If
*      End If
*      Print result
*  End
*/

using System;

class Program
{
    static void Main (String[] args){
        Console.Write("Moi ban nhap ngay: ");
        bool ngayHopLe = int.TryParse(Console.ReadLine(), out int day);
        
        Console.Write("Moi ban nhap thang: ");
        bool monthHopLe = int.TryParse(Console.ReadLine(), out int month);
        
        Console.Write("Moi ban nhap nam: ");
        bool yearHopLe = int.TryParse(Console.ReadLine(), out int year);

        if (ngayHopLe && monthHopLe && yearHopLe) 
        {
            String result = MyLib.B4B7.NgayTruoc(day, month, year);
            Console.WriteLine($"Ngày trước đó là: {result}");
        } else {
            Console.WriteLine("Ngày/tháng/năm không hợp lệ");
        }
    }

}