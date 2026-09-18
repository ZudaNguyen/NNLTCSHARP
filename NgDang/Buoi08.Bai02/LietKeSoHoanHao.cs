/*
 * CHƯƠNG TRÌNH LIỆT KÊ SỐ HOÀN HẢO GIẢM DẦN
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 15/09/2026
 * 
 * Phát biểu đề tài: Nhập số nguyên dương n. Hãy liệt kê các số hoàn hảo trong đoạn [1, n] theo thứ tự giảm dần.
 * Ý tưởng: Chạy vòng lặp ngược từ n lùi về 1 để tự động đảm bảo tính chất giảm dần. Tại mỗi bước lặp, kiểm tra nếu i là số hoàn hảo thì đưa vào kết quả.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập số nguyên dương n
 *   3. Lặp i từ n lùi về 1
 *   4. Gọi hàm kiểm tra i có phải là số hoàn hảo không
 *   5. Nếu đúng, thêm i vào danh sách kết quả
 *   6. Ghép danh sách bằng dấu phẩy và in ra màn hình
 *   7. Kết thúc
 */

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