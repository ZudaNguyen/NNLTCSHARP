namespace QuanLyThuVien.Domain.Entities;

public sealed class ChiTietPhieuMuon
{
    public int MaChiTiet { get; set; }
    public int MaPhieu { get; set; }
    public int MaSach { get; set; }
    public int SoLuongMuon { get; set; }
    public int SoLuongDaXuLy { get; set; }
    public PhieuMuon PhieuMuon { get; set; } = null!;
    public Sach Sach { get; set; } = null!;
    public ICollection<ChiTietTraSach> LichSuTra { get; set; } = new List<ChiTietTraSach>();
}
