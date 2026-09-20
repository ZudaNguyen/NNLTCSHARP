using System;

namespace MyLib
{
    public static class Buoi07_XuLyChuSo1Lib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string XuLyChuSo(long n, int k)
        {
            string chuoiSo = Math.Abs(n).ToString();
            int soChuSo = chuoiSo.Length;

            // (a) Chu so thu k tinh tu trai sang
            string dongA;
            if (k < 1 || k > soChuSo)
            {
                dongA = $"Vi tri k = {k} khong hop le (so {n} co {soChuSo} chu so).";
            }
            else
            {
                char chuSoTaiK = chuoiSo[k - 1];
                dongA = $"Chu so o vi tri {k} (tinh tu trai) cua {n} la {chuSoTaiK}.";
            }

            // (b) Tong cac chu so le va (c) chu so le nho nhat, lon nhat
            int tongChuSoLe = 0;
            int nhoNhat = int.MaxValue;
            int lonNhat = int.MinValue;
            bool coChuSoLe = false;

            foreach (char c in chuoiSo)
            {
                int chuSo = c - '0';
                if (chuSo % 2 != 0)
                {
                    coChuSoLe = true;
                    tongChuSoLe += chuSo;
                    if (chuSo < nhoNhat) nhoNhat = chuSo;
                    if (chuSo > lonNhat) lonNhat = chuSo;
                }
            }

            string dongB = $"Tong cac chu so le cua {n} la {tongChuSoLe}.";
            string dongC = coChuSoLe
                ? $"{n} co chu so le nho nhat la {nhoNhat} va lon nhat la {lonNhat}."
                : $"{n} khong co chu so le nao.";

            return $"{dongA}\n{dongB}\n{dongC}";
        }
    }
}
