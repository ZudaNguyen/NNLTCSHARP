using System;
using System.Globalization;

namespace MyLib
{
    public class B4B4
    {
        public static string PhanLoaiTamGiac(double a, double b, double c)
        {
            // Dùng InvariantCulture để đảm bảo số thực luôn in ra bằng dấu chấm
            string aStr = a.ToString(CultureInfo.InvariantCulture);
            string bStr = b.ToString(CultureInfo.InvariantCulture);
            string cStr = c.ToString(CultureInfo.InvariantCulture);

            // Kiểm tra điều kiện tồn tại tam giác
            if (a <= 0 || b <= 0 || c <= 0 || a + b <= c || a + c <= b || b + c <= a)
            {
                return $"Ba so ({aStr}, {bStr}, {cStr}) khong tao thanh tam giac.";
            }

            string tienTo = $"Ba so ({aStr}, {bStr}, {cStr}) tao thanh duoc tam giac.\nTam giac tao thanh la tam giac ";
            string loai = "thuong";

            // Dùng sai số nhỏ để so sánh số thực, tránh lỗi bộ nhớ
            double saiSo = 1e-6;
            bool isDeu = Math.Abs(a - b) < saiSo && Math.Abs(b - c) < saiSo;
            bool isCan = Math.Abs(a - b) < saiSo || Math.Abs(b - c) < saiSo || Math.Abs(a - c) < saiSo;
            
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