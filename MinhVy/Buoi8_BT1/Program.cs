/*
* THƯ VIỆN TÍNH TỔ HỢP C(K, N)
* Tác giả : Võ Minh Vỹ
* Ngày viết: 17/09/2026
*
* Phát biểu đề bài:
*   Nhập vào hai số nguyên dương n, k (k <= n). Tính C_n^k = n! / (k! * (n - k)!).
*
* Ý tưởng:
*   - TinhGiaiThua(x): Tính giai thừa từ 1 đến x.
*   - TinhToHop(k, n): Áp dụng công thức n! / (k! * (n - k)!).
*
* Mã giả:
*   Hàm TinhToHop(k, n):
*     Nếu k < 0 hoặc k > n -> Trả về 0
*     nGT = TinhGiaiThua(n)
*     kGT = TinhGiaiThua(k)
*     nkGT = TinhGiaiThua(n - k)
*     Trả về nGT / (kGT * nkGT)
*/
using System;
using MyLib;
namespace Buoi8_BT1;
public class Program
{
    public static void Main()
    {
        Console.WriteLine("nhap k:");
        int k = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException("Không nhận được giá trị k."));
        Console.WriteLine("nhap n:");
        int n = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException("Không nhận được giá trị n."));
        long ketqua=Buoi8_GiaiThuaLib.tinhToHop(k,n);
        Console.WriteLine($"C({k}, {n}) = {ketqua}.");
    }
}