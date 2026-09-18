/*
 * CHƯƠNG TRÌNH TÍNH LŨY THỪA NHANH
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 10/09/2026
 * 
 * Phát biểu đề tài: Cho số thực a. Hãy tính a^2, a^5 và a^17 chỉ dùng 6 phép nhân.
 * Ý tưởng: Sử dụng kết quả của các phép nhân trước để làm đầu vào cho phép tính sau nhằm tối ưu hóa, giảm thiểu số lần nhân.
 * Mã giả:
 *   1. Bắt đầu
 *   2. In thông báo: "Moi ban nhap so thuc a: "
 *   3. Nhập giá trị a
 *   4. Tính a2 = a * a, a4 = a2 * a2, a5 = a4 * a, a8 = a4 * a4, a16 = a8 * a8, a17 = a16 * a
 *   5. Xuất kết quả a^2, a^5, a^17 (làm tròn 2 chữ số thập phân)
 *   6. Kết thúc
 */

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