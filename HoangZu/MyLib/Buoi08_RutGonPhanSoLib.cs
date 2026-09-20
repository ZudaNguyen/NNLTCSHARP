using System;

namespace MyLib
{
    public static class Buoi08_RutGonPhanSoLib
    {
        // Hàm xử lý logic rút gọn phân số và trả về chuỗi kết quả
        public static string RutGon(int tuSo, int mauSo)
        {
            if (mauSo == 0)
            {
                throw new ArgumentException("Mau so phai khac 0.");
            }

            // Đưa dấu trừ lên tử số nếu mẫu số âm
            if (mauSo < 0)
            {
                tuSo = -tuSo;
                mauSo = -mauSo;
            }

            // Tìm UCLN bằng thuật toán Euclid
            int a = Math.Abs(tuSo);
            int b = Math.Abs(mauSo);
            
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            
            int ucln = a;

            // Thực hiện rút gọn
            int tuSoRutGon = tuSo / ucln;
            int mauSoRutGon = mauSo / ucln;

            // Định dạng chuỗi kết quả
            if (mauSoRutGon == 1 || tuSoRutGon == 0)
            {
                return $"{tuSoRutGon}";
            }
            else
            {
                return $"{tuSoRutGon}/{mauSoRutGon}";
            }
        }
    }
}