using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class DuplicateDocGiaEmailException()
    : Exception("Email này đã được đăng ký cho một độc giả khác.");

public sealed class DocGiaService(IDocGiaRepository repository) : IDocGiaService
{
    public async Task<IReadOnlyList<DocGiaDto>> SearchAsync(
        string? search, string? trangThai, CancellationToken cancellationToken)
    {
        var entities = await repository.SearchAsync(search, trangThai, cancellationToken);
        return entities.Select(Map).ToList();
    }

    public async Task<DocGiaDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<DocGiaDto> CreateAsync(CreateDocGiaDto request, CancellationToken cancellationToken)
    {
        var email = Clean(request.Email);
        await EnsureUniqueEmailAsync(email, null, cancellationToken);
        var entity = new DocGia
        {
            HoTen = request.HoTen.Trim(),
            NgaySinh = request.NgaySinh.Date,
            GioiTinh = request.GioiTinh.Trim(),
            SoDienThoai = request.SoDienThoai.Trim(),
            Email = email,
            DiaChi = Clean(request.DiaChi),
            NgayDangKy = DateTime.Today,
            TrangThai = request.TrangThai.Trim()
        };
        await repository.AddAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<DocGiaDto?> UpdateAsync(int id, UpdateDocGiaDto request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return null;

        var email = Clean(request.Email);
        await EnsureUniqueEmailAsync(email, id, cancellationToken);
        entity.HoTen = request.HoTen.Trim();
        entity.NgaySinh = request.NgaySinh.Date;
        entity.GioiTinh = request.GioiTinh.Trim();
        entity.SoDienThoai = request.SoDienThoai.Trim();
        entity.Email = email;
        entity.DiaChi = Clean(request.DiaChi);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<DocGiaDto?> UpdateTrangThaiAsync(int id, string trangThai, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return null;
        entity.TrangThai = trangThai.Trim();
        await repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private async Task EnsureUniqueEmailAsync(string? email, int? excludingId, CancellationToken cancellationToken)
    {
        if (email is not null && await repository.EmailExistsAsync(email, excludingId, cancellationToken))
            throw new DuplicateDocGiaEmailException();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DocGiaDto Map(DocGia entity) => new()
    {
        MaDocGia = entity.MaDocGia,
        HoTen = entity.HoTen,
        NgaySinh = entity.NgaySinh,
        GioiTinh = entity.GioiTinh,
        SoDienThoai = entity.SoDienThoai,
        Email = entity.Email,
        DiaChi = entity.DiaChi,
        NgayDangKy = entity.NgayDangKy,
        TrangThai = entity.TrangThai
    };
}
