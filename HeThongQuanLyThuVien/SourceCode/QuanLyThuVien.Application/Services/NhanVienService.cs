using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class DuplicateNhanVienDataException(string field)
    : Exception($"Đã có nhân viên sử dụng {field} này.");

public sealed class NhanVienService(INhanVienRepository repository) : INhanVienService
{
    public async Task<IReadOnlyList<NhanVienDto>> SearchAsync(string? search, string? status, string? jobTitle, CancellationToken ct) =>
        (await repository.SearchAsync(search, status, jobTitle, ct)).Select(Map).ToList();

    public async Task<NhanVienDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var employee = await repository.GetByIdAsync(id, ct);
        return employee is null ? null : Map(employee);
    }

    public async Task<NhanVienDto> CreateAsync(CreateNhanVienDto request, CancellationToken ct)
    {
        var phone = Clean(request.SoDienThoai);
        var email = Clean(request.Email);
        await EnsureUniqueAsync(email, phone, null, ct);
        var employee = new NhanVien
        {
            HoTen = request.HoTen.Trim(),
            SoDienThoai = phone,
            Email = email,
            ChucVu = request.ChucVu.Trim(),
            TrangThai = "Hoạt động"
        };
        await repository.AddAsync(employee, ct);
        return Map(employee);
    }

    public async Task<NhanVienDto?> UpdateAsync(int id, UpdateNhanVienDto request, CancellationToken ct)
    {
        var employee = await repository.GetByIdAsync(id, ct);
        if (employee is null) return null;
        var phone = Clean(request.SoDienThoai);
        var email = Clean(request.Email);
        await EnsureUniqueAsync(email, phone, id, ct);
        employee.HoTen = request.HoTen.Trim();
        employee.SoDienThoai = phone;
        employee.Email = email;
        employee.ChucVu = request.ChucVu.Trim();
        await repository.SaveChangesAsync(ct);
        return Map(employee);
    }

    public async Task<NhanVienDto?> UpdateTrangThaiAsync(int id, string status, CancellationToken ct)
    {
        var employee = await repository.UpdateStatusAsync(id, status.Trim(), ct);
        if (employee is null) return null;
        return Map(employee);
    }

    public async Task<IReadOnlyList<NhanVienTrangThaiLichSuDto>?> GetStatusHistoryAsync(int id, CancellationToken ct)
    {
        if (await repository.GetByIdAsync(id, ct) is null) return null;
        return (await repository.GetStatusHistoryAsync(id, ct)).Select(x => new NhanVienTrangThaiLichSuDto
        {
            Id = x.Id,
            MaNhanVien = x.MaNhanVien,
            TrangThaiCu = x.TrangThaiCu,
            TrangThaiMoi = x.TrangThaiMoi,
            ThayDoiLuc = x.ThayDoiLuc
        }).ToList();
    }

    private async Task EnsureUniqueAsync(string? email, string? phone, int? excludingId, CancellationToken ct)
    {
        if (email is not null && await repository.EmailExistsAsync(email, excludingId, ct))
            throw new DuplicateNhanVienDataException("email");
        if (phone is not null && await repository.PhoneExistsAsync(phone, excludingId, ct))
            throw new DuplicateNhanVienDataException("số điện thoại");
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static NhanVienDto Map(NhanVien employee) => new()
    {
        MaNhanVien = employee.MaNhanVien,
        HoTen = employee.HoTen,
        SoDienThoai = employee.SoDienThoai,
        Email = employee.Email,
        ChucVu = employee.ChucVu,
        TrangThai = employee.TrangThai,
        NgayTao = employee.NgayTao
    };
}
