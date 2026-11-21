namespace WebDocTruyen.ViewModels;

public class YeuThichItemVM
{
    public int MaTruyen { get; set; }
    public string TenTruyen { get; set; } = null!;
    public string? AnhBia { get; set; }
    public string? TacGia { get; set; }
    public string? TrangThai { get; set; }
    public int LuotXem { get; set; }
    public int SoChuong { get; set; }
    public DateTime? NgayTheoDoi { get; set; }
}
