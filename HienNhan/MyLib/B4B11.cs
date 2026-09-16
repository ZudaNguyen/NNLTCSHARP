namespace MyLib
{
    public class B4B11
    {
        public static int[] DaoNguocDaySo(int[]? arr)
        {
            if (arr == null || arr.Length == 0)
            {
                return new int[0]; // Trả về mảng rỗng nếu đầu vào không hợp lệ
            }

            int n = arr.Length;
            int[] ketQua = new int[n];

            // Gán phần tử từ cuối mảng gốc lên đầu mảng mới
            for (int i = 0; i < n; i++)
            {
                ketQua[i] = arr[n - 1 - i];
            }

            return ketQua;
        }
    }
}