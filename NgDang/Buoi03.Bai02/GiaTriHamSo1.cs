/*
 * CHƯƠNG TRÌNH TÍNH GIÁ TRỊ HÀM SỐ
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 11/09/2026
 * 
 * Phát biểu đề tài: Cho số thực x. Hãy tính giá trị của hàm số f1(x) và f2(x) theo các khoảng điều kiện cho trước.
 * Ý tưởng: Dùng cấu trúc rẽ nhánh if-else để kiểm tra giá trị của x nằm trong khoảng nào, sau đó áp dụng công thức toán học tương ứng.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập giá trị số thực x
 *   3. Tính f1: Nếu x <= 0 thì f1 = 0, nếu 0 < x <= 1 thì f1 = x, ngược lại f1 = x^4
 *   4. Tính f2: Nếu x <= 2 thì f2 = x^2 + 4x + 5, ngược lại f2 = 1 / (x^2 + 4x + 5)
 *   5. Xuất giá trị f1(x) và f2(x)
 *   6. Kết thúc
 */

using System;
using MyLib;

namespace Buoi03.Bai02
{
    public class GiaTriHamSo1
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so thuc x:");
            double x = Convert.ToDouble(Console.ReadLine());

            // Goi class tu MyLib
            GiaTriHamSo1Lib lib = new GiaTriHamSo1Lib();

            double f1 = lib.TinhF1(x);
            double f2 = lib.TinhF2(x);

            // In ket qua
            Console.WriteLine($"Gia tri f1({x}) = {f1}");
            Console.WriteLine($"Gia tri f2({x}) = {f2}");
        }
    }
}