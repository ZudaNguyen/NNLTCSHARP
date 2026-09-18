using System.Collections.Generic;

namespace MyLib
{
    public class B4B8
    {
        // thống kê số lần xuất hiện của các chữ số từ 0 đến 9
        public static string ThongKeChuSo(int n)
        {
            if (n <= 0)
            {
                return "Vui long nhap so nguyen duong.";
            }

            List<string> ketQua = new List<string>();

            // Duyệt kiểm tra từng chữ số từ 0 đến 9
            for (int i = 0; i <= 9; i++)
            {
                int soLan = DemMotChuSo(n, i);
                if (soLan > 0)
                {
                    ketQua.Add($"Chu so {i} xuat hien {soLan} lan.");
                }
            }

            // Nối các dòng lại với nhau bằng ký tự xuống dòng
            return string.Join("\n", ketQua);
        }

        // đếm số lần xuất hiện của chữ số k trong số n
        private static int DemMotChuSo(int n, int k)
        {
            int count = 0;
            // Sao chép n sang biến tạm để không làm thay đổi n (dù truyền theo tham trị nhưng giữ thói quen tốt)
            int temp = n; 
            
            while (temp > 0)
            {
                int chuSoCuoi = temp % 10;
                if (chuSoCuoi == k)
                {
                    count++;
                }
                temp /= 10; // Cắt bỏ chữ số cuối
            }
            return count;
        }
    }
}