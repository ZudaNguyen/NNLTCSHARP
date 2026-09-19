using System;

namespace MyLib
{
    public static class Buoi01_TinhDienTichTamGiacLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string TinhDienTichTamGiac(double a, double b, double c)
        {
            if (a <= 0 || b <= 0 || c <= 0 || a + b <= c || a + c <= b || b + c <= a)
            {
                return "Ba canh khong tao thanh mot tam giac.";
            }

            double p = (a + b + c) / 2;
            double s = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

            return $"Dien tich tam giac S = {s:F2}";
        }
    }
}
