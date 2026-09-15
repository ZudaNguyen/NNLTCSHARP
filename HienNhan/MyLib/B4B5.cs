namespace MyLib
{
    public class B4B5
    {
        public static int TimBeNhatThoa(double a)
        {
            double sum = 0;
            int n = 0;
            
             do
            {
                n++;
                sum += 1.0 / n;
            } 
            while (sum <= a);
            
            return n;
        }
    }
}