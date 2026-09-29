// * Tác giả : Võ Minh Vỹ

// *
// * Phát biểu đề bài:
// *   Viết các phương thức nhận vào 5 số thực a, b, c, d, e 
// *   và trả về giá trị lớn nhất (Max), nhỏ nhất (Min).
// *
// * Ý tưởng:
// *   Sử dụng hàm Math.Max và Math.Min lồng nhau để so sánh 5 số mà không làm thay đổi biến gốc.
// *
// * Mã giả:
// *   Hàm TimMax(a, b, c, d, e):
// *     Trả về Math.Max(a, Math.Max(b, Math.Max(c, Math.Max(d, e))))
// *   Hàm TimMin(a, b, c, d, e):
// *     Trả về Math.Min(a, Math.Min(b, Math.Min(c, Math.Min(d, e))))
using System;
using MyLib;
namespace Buoi3_BT1
{
    public class MaxMin5So
    {
        public static void Main()
        {
            Console.Write("hay nhap 5 so bat ki");
            String[] songuyen=Console.ReadLine()!.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            double a=double.Parse(songuyen[0]);
            double b=double.Parse(songuyen[1]);
            double c=double.Parse(songuyen[2]);
            double d=double.Parse(songuyen[3]);
            double e=double.Parse(songuyen[4]);
            
            double max=Buoi3_MaxMin5So.timMax(a,b,c,d,e);
            double min=Buoi3_MaxMin5So.timMin(a,b,c,d,e);

            Console.WriteLine("gia tri lon nhat la"+max);
            Console.WriteLine("gia tri nho nhat la"+min);
            
        }
    }
}

