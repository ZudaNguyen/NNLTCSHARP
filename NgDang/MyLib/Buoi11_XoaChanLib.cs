using System.Linq;

namespace MyLib
{
    public class XoaChanLib
    {
        public int[] XoaPhanTuChan(int[] arr)
        {
            return arr.Where(x => x % 2 != 0).ToArray();
        }
    }
}