using QuanLyThuVien.Application.DTOs;

namespace QuanLyThuVien.Application.Interfaces;

public interface IMuonTraService
{
    Task<IReadOnlyList<MuonTraDto>> SearchAsync(
        string? search, string? trangThai, string? loaiNgay, DateTime? tuNgay, DateTime? denNgay,
        bool chiQuaHan, CancellationToken cancellationToken);
    Task<IReadOnlyList<MuonTraDto>> GetForReaderAsync(int readerId, CancellationToken cancellationToken);
    Task<MuonTraChiTietDto?> GetByIdForReaderAsync(int readerId, int maPhieu, CancellationToken cancellationToken);
    Task<MuonTraChiTietDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SachMuonOptionDto>> GetSachConHangAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<QuaHanDto>> GetQuaHanAsync(string? search, CancellationToken cancellationToken);
    Task<MuonTraChiTietDto> CreateAsync(TaoMuonTraDto request, CancellationToken cancellationToken);
    Task<MuonTraChiTietDto?> TraChiTietAsync(int maPhieu, int maChiTiet, TraChiTietDto request, CancellationToken cancellationToken);
    Task<TienPhatDto?> GetTienPhatAsync(int maPhieu, CancellationToken cancellationToken);
    Task<TienPhatDto?> GetTienPhatForReaderAsync(int readerId, int maPhieu, CancellationToken cancellationToken);
    Task<TienPhatDto?> UpdateTienPhatAsync(int maPhieu, CapNhatTienPhatDto request, CancellationToken cancellationToken);
    Task<KhoanThuTienPhatDto> ThuTienPhatAsync(int maPhieu, ThuTienPhatDto request, CancellationToken cancellationToken);
    Task<CauHinhTienPhatDto> GetCauHinhTienPhatAsync(CancellationToken cancellationToken);
    Task<CauHinhTienPhatDto> SetCauHinhTienPhatAsync(decimal mucPhatMoiNgay, CancellationToken cancellationToken);
}
