using System;

namespace MyLib
{
    public static class Buoi12_MaTranToanLeLib
    {
        // Hàm kiểm tra ma trận toàn lẻ, nhận vào mảng 2 chiều và kích thước n, m
        public static bool KiemTraToanLe(int[,] a, int n, int m)
        {
            // Kiểm tra điều kiện giới hạn kích thước theo đề bài
            if (n < 1 || n > 100 || m < 1 || m > 100)
            {
                throw new ArgumentException("Kich thuoc ma tran khong hop le (1 <= n, m <= 100).");
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    // Nếu phát hiện bất kỳ số nào chia hết cho 2 thì lập tức trả về false
                    if (a[i, j] % 2 == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}