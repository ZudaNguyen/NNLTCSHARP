using System.Collections.Generic;
namespace MyLib
{
    public class Buoi7_InHinhTamGiacVaVuongLib
    {
        public static List<string> InHinh(int h)
        {
            List<string> ketqua= new List<string>();
            for(int i = 0; i < h; i++)
            {
                if (i == 0)
                {
                    string line =new string(' ',2*(h-1)) +"*";
                    ketqua.Add(line);
                }
                else if (i == h - 1)
                {
                    List<string> stars=new List<string>();
                    for(int j = 0; j < 2 * h - 1; j++)
                    {
                        stars.Add("*");
                    }
                    ketqua.Add(string.Join(" ",stars));
                }
                else
                {
                    int leadingSpaces=2*(h-1-i);
                    int innerSpaces=4*i-1;
                    string line=new string(' ',leadingSpaces)+"*"+new string(' ',innerSpaces)+"*";
                    ketqua.Add(line);
                }
            }
            return ketqua;

        }
    }
}