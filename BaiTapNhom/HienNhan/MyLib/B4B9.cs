using System;
using System.Globalization;

namespace MyLib
{
    public class B4B9
    {
        public static string TimKiemX(double[] arr, double x)
        {
            if (arr == null || arr.Length == 0)
            {
                return "Day so rong.";
            }

            int viTriDau = -1;
            int viTriCuoi = -1;
            int soLan = 0;
            double saiSo = 1e-6; // Dùng để so sánh số thực an toàn

            for (int i = 0; i < arr.Length; i++)
            {
                // Kiểm tra 2 số thực có xấp xỉ bằng nhau không
                if (Math.Abs(arr[i] - x) < saiSo) 
                {
                    if (viTriDau == -1)
                    {
                        viTriDau = i; // Chỉ ghi nhận lần đầu tiên
                    }
                    viTriCuoi = i;    // Liên tục cập nhật để lấy lần cuối cùng
                    soLan++;
                }
            }

            string xStr = x.ToString(CultureInfo.InvariantCulture);

            if (soLan > 0)
            {
                return $"Gia tri {xStr} xuat hien {soLan} lan o vi tri dau tien {viTriDau} va cuoi cung {viTriCuoi}.";
            }
            else
            {
                return $"Gia tri {xStr} khong ton tai trong day so.";
            }
        }
    }
}