/*
 * CHƯƠNG TRÌNH ĐẾM CẶP PHẦN TỬ KỀ NHAU TRÁI DẤU
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 17/09/2026
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

namespace Buoi10.Bai02
{
    public class KeNhauTraiDau
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap so luong phan tu n (<= 500): ");
            int n = Convert.ToInt32(Console.ReadLine());

            if (n <= 0 || n > 500)
            {
                Console.WriteLine("So luong phan tu khong hop le.");
                return;
            }

            double[] arr = new double[n];
            Console.WriteLine("// Nhap day so");
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Phan tu {i}: ");
                arr[i] = Convert.ToDouble(Console.ReadLine());
            }

            Console.Write($"Day so co {n} phan tu: ");
            Console.WriteLine(string.Join(", ", arr));

            KeNhauTraiDauLib lib = new KeNhauTraiDauLib();
            int ketQua = lib.DemCapTraiDau(arr);

            Console.WriteLine($"So luong cac phan tu ke nhau ma trai dau: {ketQua}.");
        }
    }
}