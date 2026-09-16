using System.Collections.Generic;
namespace MyLib;
public class Buoi5_GiaiThua
{ 
    public static long TinhGiaiThua(int n)
    {
        if(n<0) return 0;
        long giaiThua=1;
        for(int i = 1; i <= n; i++)
        {
            giaiThua=giaiThua*i;
        }
         return giaiThua;
    }
    
    
}