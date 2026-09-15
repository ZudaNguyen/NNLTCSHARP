using System;
using MyLib;

namespace Buoi04.Bai02
{
    public class TienNuoc
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap chi so tieu thu nuoc thang truoc (m^3): ");
            double chiSoCu = Convert.ToDouble(Console.ReadLine());

            Console.Write("Moi ban nhap chi so tieu thu nuoc thang nay (m^3): ");
            double chiSoMoi = Convert.ToDouble(Console.ReadLine());

            Console.Write("Moi ban nhap so nguoi tieu thu (da dang ky): ");
            int soNguoi = Convert.ToInt32(Console.ReadLine());

            double soM3 = chiSoMoi - chiSoCu;

            TienNuocLib lib = new TienNuocLib();
            double tongTien = lib.TinhTienNuoc(chiSoCu, chiSoMoi, soNguoi);

            Console.WriteLine($"So tien phai tra cho {soM3} m^3 tieu thu trong thang la {tongTien} D.");
        }
    }
}