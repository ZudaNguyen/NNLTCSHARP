using System;
using System.Collections.Generic;
using MyLib;
namespace Buoi6_BT1
{
    public class InChuIvaU
    {
        public static void Main()
        {
            Console.WriteLine("moi ban nhap chieu cao h:");
            int h=int.Parse(Console.ReadLine());
            Console.WriteLine($"Chu I va U ung voi h = {h} la:");
            List<String> hinh = InChuIvaULib.TaoHinhChuIVaU(h);
            foreach(string doing in hinh )
            {
                Console.WriteLine(doing);
            }
        }
    }
}