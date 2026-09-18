/*
 * CHƯƠNG TRÌNH ĐẾM SỐ TRÊN BIÊN MA TRẬN
 * Tác giả    : Nguyễn Phúc Hải Đăng
 * Ngày tạo  : 19/09/2026
 * 
 * Phát biểu đề tài: Cho ma trận số thực có n dòng và m cột. Viết chương trình đếm số lượng các số dương, âm, và số không nằm trên biên của ma trận.
 * Ý tưởng: Sử dụng 2 vòng lặp lồng nhau duyệt ma trận. Các phần tử biên là các phần tử thỏa mãn điều kiện thuộc dòng đầu (i=0), dòng cuối (i=n-1), cột đầu (j=0) hoặc cột cuối (j=m-1). Từ đó phân loại dấu để đếm.
 * Mã giả:
 *   1. Bắt đầu
 *   2. Nhập n, m và tạo ma trận số thực kích thước n x m
 *   3. In ma trận
 *   4. Lặp i từ 0 đến n-1, Lặp j từ 0 đến m-1
 *   5. Kiểm tra nếu [i, j] nằm trên biên
 *   6. Tại phần tử biên, nếu giá trị > 0 thì đếm dương++, < 0 thì đếm âm++, == 0 thì đếm không++
 *   7. Xuất số lượng 3 biến đếm ra màn hình
 *   8. Kết thúc
 */

using System;
using MyLib;

namespace Buoi12.Bai02
{
    public class DemSoTrenBienMang2C
    {
        public void ThucHien()
        {
            Console.WriteLine("// Nhap ma tran A");
            Console.Write("Nhap so dong n: ");
            int n = Convert.ToInt32(Console.ReadLine());
            Console.Write("Nhap so cot m: ");
            int m = Convert.ToInt32(Console.ReadLine());

            if (n <= 0 || m <= 0 || n > 100 || m > 100)
            {
                Console.WriteLine("Kich thuoc ma tran khong hop le.");
                return;
            }

            double[,] matrix = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Console.Write($"Nhap phan tu [{i}, {j}]: ");
                    matrix[i, j] = Convert.ToDouble(Console.ReadLine());
                }
            }

            Console.WriteLine($"\nMang co {n} dong x {m} cot:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Console.Write($"{matrix[i, j], 6} ");
                }
                Console.WriteLine();
            }

            DemSoTrenBienMang2CLib lib = new DemSoTrenBienMang2CLib();
            var ketQua = lib.Dem(matrix);

            Console.WriteLine($"Tren duong bien co {ketQua.pos} so duong, {ketQua.neg} so am va {ketQua.zero} so khong.");
        }
    }
}