using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class KhuyenMai
{
    public int MaKhuyenMai { get; set; }

    public string MaGiamGia { get; set; } = null!;

    public decimal SoTienGiam { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public bool? IsHoatDong { get; set; }
}
