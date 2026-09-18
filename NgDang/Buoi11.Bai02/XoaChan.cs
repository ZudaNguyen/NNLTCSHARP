using System;
using MyLib;

namespace Buoi11.Bai02
{
    public class XoaChan
    {
        public void ThucHien()
        {
            Console.WriteLine("// Nhap day so");
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

            Console.WriteLine("* Mang truoc xoa:");
            Console.WriteLine($"Day so co {arr.Length} phan tu: {string.Join(" ", arr)}");

            XoaChanLib lib = new XoaChanLib();
            int[] mangSauKhiXoa = lib.XoaPhanTuChan(arr);

            Console.WriteLine("* Mang sau khi xoa: ");
            Console.WriteLine($"Day so co {mangSauKhiXoa.Length} phan tu: {string.Join(" ", mangSauKhiXoa)}");
        }
    }
}