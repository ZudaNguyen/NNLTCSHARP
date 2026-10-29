using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface INhanVienRepository
{
    Task<IReadOnlyList<NhanVien>> SearchAsync(string? search, string? trangThai, string? chucVu, CancellationToken cancellationToken);
    Task<NhanVien?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, int? excludingId, CancellationToken cancellationToken);
    Task<bool> PhoneExistsAsync(string phone, int? excludingId, CancellationToken cancellationToken);
    Task AddAsync(NhanVien employee, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<NhanVien?> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken);
    Task<IReadOnlyList<NhanVienTrangThaiLichSu>> GetStatusHistoryAsync(int id, CancellationToken cancellationToken);
}
