using System.Collections.Generic;
using System.Globalization;

namespace MyLib
{
    public static class Buoi08_DocSoTiLib
    {
        private static readonly string[] TenDonVi =
        {
            "khong", "mot", "hai", "ba", "bon", "nam", "sau", "bay", "tam", "chin"
        };

        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string DocSo(long n)
        {
            if (n <= 0 || n > 999_999_999_999L)
            {
                return "So khong hop le, vui long nhap so nguyen duong co toi da 12 chu so.";
            }

            long nhomTi = n / 1_000_000_000L;
            long nhomTrieu = (n / 1_000_000L) % 1000;
            long nhomNgan = (n / 1000L) % 1000;
            long nhomDonVi = n % 1000;

            (long GiaTri, string HauTo)[] cacNhom =
            {
                (nhomTi, "ti"),
                (nhomTrieu, "trieu"),
                (nhomNgan, "ngan"),
                (nhomDonVi, "")
            };

            List<string> cacPhanDoc = new List<string>();
            bool daGapNhomKhacKhong = false;

            foreach (var nhom in cacNhom)
            {
                if (nhom.GiaTri == 0)
                {
                    // Nhom bang 0 hoan toan thi bo qua, khong doc
                    continue;
                }

                bool laNhomDauTien = !daGapNhomKhacKhong;
                daGapNhomKhacKhong = true;

                string phanDoc = DocBaChuSo((int)nhom.GiaTri, laNhomDauTien);
                string hauTo = string.IsNullOrEmpty(nhom.HauTo) ? "" : " " + nhom.HauTo;
                cacPhanDoc.Add($"[{phanDoc}{hauTo}]");
            }

            string soCoDauPhay = n.ToString("N0", CultureInfo.InvariantCulture);
            string chuoiDoc = string.Join(" ", cacPhanDoc);

            return $"So {soCoDauPhay} doc thanh: {chuoiDoc}";
        }

        // Doc mot nhom 3 chu so (0 - 999). laNhomDauTien = true neu day la nhom
        // co nghia lon nhat trong so (nhom ngoai cung ben trai duoc doc), khi do
        // khong can them "le" phia truoc neu hang tram bang 0.
        private static string DocBaChuSo(int baChuSo, bool laNhomDauTien)
        {
            int tram = baChuSo / 100;
            int haiChuSoConLai = baChuSo % 100;

            if (tram == 0)
            {
                string phanHaiChuSo = DocHaiChuSo(haiChuSoConLai);
                return laNhomDauTien ? phanHaiChuSo : "le " + phanHaiChuSo;
            }

            string phanTram = TenDonVi[tram] + " tram";
            if (haiChuSoConLai == 0)
            {
                return phanTram;
            }
            if (haiChuSoConLai < 10)
            {
                return phanTram + " le " + TenDonVi[haiChuSoConLai];
            }
            return phanTram + " " + DocHaiChuSo(haiChuSoConLai);
        }

        // Doc mot so co toi da 2 chu so (0 - 99)
        private static string DocHaiChuSo(int soHaiChuSo)
        {
            int chuc = soHaiChuSo / 10;
            int donVi = soHaiChuSo % 10;

            if (chuc == 0)
            {
                return donVi == 0 ? "khong" : TenDonVi[donVi];
            }
            if (chuc == 1)
            {
                if (donVi == 0) return "muoi";
                if (donVi == 5) return "muoi lam";
                return "muoi " + TenDonVi[donVi];
            }

            string ketQua = TenDonVi[chuc] + " muoi";
            if (donVi == 0) return ketQua;
            if (donVi == 5) return ketQua + " lam";
            return ketQua + " " + TenDonVi[donVi];
        }
    }
}
