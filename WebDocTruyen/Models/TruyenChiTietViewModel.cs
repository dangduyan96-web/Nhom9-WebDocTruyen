namespace WebDocTruyen.Models;

public sealed class TruyenChiTietViewModel
{
    public Truyen Truyen { get; set; } = null!;
    public IReadOnlyList<ChuongTruyen> Chuongs { get; set; } = Array.Empty<ChuongTruyen>();
    public int? MaChuongDaDoc { get; set; }
    public bool DaDangNhap { get; set; }
}
