
namespace MyLib
{   
    public class Buoi3_MaxMin5So
    {
        public static double timMax(double a,double b,double c,double d,double e)
        {
           return Math.Max(a, Math.Max(b, Math.Max(c, Math.Max(d, e))));
        }
        public static double timMin(double a,double b,double c,double d,double e)
        {
            return Math.Min(a, Math.Min(b, Math.Min(c, Math.Min(d, e))));
        }
    }
    
}