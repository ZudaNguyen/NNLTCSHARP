namespace MyLib
{
    public static class Buoi05_ThucDonLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string HienThiThucDon()
        {
            return "THUC DON\n" +
                   "1. Tinh dien tich tam giac\n" +
                   "2. Tinh dien tich hinh chu nhat\n" +
                   "3. Tinh dien tich hinh tron\n" +
                   "4. Thoat\n" +
                   "-------------------------------------------";
        }

        public static string LayThongBaoChucNang(int luaChon)
        {
            switch (luaChon)
            {
                case 1: return "Ban moi vua chon chuc nang tinh dien tich tam giac.";
                case 2: return "Ban moi vua chon chuc nang tinh dien tich hinh chu nhat.";
                case 3: return "Ban moi vua chon chuc nang tinh dien tich hinh tron.";
                default: return "Lua chon khong hop le, vui long chon lai [1, 2, 3, 4].";
            }
        }
    }
}
