using System;
using MyLib;

namespace Buoi12.Bai02
{
    public class DemSoTrenBienMang2C
    {
        public void ThucHien()
        {
            Console.WriteLine("// Nhap ma tran A");
            Console.Write("Nhap so dong n: ");
            int n = Convert.ToInt32(Console.ReadLine());
            Console.Write("Nhap so cot m: ");
            int m = Convert.ToInt32(Console.ReadLine());

            if (n <= 0 || m <= 0 || n > 100 || m > 100)
            {
                Console.WriteLine("Kich thuoc ma tran khong hop le.");
                return;
            }

            double[,] matrix = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Console.Write($"Nhap phan tu [{i}, {j}]: ");
                    matrix[i, j] = Convert.ToDouble(Console.ReadLine());
                }
            }

            Console.WriteLine($"\nMang co {n} dong x {m} cot:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Console.Write($"{matrix[i, j], 6} ");
                }
                Console.WriteLine();
            }

            DemSoTrenBienMang2CLib lib = new DemSoTrenBienMang2CLib();
            var ketQua = lib.Dem(matrix);

            Console.WriteLine($"Tren duong bien co {ketQua.pos} so duong, {ketQua.neg} so am va {ketQua.zero} so khong.");
        }
    }
}