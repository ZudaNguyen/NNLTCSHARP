/*
 * CHƯƠNG TRÌNH TÌM GIÁ TRỊ CHẴN ĐẦU TIÊN VÀ CUỐI CÙNG
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 16/09/2026
 * 
 * Phát biểu đề tài: Cho dãy số nguyên n phần tử. Hãy tìm giá trị chẵn dương đầu tiên và cuối cùng của dãy số đó.
 * Ý tưởng: Duyệt mảng một chiều từ đầu đến cuối. Khi phát hiện một phần tử thỏa mãn (> 0 và chẵn), ghi nhận nó làm vị trí đầu tiên (nếu chưa có), sau đó liên tục ghi đè nó làm vị trí cuối cùng cho đến khi hết mảng.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập mảng số nguyên n phần tử
 *   3. Khởi tạo viTriDau = -1, viTriCuoi = -1
 *   4. Duyệt mảng từ 0 đến n-1
 *   5. Nếu phần tử tại i là chẵn và lớn hơn 0:
 *      5a. Nếu viTriDau == -1, gán viTriDau = i
 *      5b. Gán viTriCuoi = i
 *   6. In ra màn hình vị trí và giá trị tương ứng
 *   7. Kết thúc
 */

using System;
using MyLib;

namespace Buoi09.Bai02
{
    public class TimKiemGiaTriChan
    {
        public void ThucHien()
        {
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

            TimKiemGiaTriChanLib lib = new TimKiemGiaTriChanLib();
            var kq = lib.TimChanDauCuoi(arr);

            if (kq.firstIndex != -1)
            {
                Console.WriteLine($"Gia tri chan dau tien o vi tri {kq.firstIndex} co gia tri {kq.firstValue}.");
                Console.WriteLine($"Gia tri chan cuoi cung o vi tri {kq.lastIndex} co gia tri {kq.lastValue}.");
            }
            else
            {
                Console.WriteLine("Khong co gia tri chan duong nao trong mang.");
            }
        }
    }
}