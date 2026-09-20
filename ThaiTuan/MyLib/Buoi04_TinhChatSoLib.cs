using System;

namespace MyLib
{
    public static class Buoi04_TinhChatSoLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string PhanTichTinhChatSo(int n)
        {
            if (n < 1000 || n > 9999)
            {
                return "So khong hop le, vui long nhap so tu nhien co 4 chu so.";
            }

            int[] chuSo = new int[4];
            chuSo[0] = n / 1000;
            chuSo[1] = (n / 100) % 10;
            chuSo[2] = (n / 10) % 10;
            chuSo[3] = n % 10;

            // (a) Kiem tra doi xung: chu so dau = chu so cuoi va chu so thu 2 = chu so thu 3
            bool doiXung = chuSo[0] == chuSo[3] && chuSo[1] == chuSo[2];

            // Sap xep cac chu so de de dem so lan lap lai lien tiep
            int[] daSapXep = (int[])chuSo.Clone();
            Array.Sort(daSapXep);

            int demLienTiepToiDa = 1;
            int demHienTai = 1;
            bool coChuSoTrungNhau = false;
            for (int i = 1; i < 4; i++)
            {
                if (daSapXep[i] == daSapXep[i - 1])
                {
                    demHienTai++;
                    coChuSoTrungNhau = true;
                    if (demHienTai > demLienTiepToiDa) demLienTiepToiDa = demHienTai;
                }
                else
                {
                    demHienTai = 1;
                }
            }

            // (b) Co it nhat 3 chu so giong nhau
            bool coBaChuSoGiongNhau = demLienTiepToiDa >= 3;

            // (c) Ca 4 chu so deu khac nhau doi mot
            bool coBonChuSoKhacNhau = !coChuSoTrungNhau;

            string ketQuaDoiXung = doiXung ? $"{n} la so doi xung." : $"{n} khong la so doi xung.";
            string ketQuaBaGiong = coBaChuSoGiongNhau ? $"{n} co 3 chu so giong nhau." : $"{n} khong co 3 chu so giong nhau.";
            string ketQuaBonKhac = coBonChuSoKhacNhau ? $"{n} co 4 chu so khac nhau." : $"{n} khong co 4 chu so khac nhau.";

            return $"{ketQuaDoiXung}\n{ketQuaBaGiong}\n{ketQuaBonKhac}";
        }
    }
}
