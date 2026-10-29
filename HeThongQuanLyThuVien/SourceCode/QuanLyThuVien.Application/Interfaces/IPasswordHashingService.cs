using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public enum PasswordVerification
{
    Failed,
    Succeeded,
    RehashRequired
}

public interface IPasswordHashingService
{
    string Hash(TaiKhoan account, string password);
    PasswordVerification Verify(TaiKhoan account, string passwordHash, string password);
}
