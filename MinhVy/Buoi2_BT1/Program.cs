/*
* THƯ VIỆN TÍNH TỔNG ĐOẠN [A, B]
* Tác giả: Võ Minh Vỹ
* Ngày viết: 15/09/2026
*
* Ý tưởng: Tong(a, b) = Sum(1..b) - Sum(1..a-1)
*/
using System;
using MyLib;
namespace Buoi2_BT1
{
    class Program
    {
        static void Main(String[] args )
        {
            Console.WriteLine("moi nhap so a,b");
           String[] sothuc=Console.ReadLine()!.Split( ' ',StringSplitOptions.RemoveEmptyEntries);
            long a=long.Parse(sothuc[0]);
            long b=long.Parse(sothuc[1]);
            long ketqua=Buoi2_Tongdoan.Tongdoan(a,b);
            Console.WriteLine("Tong cac so trong doan="+ketqua);

        }
    }
}