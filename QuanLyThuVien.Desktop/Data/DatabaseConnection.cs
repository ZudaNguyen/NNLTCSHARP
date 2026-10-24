
using System;
using Microsoft.Data.SqlClient;

namespace QuanLyThuVien.Desktop.Data
{
    public static class DatabaseConnection
    {
        private static readonly string connectionString =
            @"Server=NGVHNNA\SQLEXPRESS;
              Database=QuanLyThuVienDB;
              Integrated Security=True;
              Encrypt=True;
              TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static string TestConnection()
        {
            try
            {
                using var connection = GetConnection();

                connection.Open();

                using var command = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.Sach",
                    connection
                );

                int count = Convert.ToInt32(
                    command.ExecuteScalar()
                );

                return $"Kết nối SQL Server thành công!\nSố sách: {count}";
            }
            catch (Exception ex)
            {
                return $"Kết nối thất bại!\n{ex.Message}";
            }
        }
    }
}
