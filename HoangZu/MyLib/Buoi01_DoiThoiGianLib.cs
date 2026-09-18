namespace MyLib
{
    public static class Buoi01_DoiThoiGianLib
    {
        // Hàm xử lý logic tách biệt khỏi giao diện Console
        public static long DoiSangGiay(int h, int m, int s)
        {
            return h * 3600L + m * 60L + s;
        }
    }
}