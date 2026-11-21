using System;

namespace WebDocTruyen.ViewModels
{
    public class BinhLuanCuaToiItemVM
    {
        public int MaBinhLuan { get; set; }
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = string.Empty;
        public string? AnhBia { get; set; }
        public int MaChuong { get; set; }
        public int SoThuTuChuong { get; set; }
        public string? TieuDeChuong { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime? NgayTao { get; set; }
        public int TongLike { get; set; }
        public bool IsTraLoi { get; set; }
    }
}
