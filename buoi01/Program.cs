/*
* CHƯƠNG TRÌNH IN NHÃN THÔNG TIN 
* Tác giả : Võ Minh Vỹ
* Ngày viết: 09/09/2026
*
* Phát biểu đề bài:Viết chương trình c# in ra màn hình nhãn thông tin gồm 5 dòng
* Ý tưởng:dùng câu lệnh Console.writeline() xuất dòng 
* Mã giả:
Bắt đầu
*     In dòng 1: "************************"
*     In dòng 2: "* Truong: Dai Hoc HUFLIT *"
*     In dòng 3: "* Khoa: CNTT            *"
*     In dòng 4: "* Ho ten:               *"
*     In dòng 5: "************************"
*   Kết thúc
*/
using System;

namespace buoi01
{
    public class Bai01
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("************************");
            Console.WriteLine("* Truong: Dai Hoc HUFLIT *");
            Console.WriteLine("* Khoa: CNTT            *");
            Console.WriteLine("* Ho ten:               *");
            Console.WriteLine("************************");
        }
    }
}