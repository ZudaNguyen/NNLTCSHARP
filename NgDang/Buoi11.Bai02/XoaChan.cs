/*
 * CHƯƠNG TRÌNH ĐẾM CẶP PHẦN TỬ KỀ NHAU TRÁI DẤU
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 18/09/2026
 * 
 * Phát biểu đề tài: Cho dãy số thực có n phần tử. Xác định số lượng các phần tử kề nhau mà cả hai đều trái dấu.
 * Ý tưởng: Đặc điểm của 2 số trái dấu là tích của chúng luôn nhỏ hơn 0. Duyệt mảng từ phần tử đầu tiên đến phần tử áp chót, so sánh A[i] * A[i+1] < 0 để tăng biến đếm.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập mảng số thực n phần tử
 *   3. In mảng vừa nhập
 *   4. Khởi tạo biến đếm count = 0
 *   5. Lặp i từ 0 đến n-2 (phần tử kế cuối)
 *   6. Nếu phần tử tại i nhân phần tử tại i+1 < 0 thì count = count + 1
 *   7. Xuất kết quả count
 *   8. Kết thúc
 */

using System;
using MyLib;

namespace Buoi11.Bai02
{
    public class XoaChan
    {
        public void ThucHien()
        {
            Console.WriteLine("// Nhap day so");
            Console.Write("Moi ban nhap so luong phan tu: ");
            int n = Convert.ToInt32(Console.ReadLine());

            if (n <= 0 || n > 500)
            {
                Console.WriteLine("So luong phan tu khong hop le.");
                return;
            }

            int[] arr = new int[n];
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Phan tu {i}: ");
                arr[i] = Convert.ToInt32(Console.ReadLine());
            }

            Console.WriteLine("* Mang truoc xoa:");
            Console.WriteLine($"Day so co {arr.Length} phan tu: {string.Join(" ", arr)}");

            XoaChanLib lib = new XoaChanLib();
            int[] mangSauKhiXoa = lib.XoaPhanTuChan(arr);

            Console.WriteLine("* Mang sau khi xoa: ");
            Console.WriteLine($"Day so co {mangSauKhiXoa.Length} phan tu: {string.Join(" ", mangSauKhiXoa)}");
        }
    }
}