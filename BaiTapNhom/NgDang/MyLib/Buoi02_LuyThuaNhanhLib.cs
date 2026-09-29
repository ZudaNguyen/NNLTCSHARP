namespace MyLib
{
    public class LuyThuaNhanhLib
    {
        // Sử dụng ValueTuple để trả về 3 giá trị: a2, a5, a17
        public (double a2, double a5, double a17) TinhLuyThua(double a)
        {
            double a2 = a * a;       // Phép nhân 1
            double a4 = a2 * a2;     // Phép nhân 2
            double a5 = a4 * a;      // Phép nhân 3
            double a8 = a4 * a4;     // Phép nhân 4
            double a16 = a8 * a8;    // Phép nhân 5
            double a17 = a16 * a;    // Phép nhân 6

            return (a2, a5, a17);
        }
    }
}