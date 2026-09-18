namespace MyLib
{
    public class TimKiemGiaTriChanLib
    {
        public (int firstIndex, int firstValue, int lastIndex, int lastValue) TimChanDauCuoi(int[] arr)
        {
            int firstIndex = -1, firstValue = -1;
            int lastIndex = -1, lastValue = -1;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > 0 && arr[i] % 2 == 0)
                {
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                        firstValue = arr[i];
                    }
                    lastIndex = i;
                    lastValue = arr[i];
                }
            }

            return (firstIndex, firstValue, lastIndex, lastValue);
        }
    }
}