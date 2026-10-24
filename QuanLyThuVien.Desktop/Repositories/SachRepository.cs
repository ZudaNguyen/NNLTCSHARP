
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLyThuVien.Desktop.Data;
using QuanLyThuVien.Desktop.Models;

namespace QuanLyThuVien.Desktop.Repositories
{
    public class SachRepository
    {
        public List<Sach> GetAll()
        {
            List<Sach> danhSach = new List<Sach>();

            using var connection =
                DatabaseConnection.GetConnection();

            string sql = @"
                SELECT
                    s.MaSach,
                    s.TenSach,
                    s.ISBN,
                    s.NamXuatBan,
                    s.NhaXuatBan,
                    s.SoLuong,
                    s.SoLuongCon,
                    s.MaTheLoai,
                    ISNULL(tl.TenTheLoai, N'Chưa phân loại')
                        AS TenTheLoai,

                    ISNULL(
                        STUFF((
                            SELECT N', ' + tg.TenTacGia
                            FROM dbo.Sach_TacGia stg
                            INNER JOIN dbo.TacGia tg
                                ON stg.MaTacGia = tg.MaTacGia
                            WHERE stg.MaSach = s.MaSach
                            ORDER BY tg.TenTacGia
                            FOR XML PATH(''), TYPE
                        ).value('.', 'nvarchar(max)'),
                        1, 2, N''),
                        N''
                    ) AS TenTacGia,

                    ISNULL(s.IsDeleted, 0) AS IsDeleted

                FROM dbo.Sach s
                LEFT JOIN dbo.TheLoai tl
                    ON s.MaTheLoai = tl.MaTheLoai

                ORDER BY s.MaSach DESC";

            using var command =
                new SqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Sach sach = new Sach
                {
                    MaSach = Convert.ToInt32(
                        reader["MaSach"]),

                    TenSach = reader["TenSach"].ToString()!,

                    ISBN = reader["ISBN"] as string,

                    NamXuatBan =
                        reader["NamXuatBan"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["NamXuatBan"]),

                    NhaXuatBan =
                        reader["NhaXuatBan"] as string,

                    SoLuong = Convert.ToInt32(
                        reader["SoLuong"]),

                    SoLuongCon = Convert.ToInt32(
                        reader["SoLuongCon"]),

                    MaTheLoai =
                        reader["MaTheLoai"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["MaTheLoai"]),

                    TenTheLoai =
                        reader["TenTheLoai"].ToString()!,

                    TenTacGia =
                        reader["TenTacGia"].ToString()!,

                    IsDeleted = Convert.ToBoolean(
                        reader["IsDeleted"])
                };

                danhSach.Add(sach);
            }

            return danhSach;
        }
    }
}
