namespace MyLib;
public class Buoi10_TrungBinhCongLib
{
    public static double tinhTrungBinhCong(double [] arr)
    {
        if(arr==null||arr.Length<3) return 0;
        double tong=0;
        for(int i = 1; i < arr.Length - 1; i++)
        {
            bool cucDai=arr[i] > arr[i-1] && arr[i] > arr[i+1];
            bool cucTieu = arr[i] < arr[i - 1] && arr[i] < arr[i + 1];

            if (cucDai || cucTieu)
            {
                tong+=arr[i];
            }
        }
        return tong;
    }
}