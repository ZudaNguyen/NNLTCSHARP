/*
* THƯ VIỆN TÍNH TỔNG PHẦN TỬ CỰC TRỊ
* Tác giả : Võ Minh Vỹ
* Ngày viết: 18/09/2026
*
* Phát biểu đề bài:
*   Cho dãy số thực n phần tử (n <= 500). Tính tổng các phần tử cực trị có trong dãy.
*   Một phần tử là cực trị khi nó lớn hơn hoặc nhỏ hơn 2 phần tử xung quanh nó.
*
* Ý tưởng:
*   Duyệt các phần tử từ chỉ số 1 đến n - 2 (phần tử có đủ 2 lân cận trái và phải).
*   Nếu (arr[i] > arr[i-1] && arr[i] > arr[i+1]) hoặc (arr[i] < arr[i-1] && arr[i] < arr[i+1])
*   thì cộng phần tử arr[i] vào tổng.
*
* Mã giả:
*   Hàm TinhTongCucTri(arr):
*     tong = 0
*     Lặp i từ 1 đến arr.Length - 2:
*       Nếu (arr[i] > arr[i-1] và arr[i] > arr[i+1]) hoặc (arr[i] < arr[i-1] và arr[i] < arr[i+1]):
*         tong = tong + arr[i]
*     Trả về tong
*/
using System;
using MyLib;
namespace Buoi10_BT1;
public class Program
{
    public static void Main()
    {
        Console.WriteLine("nhap n phan tu ");
        string[] input=Console.ReadLine()!.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        double [] arr =new double[input.Length];
        for(int i = 0; i < input.Length; i++)
        {
            arr[i]=double.Parse(input[i]);

        }
        double tong=Buoi10_TrungBinhCongLib.tinhTrungBinhCong(arr);
        Console.WriteLine($"Tong cac phan tu cuc tri co trong day so: {tong}.");
    }
}