using System;

namespace MyLib
{
    public static class Buoi07_TinhPiLib
    {
        // Hàm tính số Pi dựa trên sai số epsilon, tách biệt hoàn toàn khỏi việc in ra màn hình
        public static double TinhPi(double epsilon)
        {
            double pi = 0.0;
            int n = 0;
            double term = 4.0 / (2 * n + 1);
            double sign = 1.0;

            while (term >= epsilon)
            {
                pi += sign * term;
                sign = -sign;
                n++;
                term = 4.0 / (2 * n + 1);
            }

            return pi;
        }
    }
}