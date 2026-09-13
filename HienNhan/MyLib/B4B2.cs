namespace MyLib
{
    public class B4B2
    {
        public static string TimQui(int thang)
        {
            if (thang < 1 || thang > 12)
            {
                return "Thang khong hop le.";
            }

            // Công thức tính quí: (tháng - 1) / 3 + 1
            int qui = (thang - 1) / 3 + 1;
            return $"Thang {thang} thuoc qui {qui}.";
        }
    }
}