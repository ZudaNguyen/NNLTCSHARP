using System;
using MyLib;

namespace Buoi05.Bai02
{
    public class GiaTriBieuThuc
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so nguyen n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            if (n <= 0)
            {
                Console.WriteLine("Vui long nhap so nguyen duong lon hon 0.");
                return;
            }

            GiaTriBieuThucLib lib = new GiaTriBieuThucLib();
            double s1 = lib.TinhS1(n);
            double s2 = lib.TinhS2(n);

            Console.WriteLine($"S1(n={n}) = {s1}");
            Console.WriteLine($"S2(n={n}) = {s2}");
        }
    }
}