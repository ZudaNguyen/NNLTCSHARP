/*
* CHƯƠNG TRÌNH ĐẾM DẤU
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 11/09/2026
*
* Phát biểu đề bài: Cho dãy số thực có n phần tử (n <= 500). Đếm dãy số có bao nhiêu số dương, số âm và số không.
* Ý tưởng:
*   - Nhập số lượng phần tử n và kiểm tra điều kiện n <= 500.
*   - Khởi tạo 3 biến đếm: demAm, demKhong, demDuong bằng 0.
*   - Sử dụng vòng lặp for chạy từ 0 đến n-1 để nhập từng phần tử.
*   - Ngay trong lúc nhập, kiểm tra giá trị của phần tử để tăng biến đếm tương ứng.
*   - In ra kết quả đếm.
*
* Mã giả:
*   Nhập số nguyên n
*   Nếu (n <= 0 hoặc n > 500) thì:
*       In thông báo lỗi và kết thúc chương trình
*   Khởi tạo demAm = 0
*   Khởi tạo demKhong = 0
*   Khởi tạo demDuong = 0
*   Cho i chạy từ 0 đến n - 1 thực hiện:
*       Nhập số thực giaTri
*       Nếu (giaTri < 0) thì:
*           demAm = demAm + 1
*       Ngược lại, nếu (giaTri == 0) thì:
*           demKhong = demKhong + 1
*       Ngược lại:
*           demDuong = demDuong + 1
*   In ra kết quả demAm, demKhong và demDuong
*/

using System;
using System.Globalization;

namespace DemDau
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Moi ban nhap so luong phan tu: ");
            int n = int.Parse(Console.ReadLine()!);

            // Kiểm tra điều kiện giới hạn của n theo đề bài
            if (n <= 0 || n > 500)
            {
                Console.WriteLine("So luong phan tu phai lon hon 0 va khong vuot qua 500.");
                return;
            }

            int demAm = 0;
            int demKhong = 0;
            int demDuong = 0;

            // Vòng lặp nhập mảng và đếm trực tiếp
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Phan tu {i}: ");
                double giaTri = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

                if (giaTri < 0)
                {
                    demAm++;
                }
                else if (giaTri == 0)
                {
                    demKhong++;
                }
                else
                {
                    demDuong++;
                }
            }

            // In kết quả theo đúng định dạng
            Console.WriteLine($"Day so co {demAm} so am, {demKhong} so khong va {demDuong} so duong.");
        }
    }
}