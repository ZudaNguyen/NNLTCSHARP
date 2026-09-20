namespace MyLib
{
    public static class Buoi02_GiaTriBieuThucLib
    {
        // Hàm xử lý logic thuật toán Horner tách biệt khỏi giao diện
        public static double TinhGiaTri(double x)
        {
            return 1 + x * (2 + x * (3 - 4 * x));
        }
    }
}