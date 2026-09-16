using System;
using MyLib;

namespace Buoi08.Bai02
{
    public class LietKeSoHoanHao
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            LietKeSoHoanHaoLib lib = new LietKeSoHoanHaoLib();
            var danhSach = lib.TimSoHoanHaoGiamDan(n);

            string chuoiKetQua = string.Join(", ", danhSach);

            Console.WriteLine($"Cac so hoan hao trong doan [1, {n}]: {chuoiKetQua}");
        }
    }
}