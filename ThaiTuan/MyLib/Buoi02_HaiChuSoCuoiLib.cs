namespace MyLib
{
    public static class Buoi02_HaiChuSoCuoiLib
    {
        private const int SoMu = 278;
        private const int Mod = 100;

        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string TinhHaiChuSoCuoi(long n)
        {
            // Dua co so ve khoang [0, 99] truoc, tranh tinh n^278 truc tiep (qua lon va tran so)
            long coSo = ((n % Mod) + Mod) % Mod;
            long ketQua = 1;

            for (int i = 0; i < SoMu; i++)
            {
                ketQua = (ketQua * coSo) % Mod;
            }

            return $"{n}^278 co 2 chu so cuoi cung la {ketQua:D2}.";
        }
    }
}
