using System.Collections.Generic;

namespace MyLib
{
    public class LietKeSoHoanHaoLib
    {
        public bool KiemTraSoHoanHao(int num)
        {
            if (num < 1) return false;
            int sum = 0;
            for (int i = 1; i <= num / 2; i++)
            {
                if (num % i == 0) sum += i;
            }
            return sum == num;
        }

        public List<int> TimSoHoanHaoGiamDan(int n)
        {
        List<int> ketQua = new List<int>();
        for (int i = n; i >= 1; i--)
            {
                if (KiemTraSoHoanHao(i))
                {
                    ketQua.Add(i);
                }
            }
            
        return ketQua;
        }
    }
}