using System;
using MyLib;

namespace Buoi06.Bai02
{
    public class XuLyChuSo1
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so nguyen n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            Console.Write("Moi ban nhap vi tri k: ");
            int k = Convert.ToInt32(Console.ReadLine());

            XuLyChuSo1Lib lib = new XuLyChuSo1Lib();

            int chuSoK = lib.TimChuSoThuK(n, k);
            if (chuSoK != -1)
            {
                Console.WriteLine($"Chu so o vi tri {k} (tinh tu trai) cua {n} la {chuSoK}.");
            }
            else
            {
                Console.WriteLine($"Khong tim thay chu so o vi tri {k}");
            }

            int tongLe = lib.TongChuSoLe(n);
            Console.WriteLine($"Tong cac chu so le cua {n} la {tongLe}.");

            var minMaxLe = lib.TimChuSoLeMinMax(n);
            if (minMaxLe.min != -1)
            {
                Console.WriteLine($"{n} co chu so le nho nhat la {minMaxLe.min} va lon nhat la {minMaxLe.max}.");
            }
            else
            {
                Console.WriteLine($"{n} khong co chu so le.");
            }
        }    
    }
}