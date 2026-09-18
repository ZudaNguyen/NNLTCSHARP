/*
* CHƯƠNG TRÌNH ĐẾM SỐ LẦN XUẤT HIỆN CỦA CHỮ SỐ
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 16/09/2026
*
* Phát biểu đề bài: Nhập vào số nguyên dương n. Hãy đếm số lần xuất hiện của từng chữ số.
* Ý tưởng: 
*  - Viết hàm DemMotChuSo(n, k) để đếm số lần chữ số k xuất hiện trong số n bằng cách lấy phần dư chia 10.
*  - Trong hàm chính, dùng vòng lặp chạy từ 0 đến 9. Với mỗi số i, gọi DemMotChuSo(n, i). 
*  - Nếu đếm được lớn hơn 0 thì in ra kết quả.
* Mã giả:
*  DemMotChuSo(n, k)
*      count = 0
*      While n > 0 Do
*          If n % 10 == k Then count = count + 1
*          n = n / 10
*      Return count
*
*  Begin Main
*      Input n
*      If n > 0 Then
*          For i = 0 To 9 Do
*              count = DemMotChuSo(n, i)
*              If count > 0 Then Print "Chu so ", i, " xuat hien ", count, " lan."
*      End If
*  End
*/

using System;

class BT4B8
{
    static void Main()
    {
        Console.Write("Moi ban nhap so nguyen n: ");
        
        if (int.TryParse(Console.ReadLine(), out int n))
        {
            // Gọi hàm xử lý từ thư viện
            string ketQua = MyLib.B4B8.ThongKeChuSo(n);
            Console.WriteLine(ketQua);
        }
        else
        {
            Console.WriteLine("Vui long nhap mot so nguyen.");
        }
    }
}