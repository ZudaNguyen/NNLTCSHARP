using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface INhanVienService
{
    Task<IReadOnlyList<NhanVienDto>> SearchAsync(string? search, string? trangThai, string? chucVu, CancellationToken cancellationToken);
    Task<NhanVienDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<NhanVienDto> CreateAsync(CreateNhanVienDto request, CancellationToken cancellationToken);
    Task<NhanVienDto?> UpdateAsync(int id, UpdateNhanVienDto request, CancellationToken cancellationToken);
    Task<NhanVienDto?> UpdateTrangThaiAsync(int id, string status, CancellationToken cancellationToken);
    Task<IReadOnlyList<NhanVienTrangThaiLichSuDto>?> GetStatusHistoryAsync(int id, CancellationToken cancellationToken);
}
