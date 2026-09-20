using System;
using System.Collections.Generic;

namespace MyLib
{
    public static class Buoi11_ThemChanLeLib
    {
        // Hàm nhận vào mảng số nguyên và trả về danh sách đã được chèn số lẻ
        public static List<int> ThemChanLe(int[] arr)
        {
            // Kiểm tra giới hạn n <= 500 theo đề bài
            if (arr == null || arr.Length == 0 || arr.Length > 500)
            {
                throw new ArgumentException("So luong phan tu phai > 0 va <= 500.");
            }

            List<int> ketQua = new List<int>();

            for (int i = 0; i < arr.Length; i++)
            {
                ketQua.Add(arr[i]);

                // Nếu phần tử hiện tại và phần tử kế tiếp đều chẵn thì chèn (a[i] + 1)
                if (i < arr.Length - 1 && arr[i] % 2 == 0 && arr[i + 1] % 2 == 0)
                {
                    ketQua.Add(arr[i] + 1);
                }
            }

            return ketQua;
        }
    }
}