namespace MyLib
{
    public static class Buoi03_SoNgayThangKeNhuanLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string TinhSoNgayTrongThang(int thang, int nam)
        {
            if (thang < 1 || thang > 12)
            {
                return "Thang khong hop le.";
            }

            int soNgay;
            switch (thang)
            {
                case 4:
                case 6:
                case 9:
                case 11:
                    soNgay = 30;
                    break;
                case 2:
                    bool namNhuan = (nam % 4 == 0 && nam % 100 != 0) || (nam % 400 == 0);
                    soNgay = namNhuan ? 29 : 28;
                    break;
                default:
                    soNgay = 31;
                    break;
            }

            return $"Thang {thang} trong nam {nam} co {soNgay} ngay.";
        }
    }
}
