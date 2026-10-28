namespace WebDocTruyen.Models;

public sealed class ReadingProgressItemViewModel
{
    public int MaTruyen { get; set; }
    public int MaChuong { get; set; }
    public string TenTruyen { get; set; } = string.Empty;
    public string TieuDeChuong { get; set; } = string.Empty;
    public string? AnhBia { get; set; }
    public int SoThuTuChuong { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
