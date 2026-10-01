using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class DanhGiaTruyen
{
    public int MaDanhGia { get; set; }

    public int MaNguoiDung { get; set; }

    public int MaTruyen { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDungDanhGia { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Truyen MaTruyenNavigation { get; set; } = null!;
}
