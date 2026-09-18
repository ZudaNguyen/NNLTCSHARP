/*
* CHƯƠNG TRÌNH TÌM ĐOẠN CON CÓ TỔNG BẰNG K DÀI NHẤT
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 16/09/2026
*
* Phát biểu đề bài: Cho dãy số thực có n phần tử (n <= 500) và số k. Tìm đoạn [i, j] dài nhất có tổng bằng k.
* Ý tưởng: 
*  - Dùng 2 vòng lặp lồng nhau: vòng lặp ngoài xác định vị trí bắt đầu i, vòng lặp trong xác định vị trí kết thúc j.
*  - Cộng dồn các phần tử từ i đến j. Nếu tổng xấp xỉ bằng k (dùng epsilon để tránh sai số thực) và chiều dài đoạn (j - i + 1) lớn hơn chiều dài lớn nhất đã tìm thấy, ta cập nhật lại kết quả.
* Mã giả:
*  Begin
*      Input arr, k
*      maxLen = 0, bestI = -1, bestJ = -1
*      For i = 0 To length - 1 Do
*          sum = 0
*          For j = i To length - 1 Do
*              sum = sum + arr[j]
*              If sum == k And (j - i + 1) > maxLen Then
*                  maxLen = j - i + 1
*                  bestI = i
*                  bestJ = j
*              End If
*          End For
*      End For
*      Print result
*  End
*/

using System;
using System.Globalization;

class BT4B10
{
    static void Main()
    {
        Console.WriteLine("// Nhap day so (cac so cach nhau boi khoang trang):");
        string input = Console.ReadLine();
        
        string[] parts = input?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts != null && parts.Length > 0 && parts.Length <= 500)
        {
            double[] arr = new double[parts.Length];
            bool nhapDung = true;

            for (int i = 0; i < parts.Length; i++)
            {
                // Cho phép nhập cả dấu phẩy lẫn dấu chấm tùy máy
                if (!double.TryParse(parts[i].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out arr[i]))
                {
                    Console.WriteLine($"Phan tu '{parts[i]}' khong phai la so thuc.");
                    nhapDung = false;
                    break;
                }
            }

            if (nhapDung)
            {
                Console.WriteLine($"Day so co {arr.Length} phan tu: " + string.Join(" ", parts));
                
                Console.Write("Moi ban nhap so k: ");
                string kInput = Console.ReadLine()?.Replace(',', '.');

                if (double.TryParse(kInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double k))
                {
                    string ketQua = MyLib.B4B10.TimDoan(arr, k);
                    Console.WriteLine(ketQua);
                }
                else
                {
                    Console.WriteLine("So k khong hop le.");
                }
            }
        }
        else
        {
            Console.WriteLine("Day so khong hop le hoac vuot qua 500 phan tu.");
        }
    }
}