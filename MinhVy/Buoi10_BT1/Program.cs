using System;
using MyLib;
namespace Buoi10_BT1;
public class Program
{
    public static void Main()
    {
        Console.WriteLine("nhap n phan tu ");
        string[] input=Console.ReadLine()!.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        double [] arr =new double[input.Length];
        for(int i = 0; i < input.Length; i++)
        {
            arr[i]=double.Parse(input[i]);

        }
        double tong=Buoi10_TrungBinhCongLib.tinhTrungBinhCong(arr);
        Console.WriteLine($"Tong cac phan tu cuc tri co trong day so: {tong}.");
    }
}