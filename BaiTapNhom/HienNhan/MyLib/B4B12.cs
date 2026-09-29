using System.Collections.Generic;

namespace MyLib
{
    public class B4B12
    {
        public static int[] DongTangDan(int[,]? maTran)
        {
            if (maTran == null || maTran.Length == 0)
            {
                return new int[0];
            }

            int soDong = maTran.GetLength(0);
            int soCot = maTran.GetLength(1);
            List<int> ketQua = new List<int>();

            // Duyệt qua từng dòng
            for (int i = 0; i < soDong; i++)
            {
                bool laDayTang = true;
                
                // Duyệt qua các cột của dòng hiện tại (bắt đầu từ cột 1 để so sánh với cột 0)
                for (int j = 1; j < soCot; j++)
                {
                    // Nếu phần tử sau nhỏ hơn hoặc bằng phần tử trước -> Không phải dãy tăng ngặt
                    if (maTran[i, j] <= maTran[i, j - 1])
                    {
                        laDayTang = false;
                        break;
                    }
                }

                // Nếu kiểm tra xong mà vẫn là dãy tăng thì lưu lại chỉ số dòng
                if (laDayTang)
                {
                    ketQua.Add(i);
                }
            }

            return ketQua.ToArray();
        }
    }
}