namespace MyLib
{
    public static class Buoi05_TongSaiSoLib
    {
        // Hàm xử lý vòng lặp tính tổng phân số dựa trên sai số epsilon
        public static double TinhTongSaiSo(double a, double epsilon)
        {
            double S = 0.0;
            int n = 0;
            double term = 1.0 / a;

            while (term >= epsilon)
            {
                S += term;
                n++;
                term = 1.0 / (a + n);
            }

            return S;
        }
    }
}