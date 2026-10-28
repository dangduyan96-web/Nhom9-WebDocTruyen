namespace WebDocTruyen.Models;

public sealed class DocChuongViewModel
{
    public Truyen Truyen { get; set; } = null!;
    public ChuongTruyen Chuong { get; set; } = null!;
    public int? MaChuongTruoc { get; set; }
    public int? MaChuongTiepTheo { get; set; }
    public bool DaDangNhap { get; set; }
}
