using System;
using System.Collections.Generic;

namespace WebDocTruyen.ViewModels
{
    public class BinhLuanChuongVM
    {
        public int MaBinhLuan { get; set; }

        public int MaChuong { get; set; }

        public int? MaBinhLuanCha { get; set; }

        public string NoiDung { get; set; } = "";

        public string? TenNguoiDung { get; set; }

        public string? AnhDaiDien { get; set; }

        public DateTime? NgayTao { get; set; }

        public List<BinhLuanChuongVM> TraLois { get; set; } = new();
    }
}
