namespace QuanLyThuVien.Domain.Entities;

/// <summary>
/// Yêu cầu khóa tài khoản độc giả do thủ thư gửi lên Admin duyệt.
/// Thủ thư không được khóa tài khoản đăng nhập trực tiếp; chỉ Admin duyệt yêu cầu
/// thì tài khoản mới chuyển sang <c>Bị khóa</c> và thẻ độc giả sang <c>Khóa thẻ</c>.
/// </summary>
public sealed class YeuCauKhoaTaiKhoan
{
    public int MaYeuCau { get; set; }

    /// <summary>Tài khoản độc giả bị yêu cầu khóa.</summary>
    public int MaTaiKhoan { get; set; }

    /// <summary>Nhân viên (thủ thư) gửi yêu cầu.</summary>
    public int MaNhanVien { get; set; }

    /// <summary>Lý do khóa do thủ thư nhập.</summary>
    public string LyDo { get; set; } = string.Empty;

    /// <summary>Số ngày khóa dự kiến; null nghĩa là không đặt thời hạn.</summary>
    public int? ThoiHanKhoaNgay { get; set; }

    /// <summary>Một trong các giá trị của <see cref="Enums.YeuCauKhoaTaiKhoanTrangThai"/>.</summary>
    public string TrangThai { get; set; } = Enums.YeuCauKhoaTaiKhoanTrangThai.ChoDuyet;

    /// <summary>Tên Admin đã duyệt / từ chối.</summary>
    public string? NguoiDuyet { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayDuyet { get; set; }

    /// <summary>Lý do Admin từ chối (nếu có).</summary>
    public string? LyDoTuChoi { get; set; }

    /// <summary>Khóa chống tạo trùng khi client gửi lại yêu cầu.</summary>
    public Guid IdempotencyKey { get; set; }

    public TaiKhoan TaiKhoan { get; set; } = null!;
    public NhanVien NhanVien { get; set; } = null!;
}
