using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class LichSuGiaoDich
{
    public int MaGiaoDich { get; set; }

    public int MaNguoiDung { get; set; }

    public string LoaiGiaoDich { get; set; } = null!;

    public int MaMụcTieu { get; set; }

    public decimal SoTien { get; set; }

    public string? MaGiamGia { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
