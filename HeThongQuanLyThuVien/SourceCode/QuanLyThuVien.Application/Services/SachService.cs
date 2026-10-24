using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Services;

public sealed class DuplicateIsbnException(string isbn)
    : Exception($"ISBN '{isbn}' đã được sử dụng.");

public sealed class BookRuleException(string message) : Exception(message)
{
}

public sealed class SachService(ISachRepository repository) : ISachService
{
    public async Task<IReadOnlyList<SachDto>> SearchAsync(
        string? keyword,
        int? maTheLoai,
        bool includeDeleted,
        bool onlyDeleted,
        CancellationToken cancellationToken)
    {
        var books = await repository.SearchAsync(
            keyword, maTheLoai, includeDeleted, onlyDeleted, cancellationToken);
        return books.Select(Map).ToList();
    }

    public async Task<SachDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(id, cancellationToken);
        return book is null ? null : Map(book);
    }

    public async Task<SachDto> CreateAsync(SachWriteDto request, CancellationToken cancellationToken)
    {
        var isbn = SachWriteDto.NormalizeIsbn(request.ISBN);
        await ValidateReferencesAndIsbnAsync(request, isbn, null, cancellationToken);

        var sach = new Sach
        {
            ISBN = isbn,
            TenSach = request.TenSach.Trim(),
            MaTheLoai = request.MaTheLoai,
            NamXB = request.NamXB,
            NhaXB = NullIfWhiteSpace(request.NhaXB),
            SoLuong = request.SoLuong,
            SoLuongCon = request.SoLuong,
            MoTa = NullIfWhiteSpace(request.MoTa),
            HinhAnh = NullIfWhiteSpace(request.HinhAnh),
            IsDeleted = false
        };

        await repository.AddAsync(sach, request.MaTacGia, cancellationToken);
        var created = await repository.GetByIdAsync(sach.MaSach, cancellationToken);
        return Map(created ?? sach);
    }

    public async Task<SachDto?> UpdateAsync(int id, SachWriteDto request, CancellationToken cancellationToken)
    {
        var sach = await repository.GetByIdAsync(id, cancellationToken);
        if (sach is null) return null;

        var isbn = SachWriteDto.NormalizeIsbn(request.ISBN);
        await ValidateReferencesAndIsbnAsync(request, isbn, id, cancellationToken);

        var borrowedCount = sach.SoLuong - sach.SoLuongCon;
        if (request.SoLuong < borrowedCount)
        {
            throw new BookRuleException(
                $"Tổng số lượng không thể nhỏ hơn số sách đang được mượn ({borrowedCount}).");
        }

        sach.ISBN = isbn;
        sach.TenSach = request.TenSach.Trim();
        sach.MaTheLoai = request.MaTheLoai;
        sach.NamXB = request.NamXB;
        sach.NhaXB = NullIfWhiteSpace(request.NhaXB);
        sach.SoLuong = request.SoLuong;
        sach.SoLuongCon = request.SoLuong - borrowedCount;
        sach.MoTa = NullIfWhiteSpace(request.MoTa);
        sach.HinhAnh = NullIfWhiteSpace(request.HinhAnh);

        await repository.UpdateWithAuthorsAsync(sach, request.MaTacGia, cancellationToken);
        return Map(sach);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
    {
        var sach = await repository.GetByIdAsync(id, cancellationToken);
        if (sach is null) return false;

        sach.IsDeleted = true;
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var sach = await repository.GetByIdIncludingDeletedAsync(id, cancellationToken);
        if (sach is null) return false;
        if (!sach.IsDeleted) return true;

        if (!await repository.CategoryExistsAsync(sach.MaTheLoai, cancellationToken))
            throw new BookRuleException("Không thể khôi phục sách vì thể loại đang bị ẩn. Hãy khôi phục thể loại trước.");

        var authorIds = sach.SachTacGias.Select(link => link.MaTacGia).Distinct().ToArray();
        var activeAuthorIds = await repository.GetExistingAuthorIdsAsync(authorIds, cancellationToken);
        if (activeAuthorIds.Count != authorIds.Length)
            throw new BookRuleException("Không thể khôi phục sách vì có tác giả đang bị ẩn. Hãy khôi phục tác giả trước.");

        sach.IsDeleted = false;
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SachLookupsDto> GetLookupsAsync(CancellationToken cancellationToken)
    {
        var categories = await repository.GetCategoriesAsync(cancellationToken);
        var authors = await repository.GetAuthorsAsync(cancellationToken);

        return new SachLookupsDto
        {
            TheLoai = categories.Select(x => new TheLoaiOptionDto
            {
                MaTheLoai = x.MaTheLoai,
                TenTheLoai = x.TenTheLoai
            }).ToList(),
            TacGia = authors.Select(x => new TacGiaOptionDto
            {
                MaTacGia = x.MaTacGia,
                TenTacGia = x.TenTacGia
            }).ToList()
        };
    }

    private async Task ValidateReferencesAndIsbnAsync(
        SachWriteDto request,
        string isbn,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        if (await repository.IsIsbnInUseAsync(isbn, excludingId, cancellationToken))
            throw new DuplicateIsbnException(isbn);

        if (!await repository.CategoryExistsAsync(request.MaTheLoai, cancellationToken))
            throw new BookRuleException("Thể loại không tồn tại hoặc đã bị ẩn.");

        var authorIds = request.MaTacGia.Distinct().ToArray();
        var foundAuthorIds = await repository.GetExistingAuthorIdsAsync(authorIds, cancellationToken);
        if (foundAuthorIds.Count != authorIds.Length)
            throw new BookRuleException("Có tác giả không tồn tại hoặc đã bị ẩn.");
    }

    private static SachDto Map(Sach sach) => new()
    {
        MaSach = sach.MaSach,
        ISBN = sach.ISBN,
        TenSach = sach.TenSach,
        MaTheLoai = sach.MaTheLoai,
        TenTheLoai = sach.TheLoai?.TenTheLoai ?? string.Empty,
        NamXB = sach.NamXB,
        NhaXB = sach.NhaXB,
        SoLuong = sach.SoLuong,
        SoLuongCon = sach.SoLuongCon,
        MoTa = sach.MoTa,
        HinhAnh = sach.HinhAnh,
        IsDeleted = sach.IsDeleted,
        MaTacGia = sach.SachTacGias.Select(x => x.MaTacGia).ToList(),
        TenTacGia = sach.SachTacGias
            .Where(x => x.TacGia is not null)
            .Select(x => x.TacGia.TenTacGia)
            .ToList()
    };

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
