using System;
using System.Globalization;

namespace MyLib
{
    public class B4B10
    {
        public static string TimDoan(double[]? arr, double k)
        {
            if (arr == null || arr.Length == 0)
            {
                return "Day so rong.";
            }

            int viTriDau_TotNhat = -1;
            int viTriCuoi_TotNhat = -1;
            int chieuDaiMax = 0;
            double saiSo = 1e-6; // Xử lý sai số thập phân

            // Duyệt điểm bắt đầu i
            for (int i = 0; i < arr.Length; i++)
            {
                double tongHienTai = 0;
                
                // Duyệt điểm kết thúc j (bắt đầu từ i)
                for (int j = i; j < arr.Length; j++)
                {
                    tongHienTai += arr[j];

                    // Nếu tổng đoạn [i, j] bằng k
                    if (Math.Abs(tongHienTai - k) < saiSo)
                    {
                        int chieuDaiHienTai = j - i + 1;
                        // Nếu chiều dài lớn hơn kỷ lục cũ thì cập nhật
                        if (chieuDaiHienTai > chieuDaiMax)
                        {
                            chieuDaiMax = chieuDaiHienTai;
                            viTriDau_TotNhat = i;
                            viTriCuoi_TotNhat = j;
                        }
                    }
                }
            }

            string kStr = k.ToString(CultureInfo.InvariantCulture);

            if (chieuDaiMax > 0)
            {
                // Trích xuất các phần tử trong đoạn để in ra
                string[] phanTu = new string[chieuDaiMax];
                for (int i = 0; i < chieuDaiMax; i++)
                {
                    phanTu[i] = arr[viTriDau_TotNhat + i].ToString(CultureInfo.InvariantCulture);
                }
                
                string chuoiPhanTu = string.Join(" ", phanTu);
                return $"Doan [{viTriDau_TotNhat}, {viTriCuoi_TotNhat}] dai nhat co tong bang {kStr}: {chuoiPhanTu}";
            }
            else
            {
                return $"Khong tim thay doan nao co tong bang {kStr}.";
            }
        }
    }
}