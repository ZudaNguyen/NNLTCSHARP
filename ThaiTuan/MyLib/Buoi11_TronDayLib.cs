namespace MyLib
{
    public static class Buoi11_TronDayLib
    {
        // Tron 2 day da tang dan thanh 1 day tang dan, khong dung thuat toan sap xep
        // (chi duyet song song 2 con tro, giong buoc "merge" trong merge sort).
        public static int[] TronHaiDaySoTangDan(int[] a, int[] b)
        {
            int n = a.Length;
            int m = b.Length;
            int[] c = new int[n + m];

            int i = 0, j = 0, k = 0;
            while (i < n && j < m)
            {
                if (a[i] <= b[j])
                {
                    c[k++] = a[i++];
                }
                else
                {
                    c[k++] = b[j++];
                }
            }

            while (i < n)
            {
                c[k++] = a[i++];
            }
            while (j < m)
            {
                c[k++] = b[j++];
            }

            return c;
        }

        // Ham xu ly logic hien thi, tach biet khoi Console de de kiem thu (unit test)
        public static string HienThiDaySo(int[] daySo)
        {
            return $"Day so co {daySo.Length} phan tu: {string.Join(" ", daySo)}";
        }
    }
}
