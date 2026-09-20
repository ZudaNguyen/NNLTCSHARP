/*
* THƯ VIỆN LIỆT KÊ SỐ CHẴN TỪ PHẢI SANG TRÁI
* Tác giả : Võ Minh Vỹ
* Ngày viết: 17/09/2026
*
* Phát biểu đề bài:
*   Cho dãy số nguyên gồm n phần tử (n <= 500).
*   Liệt kê các phần tử chẵn trong mảng theo thứ tự từ phải sang trái.
*
* Ý tưởng:
*   Duyệt ngược từ chỉ số cuối mảng (n - 1) lùi dần về 0.
*   Nếu phần tử arr[i] chia hết cho 2 (arr[i] % 2 == 0) thì lưu vào danh sách kết quả.
*
* Mã giả:
*   Hàm LietKeSoChanTuPhaiSangTrai(arr):
*     Tạo danh sách kết quả result
*     Lặp i từ arr.Length - 1 giảm về 0:
*       Nếu arr[i] % 2 == 0:
*         Thêm arr[i] vào result
*     Trả về result
*/
using System;
using System.Collections.Generic;
using MyLib;
namespace Buoi9_BT1;
public class Progam
{
    public static void Main()
    {
        Console.WriteLine("moi nhap so luong phan tu");
        int n=int.Parse(Console.ReadLine()!);
        int [] arr=new int[n];
        for(int i = 0; i < n; i++)
        {
            Console.Write($"Phan tu {i}: ");
                arr[i] = int.Parse(Console.ReadLine()!);
            }

            
            List<int> dsChan = Buoi9_LietKeChanLib.lietKeSoChan(arr);

          
            Console.WriteLine("Cac phan tu chan trong mang: " + string.Join(" ", dsChan));
        }

    }
