using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class DuplicateTheLoaiNameException(string name)
    : Exception($"Tên thể loại '{name}' đã tồn tại.");

public sealed class TheLoaiInUseException()
    : Exception("Không thể ẩn thể loại vì đang được sách sử dụng.");

public sealed class TheLoaiService(ITheLoaiRepository repository) : ITheLoaiService
{
    public async Task<IReadOnlyList<TheLoaiDto>> SearchAsync(
        string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken)
    {
        var entities = await repository.SearchAsync(keyword, includeDeleted, onlyDeleted, cancellationToken);
        return entities.Select(Map).ToList();
    }

    public async Task<TheLoaiDto?> GetByIdAsync(
        int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, includeDeleted, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<TheLoaiDto> CreateAsync(TheLoaiWriteDto request, CancellationToken cancellationToken)
    {
        var name = request.TenTheLoai.Trim();
        await EnsureUniqueNameAsync(name, null, cancellationToken);

        var entity = new TheLoai
        {
            TenTheLoai = name,
            MoTa = NullIfWhiteSpace(request.MoTa),
            IsDeleted = false
        };

        await repository.AddAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<TheLoaiDto?> UpdateAsync(
        int id, TheLoaiWriteDto request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (entity is null) return null;

        var name = request.TenTheLoai.Trim();
        await EnsureUniqueNameAsync(name, id, cancellationToken);
        entity.TenTheLoai = name;
        entity.MoTa = NullIfWhiteSpace(request.MoTa);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (entity is null) return false;
        if (await repository.IsUsedByBooksAsync(id, cancellationToken))
            throw new TheLoaiInUseException();

        entity.IsDeleted = true;
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> RestoreAsync(int id, CancellationToken cancellationToken) =>
        repository.RestoreAsync(id, cancellationToken);

    private async Task EnsureUniqueNameAsync(string name, int? excludingId, CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(name, excludingId, cancellationToken))
            throw new DuplicateTheLoaiNameException(name);
    }

    private static TheLoaiDto Map(TheLoai entity) => new()
    {
        MaTheLoai = entity.MaTheLoai,
        TenTheLoai = entity.TenTheLoai,
        MoTa = entity.MoTa,
        IsDeleted = entity.IsDeleted
    };

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
