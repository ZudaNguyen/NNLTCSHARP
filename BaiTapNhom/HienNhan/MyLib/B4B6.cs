using System.Collections.Generic;

namespace MyLib
{
    public class B4B6
    {
        public static string KiemTraSoHoanHao(int n)
        {
            if (n <= 1)
            {
                return $"{n} khong la so hoan hao.";
            }

            int sum = 0;
            List<int> uocSo = new List<int>();

            // chạy vòng lặp đến n/2 vì không có ước số thực sự nào lớn hơn n/2
            for (int i = 1; i <= n / 2; i++)
            {
                if (n % i == 0)
                {
                    sum += i;
                    uocSo.Add(i); // Lưu lại ước số để in ra màn hình
                }
            }

            if (sum == n)
            {
                // Nối các phần tử trong danh sách bằng dấu " + "
                string bieuThuc = string.Join(" + ", uocSo);
                return $"{n} = {bieuThuc} la so hoan hao.";
            }
            else
            {
                return $"{n} khong la so hoan hao.";
            }
        }
    }
}