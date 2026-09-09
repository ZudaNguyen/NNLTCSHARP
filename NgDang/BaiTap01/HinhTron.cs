/*
 * CHƯƠNG TRÌNH TÍNH DIỆN TÍCH VÀ CHU VI HÌNH TRÒN
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo   : 08/09/2026
 *
 * Phát biểu đề tài: Nhập vào bán kính R của một hình tròn. Hãy tính diện tích và chu vi hình tròn đó theo công thức S = 3.14xR^2, P = 2x3.14xR. In kết quả với 1 số lẻ thập phân.
 * Ý tưởng: Khai báo biến R kiểu số thực, nhận dữ liệu nhập từ bàn phím. Áp dụng công thức toán học để tính S và P, sau đó dùng định dạng {value:F1} để in ra màn hình.
 * Mã giả: 
 *   1. Bắt đầu
 *   2. In thông báo: "Nhap ban kinh R: "
 *   3. Nhập giá trị R
 *   4. Tính S = 3.14 * R * R
 *   5. Tính P = 2 * 3.14 * R
 *   6. Xuất S và P (làm tròn 1 chữ số thập phân)
 *   7. Kết thúc
 */

using System;
using MyLib;

namespace BaiTap01
{
    public class HinhTron
    {
        // Tạo một phương thức (hàm) chứa logic giải bài tập
        public void ThucHienNhapXuat()
        {
            Console.Write("Nhap ban kinh R: ");
            double R = Convert.ToDouble(Console.ReadLine());

            HinhTronLib thuVienHinhTron = new HinhTronLib();

            double S = thuVienHinhTron.TinhDienTich(R);
            double P = thuVienHinhTron.TinhChuVi(R);

            Console.WriteLine($"Dien tich S = {S:F1}");
            Console.WriteLine($"Chu vi P = {P:F1}");
        }
    }
}