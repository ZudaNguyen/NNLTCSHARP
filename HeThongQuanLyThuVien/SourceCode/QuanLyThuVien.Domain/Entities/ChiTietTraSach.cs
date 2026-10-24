namespace QuanLyThuVien.Domain.Entities;

public sealed class ChiTietTraSach
{
    public int MaLanTra { get; set; }
    public int MaChiTiet { get; set; }
    public int SoLuongXuLy { get; set; }
    public DateTime NgayTra { get; set; }
    public string TinhTrangSach { get; set; } = "Nguyên vẹn";
    public int SoNgayTre { get; set; }
    public decimal? MucPhatMoiNgayApDung { get; set; }
    public decimal? SoTienPhatPhaiThu { get; set; }
    public Guid IdempotencyKey { get; set; }
    public ChiTietPhieuMuon ChiTiet { get; set; } = null!;
    public ICollection<ThuTienPhat> CacKhoanThu { get; set; } = new List<ThuTienPhat>();
}
