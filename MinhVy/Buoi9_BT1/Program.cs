using System;
using System.Collections.Generic;
using MyLib;
namespace Buoi9_BT1;
public class Progam
{
    public static void Main()
    {
        Console.WriteLine("moi nhap so luong phan tu");
        int n=int.Parse(Console.ReadLine());
        int [] arr=new int[n];
        for(int i = 0; i < n; i++)
        {
            Console.Write($"Phan tu {i}: ");
                arr[i] = int.Parse(Console.ReadLine()!);
            }

            
            List<int> dsChan = Buoi9_LietKeChanLib.lietKeSoChan(arr);

          
            Console.WriteLine("Cac phan tu chan trong mang: " + string.Join(" ", dsChan));
        }

    }
