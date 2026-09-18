using System;

namespace MyLib
{
    public class TienNuocLib
    {
        public double TinhTienNuoc(double chiSoCu, double chiSoMoi, int soNguoi)
        {
            double soM3 = chiSoMoi - chiSoCu;
            if (soM3 <= 0) return 0;
            if (soNguoi <= 0) soNguoi =1; // mac dinh 1 nguoi neu nguoi dung nhap sai

            // tinh dinh muc theo so nguoi
            double dinhMuc1 = 4 * soNguoi;
            double dinhMuc2 = 6 * soNguoi;

            double tienNuoc = 0;

            if (soM3 <= dinhMuc1)
            {
                tienNuoc = soM3 * 4400;
            }
            else if (soM3 <= dinhMuc2)
            {
                tienNuoc = (dinhMuc1 * 4400) + ((soM3 - dinhMuc1) * 8300);
            }
            else
            {
                tienNuoc = (dinhMuc1 * 4400) + ((dinhMuc2 - dinhMuc1) * 8300) + ((soM3 - dinhMuc2) * 10500);
            }

            // them 5% VAT va 10% phi BVMT
            return tienNuoc * 1.15;
        }
    }
}