using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class GoiDoc
{
    public int MaGoiDoc { get; set; }

    public string TenGoi { get; set; } = null!;

    public decimal GiaTien { get; set; }

    public int SoNgayHieuLuc { get; set; }

    public string? MoTa { get; set; }

    public bool? IsHoatDong { get; set; }

    public virtual ICollection<NguoiDungGoiDoc> NguoiDungGoiDocs { get; set; } = new List<NguoiDungGoiDoc>();
}
