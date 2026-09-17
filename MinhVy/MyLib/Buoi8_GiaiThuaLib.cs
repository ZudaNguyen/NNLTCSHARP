namespace MyLib
{
    public class Buoi8_GiaiThuaLib
    {
        public static long tinhGiaiThua(int n)
        {
            if(n<0) return 0;
            long gt=1;
            for(int i = 1; i <= n; i++)
            {
                gt=gt*i;
            }
            return gt;

        }
        public static long tinhToHop(int k,int n)
        {
            if(k<0||k>n) return 0;
            long nGT=tinhGiaiThua(n);
            long kGT=tinhGiaiThua(k);
            long nkGT=tinhGiaiThua(n-k);
            return nGT/(kGT*nkGT);
        }
    }
}