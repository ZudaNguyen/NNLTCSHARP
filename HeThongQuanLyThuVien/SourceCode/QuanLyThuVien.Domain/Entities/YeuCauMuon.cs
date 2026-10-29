namespace QuanLyThuVien.Domain.Entities;

/// <summary>
/// Yêu cầu mượn sách do độc giả gửi từ Web. Thủ thư duyệt yêu cầu thì hệ thống
/// mới tạo <see cref="PhieuMuon"/> tương ứng; từ chối/hủy thì không tạo phiếu.
/// </summary>
public sealed class YeuCauMuon
{
    public int MaYeuCau { get; set; }
    public int MaDocGia { get; set; }
    public int MaSach { get; set; }
    public int SoLuong { get; set; }

    /// <summary>Thời điểm độc giả gửi yêu cầu.</summary>
    public DateTime NgayTao { get; set; }

    /// <summary>Một trong các giá trị của <see cref="Enums.YeuCauMuonTrangThai"/>.</summary>
    public string TrangThai { get; set; } = Enums.YeuCauMuonTrangThai.ChoDuyet;

    /// <summary>Ghi chú của độc giả khi gửi yêu cầu.</summary>
    public string? GhiChu { get; set; }

    /// <summary>Lý do thủ thư từ chối (nếu có).</summary>
    public string? LyDoTuChoi { get; set; }

    /// <summary>Tên nhân viên đã duyệt / từ chối.</summary>
    public string? NguoiDuyet { get; set; }

    public DateTime? NgayDuyet { get; set; }

    /// <summary>Phiếu mượn được tạo khi yêu cầu được duyệt.</summary>
    public int? MaPhieu { get; set; }

    /// <summary>
    /// Khóa chống trùng khi duyệt: dùng lại đúng khóa này để tạo phiếu mượn, nên
    /// bấm duyệt hai lần cũng không sinh hai phiếu.
    /// </summary>
    public Guid IdempotencyKey { get; set; }

    public DocGia DocGia { get; set; } = null!;
    public Sach Sach { get; set; } = null!;
    public PhieuMuon? PhieuMuon { get; set; }
}
