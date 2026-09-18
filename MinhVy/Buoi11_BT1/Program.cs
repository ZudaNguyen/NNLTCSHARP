/*
* THƯ VIỆN SẮP XẾP CHẴN LẺ
* Tác giả : Võ Minh Vỹ
* Ngày viết: 18/09/2026
*
* Phát biểu đề bài:
*   Cho dãy n số nguyên (n <= 500). Sắp xếp các số chẵn ở phía trước tăng dần,
*   các số lẻ ở phía sau giảm dần.
*
* Ý tưởng:
*   - Tách mảng đầu vào thành 2 danh sách: dsChan và dsLe.
*   - Sắp xếp dsChan theo thứ tự tăng dần (Sort).
*   - Sắp xếp dsLe theo thứ tự giảm dần (Sort với SoSanhGiamban).
*   - Nối 2 danh sách lại thành mảng kết quả duy nhất.
*
* Mã giả:
*   Hàm SapXepChanLe(arr):
*     Tạo dsChan, dsLe
*     Duyệt từng phần tử x trong arr:
*       Nếu x % 2 == 0 -> Thêm vào dsChan
*       Ngược lại -> Thêm vào dsLe
*     Sắp xếp dsChan tăng dần
*     Sắp xếp dsLe giảm dần
*     Nối dsChan và dsLe thành mảng kết quả và trả về
*/
using System;
using MyLib;

namespace Buoi11_BT1
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("// Nhap day so");
            Console.Write("Moi ban nhap cac phan tu (cach nhau bang khoang trang): ");
            string[] input = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int[] arr = new int[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                arr[i] = int.Parse(input[i]);
            }

            Console.WriteLine("* Mang truoc khi sap xep:");
            Console.WriteLine($"Day so co {arr.Length} phan tu: " + string.Join(" ", arr));

            // Gọi logic xử lý từ MyLib
            int[] ketQua = SapChanLeLib.SapXepChanLe(arr);

            Console.WriteLine("* Mang sau khi sap xep:");
            Console.WriteLine($"Day so co {ketQua.Length} phan tu: " + string.Join(" ", ketQua));
        }
    }
}