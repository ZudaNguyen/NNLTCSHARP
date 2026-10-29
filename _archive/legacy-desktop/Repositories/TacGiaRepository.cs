
using Microsoft.Data.SqlClient;
using QuanLyThuVien.Desktop.Data;
using QuanLyThuVien.Desktop.Models;

namespace QuanLyThuVien.Desktop.Repositories
{
    public class TacGiaRepository
    {
        public List<TacGia> GetAll()
        {
            List<TacGia> danhSach = new();

            using var connection = DatabaseConnection.GetConnection();

            string sql = @"
                SELECT MaTacGia, TenTacGia
                FROM dbo.TacGia
                WHERE ISNULL(IsDeleted, 0) = 0
                ORDER BY TenTacGia";

            using var command = new SqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                danhSach.Add(new TacGia
                {
                    MaTacGia = reader.GetInt32(0),
                    TenTacGia = reader.GetString(1)
                });
            }

            return danhSach;
        }
    }
}
