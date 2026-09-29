/*
 * CHƯƠNG TRÌNH KIỂM TRA SỐ HOÀN HẢO
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 14/09/2026
 * 
 * Phát biểu đề tài: Nhập số tự nhiên n. Kiểm tra n có phải là số hoàn hảo hay không (tổng các ước thực sự bằng chính nó), nếu có hãy in ra phép cộng các ước.
 * Ý tưởng: Dùng vòng lặp duyệt từ 1 đến n/2. Nếu n chia hết cho i thì đó là ước số, lưu vào danh sách và cộng dồn. Cuối cùng so sánh tổng đó với n.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập số n
 *   3. Khởi tạo tổng = 0, tạo danh sách chứa ước số
 *   4. Lặp i từ 1 đến n/2: Nếu n % i == 0 thì thêm i vào danh sách và cộng i vào tổng
 *   5. Nếu tổng == n: Nối chuỗi danh sách ước số và xuất thông báo là số hoàn hảo
 *   6. Ngược lại: Xuất thông báo không phải là số hoàn hảo
 *   7. Kết thúc
 */

using System;
using MyLib;

namespace Buoi07.Bai02
{
    public class KiemTraSoHoanHao
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap n: ");
            int n = Convert.ToInt32(Console.ReadLine());

            KiemTraSoHoanHaoLib lib = new KiemTraSoHoanHaoLib();
            var ketQua = lib.KiemTra(n);

            if (ketQua.IsPerfect)
            {
                string chuoiUocSo = string.Join(" + ", ketQua.Divisors);
                Console.WriteLine($"{n} = {chuoiUocSo} la so hoan hao.");
            }
            else
            {
                Console.WriteLine($"{n} khong phai la so hoan hao.");
            }
        }
    }
}