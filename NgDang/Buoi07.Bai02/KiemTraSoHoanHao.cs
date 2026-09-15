using System;
using MyLib;

namespace Buoi07.Bai02
{
    public class KiemTraSoHoanHao
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            KiemTraSoHoanHaoLib lib = new KiemTraSoHoanHaoLib();
            var ketQua = lib.KiemTra(n);

            if (ketQua.IsPerfect)
            {
                string chuoiUocSo = string.Join(" + ", ketQua.Divisors);
                Console.WriteLine($"{n} = {chuoiUocSo} la so hoan hao.");
            }
            else
            {
                Console.WriteLine($"{n} khong phai la so hoan hao.");
            }
        }
    }
}