using System.Collections.Generic;
using System.Globalization;

namespace MyLib
{
    public static class Buoi09_DaySoChanLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string ThongKeDaySo(int[]? daySo)
        {
            if (daySo == null || daySo.Length == 0)
            {
                return "Day so rong.";
            }

            string chuoiDaySo = string.Join(" ", daySo);
            string dongDaySo = $"Day so co {daySo.Length} phan tu {chuoiDaySo}";

            List<int> daySoChan = new List<int>();
            foreach (int x in daySo)
            {
                if (x % 2 == 0)
                {
                    daySoChan.Add(x);
                }
            }

            const string dongTieuDe = "* Day so chan";

            string dongKetQua;
            if (daySoChan.Count == 0)
            {
                dongKetQua = "Day so khong co so chan nao.";
            }
            else
            {
                long tong = 0;
                foreach (int x in daySoChan)
                {
                    tong += x;
                }

                double trungBinh = (double)tong / daySoChan.Count;
                string chuoiSoChan = string.Join(" ", daySoChan);

                dongKetQua = $"Day so co {daySoChan.Count} so chan: {chuoiSoChan} voi tong la {tong}, " +
                             $"trung binh cong la {trungBinh.ToString("F2", CultureInfo.InvariantCulture)}.";
            }

            return $"{dongDaySo}\n{dongTieuDe}\n{dongKetQua}";
        }
    }
}
