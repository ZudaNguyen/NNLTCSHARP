/*
 * CHƯƠNG TRÌNH TÍNH TIỀN NƯỚC TIÊU THỤ TRONG HỘ GIA ĐÌNH
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 12/09/2026
 * 
 * Phát biểu đề tài: Tính tổng số tiền nước hộ gia đình phải trả dựa trên chỉ số cũ, chỉ số mới, số người đăng ký theo bảng giá định mức lũy tiến (kèm 5% VAT và 10% phí BVMT).
 * Ý tưởng: Xác định số m3 nước tiêu thụ. Tính các mốc định mức dựa vào số người. Dùng if-else để tính tiền lũy tiến từng bậc, cuối cùng nhân với 1.15 để cộng các loại thuế phí.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập chỉ số cũ, chỉ số mới, số người
 *   3. Tính số m3 = chỉ số mới - chỉ số cũ
 *   4. Tính định mức 1 (4m3 * số người) và định mức 2 (6m3 * số người)
 *   5. So sánh số m3 tiêu thụ với định mức để tính tổng tiền theo đơn giá bậc thang
 *   6. Nhân tổng tiền với 1.15
 *   7. Xuất số tiền phải trả
 *   8. Kết thúc
 */

using System;
using MyLib;

namespace Buoi04.Bai02
{
    public class TienNuoc
    {
        public void ThucHien()
        {
            Console.Write("Moi ban nhap chi so tieu thu nuoc thang truoc (m^3): ");
            double chiSoCu = Convert.ToDouble(Console.ReadLine());

            Console.Write("Moi ban nhap chi so tieu thu nuoc thang nay (m^3): ");
            double chiSoMoi = Convert.ToDouble(Console.ReadLine());

            Console.Write("Moi ban nhap so nguoi tieu thu (da dang ky): ");
            int soNguoi = Convert.ToInt32(Console.ReadLine());

            double soM3 = chiSoMoi - chiSoCu;

            TienNuocLib lib = new TienNuocLib();
            double tongTien = lib.TinhTienNuoc(chiSoCu, chiSoMoi, soNguoi);

            Console.WriteLine($"So tien phai tra cho {soM3} m^3 tieu thu trong thang la {tongTien} D.");
        }
    }
}