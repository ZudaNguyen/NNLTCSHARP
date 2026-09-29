namespace MyLib
{
    public class TongChanLeMang2CLib
    {
        public static (long tongChan, long tongLe) TinhTongChanLe(int[,] matrix)
        {
            if (matrix == null) return (0, 0);

            long tongChan = 0;
            long tongLe = 0;

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int val = matrix[r, c];
                    if (val % 2 == 0)
                    {
                        tongChan += val;
                    }
                    else
                    {
                        tongLe += val;
                    }
                }
            }

            return (tongChan, tongLe);
        }
    }
}