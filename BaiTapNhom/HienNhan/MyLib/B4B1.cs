namespace MyLib
{
    public class B4B1
    {

        public static string DoiSangGioPhutGiay(int t)
        {
            if (t < 0)
            {
                return "So giay khong hop le";
            }

            int h = t / 3600;
            int m = (t % 3600) / 60;
            int s = t % 60;

            return $"{t} giay co dang {h}:{m}:{s}";
        }
    }
}