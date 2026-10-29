namespace QuanLyThuVien.Domain.Entities;

public sealed class PhieuMuon
{
    public int MaPhieu { get; set; }
    public int MaDocGia { get; set; }
    public string? NguoiLap { get; set; }
    public DateTime NgayMuon { get; set; }
    public DateTime HanTra { get; set; }
    public Guid IdempotencyKey { get; set; }
    public DocGia DocGia { get; set; } = null!;
    public ICollection<ChiTietPhieuMuon> ChiTiets { get; set; } = new List<ChiTietPhieuMuon>();
}
