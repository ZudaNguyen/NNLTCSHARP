/*
* CHƯƠNG TRÌNH PHÂN TÍCH THỪA SỐ NGUYÊN TỐ (CẢI TIẾN)
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập số nguyên n (n>1). Phân tích n thành các thừa số nguyên tố.
* Ý tưởng (Thuật toán cải tiến):
*   - Sử dụng vòng lặp duyệt i từ 2 đến căn bậc hai của n (i * i <= n).
*   - Nếu n chia hết cho i, dùng vòng lặp while để đếm số lần chia hết (số mũ) và thu gọn n = n / i.
*   - In ra kết quả dưới dạng cơ_số^số_mũ (nếu mũ > 1) hoặc cơ_số (nếu mũ = 1).
*   - Nếu sau vòng lặp mà n vẫn còn lớn hơn 1, thì phần còn lại chính là một số nguyên tố.
*/

using System;

namespace PhanTichThuaSo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Moi ban nhap so nguyen n: ");
            
            // Dùng int.TryParse kèm dấu ! để tránh cảnh báo CS8604 như bạn vừa xử lý ở bài trước
            if (int.TryParse(Console.ReadLine()!, out int n) && n > 1)
            {
                Console.Write($"{n} = ");
                
                bool isFirst = true;
                int tempN = n; // Biến tạm để xử lý vòng lặp chia nhỏ dần

                // Cải tiến: Chỉ lặp đến căn bậc hai của tempN
                for (int i = 2; i * i <= tempN; i++)
                {
                    if (tempN % i == 0)
                    {
                        int count = 0;
                        // Đếm số lần xuất hiện của thừa số i (số mũ)
                        while (tempN % i == 0)
                        {
                            count++;
                            tempN /= i;
                        }

                        // Xử lý in chữ " x " giữa các thừa số
                        if (!isFirst)
                        {
                            Console.Write(" x ");
                        }
                        
                        // In định dạng có số mũ nếu đếm > 1, ngược lại chỉ in số
                        if (count > 1)
                            Console.Write($"{i}^{count}");
                        else
                            Console.Write($"{i}");
                            
                        isFirst = false;
                    }
                }

                // Nếu sau khi rút gọn mà tempN vẫn lớn hơn 1 thì nó chính là thừa số nguyên tố cuối cùng
                if (tempN > 1)
                {
                    if (!isFirst)
                    {
                        Console.Write(" x ");
                    }
                    Console.Write($"{tempN}");
                }
                
                // Xuống dòng kết thúc chương trình
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Vui long nhap mot so nguyen lon hon 1.");
            }
        }
    }
}