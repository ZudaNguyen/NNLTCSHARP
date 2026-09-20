using System;
using System.Collections.Generic;

namespace MyLib
{
    public static class Buoi06_PhanTichThuaSoLib
    {
        // Hàm trả về chuỗi kết quả phân tích thừa số nguyên tố
        public static string PhanTich(int n)
        {
            if (n <= 1)
            {
                throw new ArgumentException("So nhap vao phai lon hon 1.");
            }

            List<string> parts = new List<string>();
            int tempN = n;

            for (int i = 2; i * i <= tempN; i++)
            {
                if (tempN % i == 0)
                {
                    int count = 0;
                    while (tempN % i == 0)
                    {
                        count++;
                        tempN /= i;
                    }

                    if (count > 1)
                        parts.Add($"{i}^{count}");
                    else
                        parts.Add($"{i}");
                }
            }

            // Nếu tempN còn lại lớn hơn 1 thì đó chính là thừa số nguyên tố cuối cùng
            if (tempN > 1)
            {
                parts.Add($"{tempN}");
            }

            // Nối các phần tử trong list bằng chuỗi " x "
            return string.Join(" x ", parts);
        }
    }
}