using System.Collections.Generic;
namespace MyLib
{
    public class InChuIvaULib
    {
        public static List<String> TaoHinhChuIVaU(int h)
        {
            List<string> ketqua=new List<string>();
            int midI=h/2;
            int widthI=h*2-1;
           for (int r = 0; r < h; r++)
            {
                // Xây dựng chữ I
                string iStr = "";
                for (int j = 0; j < h; j++)
                {
                    if (r == 0 || r == h - 1)
                    {
                        iStr += (j == h - 1) ? "*" : "* ";
                    }
                    else
                    {
                        if (j == midI) iStr += "*";
                        else if (j < midI) iStr += "  ";
                    }
                }
                iStr = iStr.PadRight(widthI);

                // Khoảng cách giữa I và U
                string gap = "   ";

                // Xây dựng chữ U
                string uStr = "";
                for (int j = 0; j < h; j++)
                {
                    if (r == h - 1)
                    {
                        uStr += (j == h - 1) ? "*" : "* ";
                    }
                    else
                    {
                        if (j == 0) uStr += "* ";
                        else if (j == h - 1) uStr += "*";
                        else uStr += "  ";
                    }
                }

                ketqua.Add(iStr + gap + uStr);
            }

            return ketqua;
        }
    }
}