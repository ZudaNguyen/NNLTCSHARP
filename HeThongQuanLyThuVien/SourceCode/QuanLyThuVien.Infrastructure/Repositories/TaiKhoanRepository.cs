using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Entities;
using QuanLyThuVien.Infrastructure.Persistence;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Infrastructure.Repositories;

public sealed class TaiKhoanRepository(LibraryDbContext db) : ITaiKhoanRepository
{
    public Task<TaiKhoan?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken ct) =>
        db.TaiKhoan.FirstOrDefaultAsync(x => x.TenDangNhapChuanHoa == normalizedUserName, ct);

    public Task<TaiKhoan?> GetByIdAsync(int id, CancellationToken ct) =>
        db.TaiKhoan.FirstOrDefaultAsync(x => x.MaTaiKhoan == id, ct);

    public async Task<IReadOnlyList<TaiKhoan>> SearchAsync(string? search, string? role, string? status, CancellationToken ct)
    {
        var query = db.TaiKhoan.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.TenDangNhap.Contains(keyword) || x.TenHienThi.Contains(keyword));
        }
        if (!string.IsNullOrWhiteSpace(role)) query = query.Where(x => x.VaiTro == role.Trim());
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.TrangThai == status.Trim());
        return await query.OrderBy(x => x.TenDangNhapChuanHoa).Take(1000).ToListAsync(ct);
    }

    public Task<bool> UserNameExistsAsync(string normalizedUserName, int? excludingId, CancellationToken ct) =>
        db.TaiKhoan.AnyAsync(x => x.TenDangNhapChuanHoa == normalizedUserName
            && (!excludingId.HasValue || x.MaTaiKhoan != excludingId.Value), ct);

    public Task<bool> EmployeeExistsAsync(int id, CancellationToken ct) =>
        db.NhanVien.AnyAsync(x => x.MaNhanVien == id && x.TrangThai == "Hoạt động", ct);

    public Task<bool> ReaderExistsAsync(int id, CancellationToken ct) =>
        db.DocGia.AnyAsync(x => x.MaDocGia == id && x.TrangThai == "Hoạt động", ct);

    public Task<bool> HasAnyAdminAsync(CancellationToken ct) =>
        db.TaiKhoan.AnyAsync(x => x.VaiTro == AccountRoles.Admin, ct);

    public async Task AddAsync(TaiKhoan account, CancellationToken ct)
    {
        db.TaiKhoan.Add(account);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddBootstrapAdminIfNoneAsync(TaiKhoan account, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await db.TaiKhoan.AnyAsync(x => x.VaiTro == AccountRoles.Admin, ct))
        {
            db.TaiKhoan.Add(account);
            await db.SaveChangesAsync(ct);
        }
        await transaction.CommitAsync(ct);
    }

    public async Task SaveAdministrativeChangesAsync(TaiKhoan account, bool guardLastAdmin, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entry = db.Entry(account);
        var oldRole = entry.Property(x => x.VaiTro).OriginalValue;
        var oldStatus = entry.Property(x => x.TrangThai).OriginalValue;
        var leavesActiveAdmin = oldRole == AccountRoles.Admin && oldStatus == "Hoạt động"
            && (account.VaiTro != AccountRoles.Admin || account.TrangThai != "Hoạt động");
        if (guardLastAdmin && leavesActiveAdmin
            && await db.TaiKhoan.CountAsync(x => x.VaiTro == AccountRoles.Admin && x.TrangThai == "Hoạt động", ct) <= 1)
            throw new LastAdminAccountException();

        await db.SaveChangesAsync(ct);
        if (oldRole != account.VaiTro || oldStatus != account.TrangThai)
            await RevokeSessionsForAccountAsync(account.MaTaiKhoan, DateTime.UtcNow, ct);
        await transaction.CommitAsync(ct);
    }

    public async Task SaveCredentialChangesAndRevokeSessionsAsync(TaiKhoan account, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.SaveChangesAsync(ct);
        await RevokeSessionsForAccountAsync(account.MaTaiKhoan, DateTime.UtcNow, ct);
        await transaction.CommitAsync(ct);
    }

    public async Task RecordFailedLoginAsync(int accountId, int maxAttempts, TimeSpan lockoutDuration, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var account = await db.TaiKhoan.FirstOrDefaultAsync(x => x.MaTaiKhoan == accountId, ct);
        if (account is not null)
        {
            if (account.KhoaDenUtc <= now) account.KhoaDenUtc = null;
            account.SoLanDangNhapThatBai++;
            if (account.SoLanDangNhapThatBai >= maxAttempts)
            {
                account.SoLanDangNhapThatBai = 0;
                account.KhoaDenUtc = now.Add(lockoutDuration);
            }
            await db.SaveChangesAsync(ct);
        }
        await transaction.CommitAsync(ct);
    }

    public async Task ResetLoginFailuresAsync(TaiKhoan account, CancellationToken ct)
    {
        if (account.SoLanDangNhapThatBai == 0 && account.KhoaDenUtc is null) return;
        account.SoLanDangNhapThatBai = 0;
        account.KhoaDenUtc = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task CreateSessionAsync(TaiKhoanSession session, CancellationToken ct)
    {
        db.TaiKhoanSession.Add(session);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ValidatedSession?> FindValidSessionAsync(byte[] tokenHash, DateTime nowUtc, CancellationToken ct)
    {
        var found = await (from session in db.TaiKhoanSession.AsNoTracking()
                           join account in db.TaiKhoan.AsNoTracking() on session.MaTaiKhoan equals account.MaTaiKhoan
                           where session.TokenHash == tokenHash
                               && session.ThuHoiLucUtc == null
                               && session.HetHanLucUtc > nowUtc
                               && account.TrangThai == "Hoạt động"
                           select new { SessionId = session.Id, Account = account })
            .FirstOrDefaultAsync(ct);
        return found is null ? null : new ValidatedSession(found.SessionId, found.Account);
    }

    public async Task RevokeSessionAsync(Guid sessionId, int accountId, DateTime revokedAtUtc, CancellationToken ct)
    {
        await db.TaiKhoanSession
            .Where(x => x.Id == sessionId && x.MaTaiKhoan == accountId && x.ThuHoiLucUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ThuHoiLucUtc, revokedAtUtc), ct);
    }

    public async Task RevokeAllSessionsAsync(int accountId, DateTime revokedAtUtc, CancellationToken ct) =>
        await RevokeSessionsForAccountAsync(accountId, revokedAtUtc, ct);

    private async Task RevokeSessionsForAccountAsync(int accountId, DateTime revokedAtUtc, CancellationToken ct) =>
        await db.TaiKhoanSession
            .Where(x => x.MaTaiKhoan == accountId && x.ThuHoiLucUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ThuHoiLucUtc, revokedAtUtc), ct);
}

