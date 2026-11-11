using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class NguoiDungGoiDoc
{
    public int MaNguoiDungGoiDoc { get; set; }

    public int MaNguoiDung { get; set; }

    public int MaGoiDoc { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public string? TrangThai { get; set; }

    public virtual GoiDoc MaGoiDocNavigation { get; set; } = null!;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
