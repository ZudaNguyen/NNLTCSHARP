using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class DuplicateTacGiaNameException(string name)
    : Exception($"Tên tác giả '{name}' đã tồn tại.");

public sealed class TacGiaInUseException()
    : Exception("Không thể ẩn tác giả vì đang được liên kết với sách.");

public sealed class TacGiaService(ITacGiaRepository repository) : ITacGiaService
{
    public async Task<IReadOnlyList<TacGiaDto>> SearchAsync(
        string? keyword, bool includeDeleted, bool onlyDeleted, CancellationToken cancellationToken)
    {
        var entities = await repository.SearchAsync(keyword, includeDeleted, onlyDeleted, cancellationToken);
        return entities.Select(Map).ToList();
    }

    public async Task<TacGiaDto?> GetByIdAsync(
        int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, includeDeleted, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<TacGiaDto> CreateAsync(TacGiaWriteDto request, CancellationToken cancellationToken)
    {
        var name = request.TenTacGia.Trim();
        await EnsureUniqueNameAsync(name, null, cancellationToken);
        var entity = new TacGia
        {
            TenTacGia = name,
            QuocTich = NullIfWhiteSpace(request.QuocTich),
            GioiThieu = NullIfWhiteSpace(request.GioiThieu),
            IsDeleted = false
        };

        await repository.AddAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<TacGiaDto?> UpdateAsync(
        int id, TacGiaWriteDto request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (entity is null) return null;

        var name = request.TenTacGia.Trim();
        await EnsureUniqueNameAsync(name, id, cancellationToken);
        entity.TenTacGia = name;
        entity.QuocTich = NullIfWhiteSpace(request.QuocTich);
        entity.GioiThieu = NullIfWhiteSpace(request.GioiThieu);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (entity is null) return false;
        if (await repository.IsLinkedToBooksAsync(id, cancellationToken))
            throw new TacGiaInUseException();

        entity.IsDeleted = true;
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> RestoreAsync(int id, CancellationToken cancellationToken) =>
        repository.RestoreAsync(id, cancellationToken);

    private async Task EnsureUniqueNameAsync(string name, int? excludingId, CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(name, excludingId, cancellationToken))
            throw new DuplicateTacGiaNameException(name);
    }

    private static TacGiaDto Map(TacGia entity) => new()
    {
        MaTacGia = entity.MaTacGia,
        TenTacGia = entity.TenTacGia,
        QuocTich = entity.QuocTich,
        GioiThieu = entity.GioiThieu,
        IsDeleted = entity.IsDeleted
    };

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
