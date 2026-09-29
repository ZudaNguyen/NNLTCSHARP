using System.Collections.Generic;

namespace MyLib
{
    public class KiemTraSoHoanHaoLib
    {
        public (bool IsPerfect, List<int> Divisors) KiemTra(int n)
        {
            List<int> divisors = new List<int>();

            if (n <= 1) return (false, divisors);

            int sum = 0;

            for (int i = 1; i <= n / 2; i++)
            {
                if (n % i == 0)
                {
                    divisors.Add(i);
                    sum += i;   
                }
            }

            bool isPerfect = (sum == n);

            return (isPerfect, divisors);
        }
    }
}