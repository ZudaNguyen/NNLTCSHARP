namespace MyLib
{
    public class GiaTriBieuThucLib
    {
        public double TinhS1(int n)
        {
            double sum = 0;
            for (int i = 1; i <= n; i++)
            {
                sum += 1.0 / i;
            }
            return sum;
        }

        public double TinhS2(int n)
        {
            double sum = 0;
            int sign = 1;

            for (int i = n; i >= 1; i--)
            {
                sum += sign * (1.0 / i);
                sign = -sign;
            }
            return sum;
        }
    }
}