namespace MyLib
{
    public class DiemTrungBinh
    {
        public static double TinhDTB(double toan,double ly,double hoa)
        {
            return (toan*2+ly*3+hoa)/6.0;
        }
        public static string xepLoai(double dtb)
        {
            if (dtb >= 8.0 && dtb <= 10.0) return "Gioi";
            if (dtb >= 6.5) return "Kha";
            if (dtb >= 5.0) return "Trung binh";
            return "Yeu";
        }
    }
}
