/*
* CHUONG TRINH TIM SO NGAY CUA THANG (CO XET NAM NHUAN)
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Nhap vao thang, nam. Hay cho biet thang trong nam vua nhap co bao nhieu ngay.
*
* Y tuong:
*  - Cac thang 4, 6, 9, 11 co 30 ngay.
*  - Thang 2 co 28 ngay, hoac 29 ngay neu nam do la nam nhuan.
*  - Cac thang con lai co 31 ngay.
*  - Nam nhuan: chia het cho 4 va khong chia het cho 100, hoac chia het cho 400.
*
* Ma gia:
*  Begin
*      Input thang, nam
*      If thang = 4, 6, 9, 11 Then soNgay = 30
*      Else If thang = 2 Then
*          If nam nhuan Then soNgay = 29 Else soNgay = 28
*      Else soNgay = 31
*      Print thang, nam, soNgay
*  End
*/

using System;
using MyLib;

class Buoi3_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap vao thang, nam: ");
        string[] phan = Console.ReadLine()!.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int thang = int.Parse(phan[0]);
        int nam = int.Parse(phan[1]);

        string ketQua = Buoi03_SoNgayThangKeNhuanLib.TinhSoNgayTrongThang(thang, nam);
        Console.WriteLine(ketQua);
    }
}
