using System;
using MyLib;

namespace Buoi03.Bai02
{
    public class GiaTriHamSo1
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so thuc x:");
            double x = Convert.ToDouble(Console.ReadLine());

            // Goi class tu MyLib
            GiaTriHamSo1Lib lib = new GiaTriHamSo1Lib();

            double f1 = lib.TinhF1(x);
            double f2 = lib.TinhF2(x);

            // In ket qua
            Console.WriteLine($"Gia tri f1({x}) = {f1}");
            Console.WriteLine($"Gia tri f2({x}) = {f2}");
        }
    }
}