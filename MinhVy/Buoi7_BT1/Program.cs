using System;
using System.Collections.Generic;
using MyLib;
namespace Buoi7_BT1;
public class Program
{
    public static void Main()
    {
        Console.Write("hay nhap chieu cao h");
        int h=int.Parse(Console.ReadLine());
        int [] list=new int[]{h,h+2,h+4};
        foreach(int c in list){
            Console.WriteLine($"h={c}");
            List<string> tamGiac=Buoi7_InHinhTamGiacVaVuongLib.InHinh(c);
            foreach(string dong in tamGiac)
            {
                Console.WriteLine(dong);
            }
            
        }
    }
}