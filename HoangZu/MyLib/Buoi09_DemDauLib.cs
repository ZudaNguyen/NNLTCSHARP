using System;

namespace MyLib
{
    public static class Buoi09_DemDauLib
    {
        // Hàm nhận vào mảng số thực và trả về bộ 3 kết quả đếm
        public static (int Am, int Khong, int Duong) Dem(double[] arr)
        {
            // Kiểm tra điều kiện giới hạn số lượng phần tử của đề bài
            if (arr == null || arr.Length == 0 || arr.Length > 500)
            {
                throw new ArgumentException("So luong phan tu phai lon hon 0 va khong vuot qua 500.");
            }

            int demAm = 0;
            int demKhong = 0;
            int demDuong = 0;

            foreach (double giaTri in arr)
            {
                if (giaTri < 0)
                {
                    demAm++;
                }
                else if (giaTri == 0)
                {
                    demKhong++;
                }
                else
                {
                    demDuong++;
                }
            }

            return (demAm, demKhong, demDuong);
        }
    }
}