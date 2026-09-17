using System.Collections.Generic;
namespace MyLib
{
    public class Buoi9_LietKeChanLib
    {
        public static List<int> lietKeSoChan(int[] arr)
        {
            List<int> ketqua=new List<int>();
            if(arr==null) return ketqua;
            for(int i = arr.Length - 1; i >= 0; i--)
            {
                if (arr[i] % 2 == 0)
                {
                    ketqua.Add(arr[i]);
                }
            }
            return ketqua;
        }
    }
}