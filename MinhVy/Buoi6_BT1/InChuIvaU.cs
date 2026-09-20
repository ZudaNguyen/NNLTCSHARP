// * THƯ VIỆN IN HÌNH CHỮ I VÀ CHỮ U
// * Tác giả : Võ Minh Vỹ
// * Ngày viết: 16/09/2026
// *
// * Phát biểu đề bài:
// *   Nhập vào chiều cao h. In chữ I và chữ U đứng cạnh nhau tương ứng với chiều cao h.
// *
// * Ý tưởng:
// *   - Ghép từng dòng của chữ I và chữ U trên cùng một hàng.
// *   - Chữ I: Hàng đầu và hàng cuối in h dấu '*'. Các hàng giữa in 1 dấu '*' ở vị trí trung tâm h / 2.
// *   - Chữ U: Hàng cuối in h dấu '*'. Các hàng trên in '*' ở cột đầu (0) và cột cuối (h - 1).
// *   - Sử dụng khoảng cách 3 dấu cách giữa 2 chữ.

using System;
using System.Collections.Generic;
using MyLib;
namespace Buoi6_BT1
{
    public class InChuIvaU
    {
        public static void Main()
        {
            Console.WriteLine("moi ban nhap chieu cao h:");
            int h = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException("Không nhận được chiều cao."));
            Console.WriteLine($"Chu I va U ung voi h = {h} la:");
            List<String> hinh = InChuIvaULib.TaoHinhChuIVaU(h);
            foreach(string doing in hinh )
            {
                Console.WriteLine(doing);
            }
        }
    }
}