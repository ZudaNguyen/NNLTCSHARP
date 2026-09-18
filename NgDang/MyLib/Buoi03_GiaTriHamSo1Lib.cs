using System;

namespace MyLib
{
    public class GiaTriHamSo1Lib
    {
        // Tính f1(x)
        public double TinhF1(double x)
        {
            if (x <= 0)
            {
                return 0;
            }
            else if (x <= 1) // Lúc này x đã > 0, chỉ cần kiểm tra <= 1
            {
                return x;
            }
            else // Trường hợp x > 1
            {
                return Math.Pow(x, 4); // Tương đương x * x * x * x
            }
        }

        // Tính f2(x)
        public double TinhF2(double x)
        {
            // Do cả 2 trường hợp đều dùng chung biểu thức x^2 + 4x + 5, 
            // ta tính trước gán vào biến để tránh phải tính lại 2 lần
            double bieuThuc = (x * x) + (4 * x) + 5; 

            if (x <= 2)
            {
                return bieuThuc;
            }
            else
            {
                return 1.0 / bieuThuc;
            }
        }
    }
}