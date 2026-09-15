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

