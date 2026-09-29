/*
* THƯ VIỆN TÍNH GIAI THỪA
* Tác giả : Võ Minh Vỹ
* Ngày viết: 16/09/2026
*
* Phát biểu đề bài:
*   Tính n! = 1.2.3...n với 1 <= n <= 12 và tạo chuỗi hiển thị biểu thức nhân.
*
* Ý tưởng:
*   - TinhGiaiThua: Dùng vòng lặp for nhân tích lũy từ 1 đến n.
*   - TaoChuoiTich: Dùng string.Join để tạo chuỗi nối các số bằng dấu chấm "." (vd: "1.2.3.4.5").
*
* Mã giả:
*   Hàm TinhGiaiThua(n):
*     gt = 1
*     Lặp i từ 1 đến n: gt = gt * i
*     Trả về gt
*
*/
using System;
using MyLib;
namespace Buoi5_BT1
{
    public class giaiThua
    {
        public static void Main()
        {
            Console.WriteLine("nhap so nguyen n:");
            int n = int.Parse(Console.ReadLine() ?? "0");
            
            long ketqua=Buoi5_GiaiThua.TinhGiaiThua(n);
          Console.WriteLine("ket qua="+ketqua);
            
        }
    }
}
