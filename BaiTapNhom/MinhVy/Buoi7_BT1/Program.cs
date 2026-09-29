
// * THƯ VIỆN IN 3 HÌNH TAM GIÁC
// * Tác giả : Võ Minh Vỹ
// * Ngày viết: 17/09/2026
// *
// * Phát biểu đề bài:
// *   Nhập vào chiều cao h. In ra 3 hình tam giác rỗng với chiều cao lần lượt là h, h + 2, h + 4.
// *
// * Ý tưởng:
// *   - Với mỗi chiều cao H:
// *     + Dòng đầu (r = 0): In 2 * (H - 1) khoảng trắng + 1 dấu '*'.
// *     + Dòng giữa (0 < r < H - 1): In 2 * (H - 1 - r) khoảng trắng đầu, 1 dấu '*', (4 * r - 1) khoảng trắng giữa, 1 dấu '*'.
// *     + Dòng cuối (r = H - 1): In (2 * H - 1) dấu '*' phân tách bởi khoảng trắng.
using System;
using System.Collections.Generic;
using MyLib;
namespace Buoi7_BT1;
public class Program
{
    public static void Main()
    {
        Console.Write("hay nhap chieu cao h");
        int h = int.Parse(Console.ReadLine() ?? "0");
        int [] list=new int[]{h,h+2,h+4};
        foreach(int c in list){
            Console.WriteLine($"h={c}");
            List<string> tamGiac=Buoi7_InHinhTamGiacVaVuongLib.InHinh(c);
            foreach(string dong in tamGiac)
            {
                Console.WriteLine(dong);
            }
            
        }
    }
}