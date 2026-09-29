namespace MyLib
{
    public class DemSoTrenBienMang2CLib
    {
        public (int pos, int neg, int zero) Dem(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int pos = 0, neg = 0, zero = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == 0 || i == rows - 1 || j == 0 || j == cols - 1)
                    {
                        if (matrix[i, j] > 0) pos++;
                        else if (matrix[i, j] < 0) neg++;
                        else zero++;
                    }
                }
            }
            return (pos, neg, zero);
        }
    }
}