/*
* CHƯƠNG TRÌNH TÌM NGÀY TRƯỚC ĐÓ
* Tác giả : Nguyen Van Hien Nhan - 3123411207
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

using system;

class Program
{
    static void Main (String[] args){
        Console.WriteLine("Nhập ngày: ");
        int day = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Nhập tháng: ");
        int month = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Nhập năm: ");
        int year = Convert.ToInt32(Console.ReadLine());

        if(ngayHopLe && monthHopLe && yearHopLe){
            String result = MyLib.B4B7.NgayTruoc(day, month, year);
            Console.WriteLine($"Ngày trước đó là: {result}");
        } else {
            Console.WriteLine("Ngày/tháng/năm không hợp lệ");
        }
    }

}