using System;
using MyLib;

namespace Buoi10.Bai02
{
    public class KeNhauTraiDau
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so luong phan tu n (<= 500): ");
            int n = Convert.ToInt32(Console.ReadLine());

            if (n <= 0 || n > 500)
            {
                Console.WriteLine("So luong phan tu khong hop le.");
                return;
            }

            double[] arr = new double[n];
            Console.WriteLine("// Nhap day so");
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Phan tu {i}: ");
                arr[i] = Convert.ToDouble(Console.ReadLine());
            }

            Console.Write($"Day so co {n} phan tu: ");
            Console.WriteLine(string.Join(", ", arr));

            KeNhauTraiDauLib lib = new KeNhauTraiDauLib();
            int ketQua = lib.DemCapTraiDau(arr);

            Console.WriteLine($"So luong cac phan tu ke nhau ma trai dau: {ketQua}.");
        }
    }
}