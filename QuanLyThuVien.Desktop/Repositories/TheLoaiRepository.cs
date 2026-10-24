
using Microsoft.Data.SqlClient;
using QuanLyThuVien.Desktop.Data;
using QuanLyThuVien.Desktop.Models;

namespace QuanLyThuVien.Desktop.Repositories
{
    public class TheLoaiRepository
    {
        public List<TheLoai> GetAll()
        {
            List<TheLoai> danhSach = new();

            using var connection = DatabaseConnection.GetConnection();

            string sql = @"
                SELECT MaTheLoai, TenTheLoai
                FROM dbo.TheLoai
                WHERE ISNULL(IsDeleted, 0) = 0
                ORDER BY TenTheLoai";

            using var command = new SqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                danhSach.Add(new TheLoai
                {
                    MaTheLoai = reader.GetInt32(0),
                    TenTheLoai = reader.GetString(1)
                });
            }

            return danhSach;
        }
    }
}
