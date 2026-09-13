using System;

namespace MyLib
{
    public class B4B4
    {
        public static string PhanLoaiTamGiac(double a, double b, double c)
        {
            // Kiểm tra điều kiện tồn tại tam giác
            if (a <= 0 || b <= 0 || c <= 0 || a + b <= c || a + c <= b || b + c <= a)
            {
                return $"Ba so ({a}, {b}, {c}) khong tao thanh tam giac.";
            }

            string tienTo = $"Ba so ({a}, {b}, {c}) tao thanh duoc tam giac.\nTam giac tao thanh la tam giac ";
            string loai = "thuong";

            bool isDeu = (a == b) && (b == c);
            bool isCan = (a == b) || (b == c) || (a == c);
            
            // Dùng sai số nhỏ để tránh lỗi so sánh số thực khi bình phương
            double saiSo = 1e-5;
            bool isVuong = Math.Abs(a * a + b * b - c * c) < saiSo ||
                           Math.Abs(a * a + c * c - b * b) < saiSo ||
                           Math.Abs(b * b + c * c - a * a) < saiSo;

            if (isDeu) loai = "deu";
            else if (isVuong && isCan) loai = "vuong can";
            else if (isVuong) loai = "vuong";
            else if (isCan) loai = "can";

            return tienTo + loai + ".";
        }
    }
}