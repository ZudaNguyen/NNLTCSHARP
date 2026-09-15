namespace MyLib
{

public class Buoi2_Tongdoan
{
public static long Tongdoan(long a,long b)
    {
        if(a>b) return 0;
        return b*(b+1)/2-(a-1)*a/2;
    }
}
}
