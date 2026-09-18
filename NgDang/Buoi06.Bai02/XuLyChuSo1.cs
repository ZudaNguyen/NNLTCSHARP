/*
 * CHƯƠNG TRÌNH XỬ LÝ CHỮ SỐ
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 13/09/2026
 * 
 * Phát biểu đề tài: Nhập số nguyên n. (a) Tìm chữ số thứ k tính từ trái sang, (b) Tính tổng các chữ số lẻ, (c) Tìm chữ số lẻ nhỏ nhất và lớn nhất.
 * Ý tưởng: Chuyển số thành chuỗi để xác định ký tự thứ k. Sử dụng vòng lặp while kết hợp phép chia lấy dư (% 10) và phép chia nguyên (/ 10) để bóc tách và kiểm tra từng chữ số từ phải sang trái.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập số nguyên n và vị trí k
 *   3. Chuyển n thành chuỗi để lấy ký tự ở vị trí k - 1
 *   4. Dùng vòng lặp while để tách từng chữ số của n
 *   5. Kiểm tra nếu chữ số là lẻ: cộng dồn vào tổng, cập nhật min, max
 *   6. Xuất chữ số vị trí k, tổng chữ số lẻ, và chữ số lẻ min/max
 *   7. Kết thúc
 */

using System;
using MyLib;

namespace Buoi06.Bai02
{
    public class XuLyChuSo1
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so nguyen n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            Console.Write("Moi ban nhap vi tri k: ");
            int k = Convert.ToInt32(Console.ReadLine());

            XuLyChuSo1Lib lib = new XuLyChuSo1Lib();

            int chuSoK = lib.TimChuSoThuK(n, k);
            if (chuSoK != -1)
            {
                Console.WriteLine($"Chu so o vi tri {k} (tinh tu trai) cua {n} la {chuSoK}.");
            }
            else
            {
                Console.WriteLine($"Khong tim thay chu so o vi tri {k}");
            }

            int tongLe = lib.TongChuSoLe(n);
            Console.WriteLine($"Tong cac chu so le cua {n} la {tongLe}.");

            var minMaxLe = lib.TimChuSoLeMinMax(n);
            if (minMaxLe.min != -1)
            {
                Console.WriteLine($"{n} co chu so le nho nhat la {minMaxLe.min} va lon nhat la {minMaxLe.max}.");
            }
            else
            {
                Console.WriteLine($"{n} khong co chu so le.");
            }
        }    
    }
}