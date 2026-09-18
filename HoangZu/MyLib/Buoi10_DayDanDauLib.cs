using System;

namespace MyLib
{
    public static class Buoi10_DayDanDauLib
    {
        // Hàm kiểm tra dãy đan dấu, trả về true nếu đúng, false nếu sai
        public static bool KiemTraDanDau(double[] arr)
        {
            // Kiểm tra điều kiện giới hạn số lượng phần tử
            if (arr == null || arr.Length == 0 || arr.Length > 500)
            {
                throw new ArgumentException("So luong phan tu phai > 0 va <= 500.");
            }

            // Thuật toán kiểm tra đan dấu
            for (int i = 0; i < arr.Length - 1; i++)
            {
                // Nếu 2 số liền kề nhân nhau ra số dương (cùng dấu) hoặc bằng 0 -> Sai
                if (arr[i] * arr[i + 1] >= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}