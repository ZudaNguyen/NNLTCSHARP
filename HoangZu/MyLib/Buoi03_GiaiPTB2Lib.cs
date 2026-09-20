using System;

namespace MyLib
{
    public static class Buoi03_GiaiPTB2Lib
    {
        // Hàm trả về số nguyên đại diện cho số nghiệm: 
        // -1 (vô số nghiệm), 0 (vô nghiệm), 1 (1 nghiệm / nghiệm kép), 2 (2 nghiệm phân biệt)
        // Giá trị x1, x2 sẽ được gán và đẩy ra ngoài qua tham số 'out'
        public static int GiaiPhuongTrinh(double a, double b, double c, out double x1, out double x2)
        {
            x1 = 0;
            x2 = 0;

            if (a == 0)
            {
                if (b == 0)
                {
                    return (c == 0) ? -1 : 0;
                }
                else
                {
                    x1 = x2 = -c / b;
                    return 1;
                }
            }
            else
            {
                double delta = b * b - 4 * a * c;
                if (delta < 0)
                {
                    return 0;
                }
                else if (delta == 0)
                {
                    x1 = x2 = -b / (2 * a);
                    return 1;
                }
                else
                {
                    double sqrtDelta = Math.Sqrt(delta);
                    x1 = (-b + sqrtDelta) / (2 * a);
                    x2 = (-b - sqrtDelta) / (2 * a);
                    return 2;
                }
            }
        }
    }
}