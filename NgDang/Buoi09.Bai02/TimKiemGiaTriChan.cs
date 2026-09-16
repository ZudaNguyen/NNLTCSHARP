using System;
using MyLib;

namespace Buoi09.Bai02
{
    public class TimKiemGiaTriChan
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so luong phan tu: ");
            int n = Convert.ToInt32(Console.ReadLine());

            if (n <= 0 || n > 500)
            {
                Console.WriteLine("So luong phan tu khong hop le.");
                return;
            }

            int[] arr = new int[n];
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Phan tu {i}: ");
                arr[i] = Convert.ToInt32(Console.ReadLine());
            }

            TimKiemGiaTriChanLib lib = new TimKiemGiaTriChanLib();
            var kq = lib.TimChanDauCuoi(arr);

            if (kq.firstIndex != -1)
            {
                Console.WriteLine($"Gia tri chan dau tien o vi tri {kq.firstIndex} co gia tri {kq.firstValue}.");
                Console.WriteLine($"Gia tri chan cuoi cung o vi tri {kq.lastIndex} co gia tri {kq.lastValue}.");
            }
            else
            {
                Console.WriteLine("Khong co gia tri chan duong nao trong mang.");
            }
        }
    }
}