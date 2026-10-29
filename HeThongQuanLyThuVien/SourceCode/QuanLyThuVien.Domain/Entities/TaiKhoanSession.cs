namespace QuanLyThuVien.Domain.Entities;

public sealed class TaiKhoanSession
{
    public Guid Id { get; set; }
    public int MaTaiKhoan { get; set; }
    public byte[] TokenHash { get; set; } = [];
    public DateTime TaoLucUtc { get; set; }
    public DateTime HetHanLucUtc { get; set; }
    public DateTime? ThuHoiLucUtc { get; set; }
}
