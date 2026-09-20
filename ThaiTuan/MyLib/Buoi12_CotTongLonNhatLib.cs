using System;
using System.Collections.Generic;

namespace MyLib
{
    public static class Buoi12_CotTongLonNhatLib
    {
        private const double SaiSo = 1e-9;

        public static double[] TinhTongTungCot(double[,] matran)
        {
            int soDong = matran.GetLength(0);
            int soCot = matran.GetLength(1);
            double[] tongCot = new double[soCot];

            for (int j = 0; j < soCot; j++)
            {
                double tong = 0;
                for (int i = 0; i < soDong; i++)
                {
                    tong += matran[i, j];
                }
                tongCot[j] = tong;
            }

            return tongCot;
        }

        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static int[] LietKeCotTongLonNhat(double[,] matran)
        {
            double[] tongCot = TinhTongTungCot(matran);
            if (tongCot.Length == 0)
            {
                return Array.Empty<int>();
            }

            double tongLonNhat = tongCot[0];
            for (int j = 1; j < tongCot.Length; j++)
            {
                if (tongCot[j] > tongLonNhat)
                {
                    tongLonNhat = tongCot[j];
                }
            }

            List<int> ketQua = new List<int>();
            for (int j = 0; j < tongCot.Length; j++)
            {
                if (Math.Abs(tongCot[j] - tongLonNhat) < SaiSo)
                {
                    ketQua.Add(j);
                }
            }

            return ketQua.ToArray();
        }

        public static string HienThiKetQua(double[,] matran)
        {
            int[] cacCot = LietKeCotTongLonNhat(matran);
            return $"Cac cot co tong lon nhat: {string.Join(" ", cacCot)}";
        }
    }
}
