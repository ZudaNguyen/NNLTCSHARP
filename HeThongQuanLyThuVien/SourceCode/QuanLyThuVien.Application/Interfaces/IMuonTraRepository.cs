using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Domain.Entities;

namespace QuanLyThuVien.Application.Interfaces;

public interface IMuonTraRepository
{
    Task<IReadOnlyList<PhieuMuon>> SearchAsync(
        string? search, string? trangThai, string? loaiNgay, DateTime? tuNgay, DateTime? denNgay,
        bool chiQuaHan, CancellationToken cancellationToken);
    Task<IReadOnlyList<PhieuMuon>> GetForReaderAsync(int readerId, CancellationToken cancellationToken);
    Task<PhieuMuon?> GetByIdForReaderAsync(int readerId, int maPhieu, CancellationToken cancellationToken);
    Task<PhieuMuon?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SachMuonOptionDto>> GetSachConHangAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ChiTietPhieuMuon>> GetQuaHanAsync(string? search, CancellationToken cancellationToken);
    Task<PhieuMuon> CreateAsync(TaoMuonTraDto request, CancellationToken cancellationToken);
    Task<PhieuMuon?> TraChiTietAsync(int maPhieu, int maChiTiet, TraChiTietDto request, CancellationToken cancellationToken);
    Task<TienPhatDto?> GetTienPhatAsync(int maPhieu, CancellationToken cancellationToken);
    Task<TienPhatDto?> GetTienPhatForReaderAsync(int readerId, int maPhieu, CancellationToken cancellationToken);
    Task<TienPhatDto?> UpdateTienPhatAsync(int maPhieu, CapNhatTienPhatDto request, CancellationToken cancellationToken);
    Task<KhoanThuTienPhatDto> ThuTienPhatAsync(int maPhieu, ThuTienPhatDto request, CancellationToken cancellationToken);
    Task<CauHinhTienPhatDto> GetCauHinhTienPhatAsync(CancellationToken cancellationToken);
    Task<CauHinhTienPhatDto> SetCauHinhTienPhatAsync(decimal mucPhatMoiNgay, CancellationToken cancellationToken);
}
