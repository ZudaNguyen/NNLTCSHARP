using System;

namespace MyLib
{
    public class XuLyChuSo1Lib
    {
        public int TimChuSoThuK(int n, int k)
        {
            string chuoiSo = Math.Abs(n).ToString();

            if (k > 0 && k <= chuoiSo.Length)
            {
                return chuoiSo[k -1] - '0';
            }
            return -1;
        }

        public int TongChuSoLe(int n)
        {
            int sum = 0;
            int temp = Math.Abs(n);

            while (temp > 0)
            {
                int digit = temp % 10;
                if (digit % 2 != 0)
                {
                    sum += digit;
                }
                temp /= 10;
            }
            return sum;
        }

        public (int min, int max) TimChuSoLeMinMax(int n)
        {
            int min = 9;
            int max =-1;
            int temp = Math.Abs(n);
            bool hasOdd = false;

            while (temp > 0)
            {
                int digit = temp % 10;
                if (digit % 2 != 0)
                {
                    hasOdd = true;
                    if (digit < min) min = digit;
                    if (digit > max) max = digit;
                }
                temp /= 10;
            }

            if (!hasOdd) return (-1, -1);
            return (min, max);
        }
    }
}