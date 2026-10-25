using Microsoft.AspNetCore.Identity;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Api.Services;

public sealed class IdentityPasswordHashingService(IPasswordHasher<TaiKhoan> passwordHasher) : IPasswordHashingService
{
    public string Hash(TaiKhoan account, string password) => passwordHasher.HashPassword(account, password);

    public PasswordVerification Verify(TaiKhoan account, string passwordHash, string password) =>
        passwordHasher.VerifyHashedPassword(account, passwordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordVerification.Succeeded,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerification.RehashRequired,
            _ => PasswordVerification.Failed
        };
}
