using System.Globalization;

namespace MyLib
{
    public static class Buoi10_DoanDuongDaiNhatLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string TimDoanDuongDaiNhat(double[]? daySo)
        {
            if (daySo == null || daySo.Length == 0)
            {
                return "Day so rong.";
            }

            string dongDaySo = $"Day so co {daySo.Length} phan tu: {GhepChuoi(daySo)}";

            int viTriDauTotNhat = -1;
            int viTriCuoiTotNhat = -1;
            int doDaiToiDa = 0;

            int viTriDauHienTai = -1;
            int doDaiHienTai = 0;

            for (int i = 0; i < daySo.Length; i++)
            {
                if (daySo[i] > 0)
                {
                    if (doDaiHienTai == 0)
                    {
                        viTriDauHienTai = i;
                    }
                    doDaiHienTai++;

                    if (doDaiHienTai > doDaiToiDa)
                    {
                        doDaiToiDa = doDaiHienTai;
                        viTriDauTotNhat = viTriDauHienTai;
                        viTriCuoiTotNhat = i;
                    }
                }
                else
                {
                    doDaiHienTai = 0;
                }
            }

            if (doDaiToiDa == 0)
            {
                return $"{dongDaySo}\nKhong co doan so duong nao trong day so.";
            }

            double[] doanDuong = new double[doDaiToiDa];
            for (int i = 0; i < doDaiToiDa; i++)
            {
                doanDuong[i] = daySo[viTriDauTotNhat + i];
            }

            string dongKetQua = $"Doan duong [{viTriDauTotNhat}, {viTriCuoiTotNhat}] dai nhat: {GhepChuoi(doanDuong)}";

            return $"{dongDaySo}\n{dongKetQua}";
        }

        private static string GhepChuoi(double[] mang)
        {
            string[] chuoiPhanTu = new string[mang.Length];
            for (int i = 0; i < mang.Length; i++)
            {
                chuoiPhanTu[i] = mang[i].ToString(CultureInfo.InvariantCulture);
            }
            return string.Join(" ", chuoiPhanTu);
        }
    }
}
