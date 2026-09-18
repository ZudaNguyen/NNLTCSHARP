/*
 * CHƯƠNG TRÌNH TÍNH GIÁ TRỊ BIỂU THỨC S1 VÀ S2
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 12/09/2026
 * 
 * Phát biểu đề tài: Nhập vào số nguyên dương n. Hãy tính giá trị của chuỗi tổng phân số S1 (cộng dồn) và S2 (tổng đan dấu cộng/trừ).
 * Ý tưởng: Dùng vòng lặp for. Đối với S2, lặp ngược từ n về 1 và sử dụng một biến sign để tự động đảo dấu (+ thành - và ngược lại) sau mỗi lần lặp.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập số nguyên dương n
 *   3. Khởi tạo S1 = 0, S2 = 0, sign = 1
 *   4. Lặp i từ 1 đến n: S1 += 1.0 / i
 *   5. Lặp i từ n lùi về 1: S2 += sign * (1.0 / i), đổi dấu sign = -sign
 *   6. Xuất S1 và S2
 *   7. Kết thúc
 */

using System;
using MyLib;

namespace Buoi05.Bai02
{
    public class GiaTriBieuThuc
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so nguyen n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            if (n <= 0)
            {
                Console.WriteLine("Vui long nhap so nguyen duong lon hon 0.");
                return;
            }

            GiaTriBieuThucLib lib = new GiaTriBieuThucLib();
            double s1 = lib.TinhS1(n);
            double s2 = lib.TinhS2(n);

            Console.WriteLine($"S1(n={n}) = {s1}");
            Console.WriteLine($"S2(n={n}) = {s2}");
        }
    }
}