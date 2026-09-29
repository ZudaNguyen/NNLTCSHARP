using System.Collections.Generic;

namespace MyLib
{
    public static class Buoi06_LietKeSoHoanHaoLib
    {
        // Ham xu ly logic, tach biet khoi giao dien Console de de kiem thu (unit test)
        public static string LietKeSoHoanHaoGiamDan(int n)
        {
            if (n < 1)
            {
                return "So n khong hop le, vui long nhap so nguyen duong.";
            }

            List<int> dsSoHoanHao = new List<int>();
            for (int i = 1; i <= n; i++)
            {
                if (LaSoHoanHao(i))
                {
                    dsSoHoanHao.Add(i);
                }
            }

            if (dsSoHoanHao.Count == 0)
            {
                return $"Khong co so hoan hao nao trong doan [1, {n}].";
            }

            // Danh sach dang duoc them theo thu tu tang dan, dao lai de duoc giam dan
            dsSoHoanHao.Reverse();

            string chuoiKetQua = string.Join(" ", dsSoHoanHao);
            return $"Cac so hoan hao trong doan [1, {n}]: {chuoiKetQua}";
        }

        // So hoan hao: tong cac uoc so thuc su (khong ke chinh no) bang chinh no
        private static bool LaSoHoanHao(int soCanKiemTra)
        {
            if (soCanKiemTra <= 1)
            {
                return false;
            }

            int tongUocSo = 0;
            for (int i = 1; i <= soCanKiemTra / 2; i++)
            {
                if (soCanKiemTra % i == 0)
                {
                    tongUocSo += i;
                }
            }

            return tongUocSo == soCanKiemTra;
        }
    }
}
