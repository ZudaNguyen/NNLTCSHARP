namespace MyLib
{
    public class KeNhauTraiDauLib
    {
        public int DemCapTraiDau(double[] arr)
        {
            int count = 0;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                if (arr[i] * arr[i + 1] < 0)
                {
                    count++;
                }
            }
            return count;
        }
    }
}