using System;
using System.Collections.Generic;
using System.Linq;

namespace MyLib
{
    public class SapChanLeLib
    {
        public static int[] SapXepChanLe(int[] arr)
        {
            if (arr == null) return new int[0];

            List<int> dsChan = new List<int>();
            List<int> dsLe = new List<int>();

            foreach (int x in arr)
            {
                if (x % 2 == 0)
                    dsChan.Add(x);
                else
                    dsLe.Add(x);
            }

            // Sắp xếp chẵn tăng dần
            dsChan.Sort();

            // Sắp xếp lẻ giảm dần
            dsLe.Sort((a, b) => b.CompareTo(a));

            // Nối 2 mảng lại với nhau
            return dsChan.Concat(dsLe).ToArray();
        }
    }
}