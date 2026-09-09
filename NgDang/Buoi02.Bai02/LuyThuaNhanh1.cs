using System;
using MyLib;

namespace Buoi02.Bai02
{
    public class LuyThuaNhanh1
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so thuc a: ");
            double a = Convert.ToDouble(Console.ReadLine());

            // Gọi thư viện
            LuyThuaNhanhLib lib = new LuyThuaNhanhLib();
            
            // Nhận kết quả chứa cả 3 giá trị
            var ketQua = lib.TinhLuyThua(a);

            // In ra kết quả (Dùng định dạng :0.00 để ra 2 số thập phân giống ví dụ ?.??)
            Console.WriteLine($"Ket qua: {a}^2={ketQua.a2:0.00}, {a}^5={ketQua.a5:0.00}, {a}^17={ketQua.a17:0.00}.");
        }
    }
}