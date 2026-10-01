using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class ChuongTruyen
{
    public int MaChuong { get; set; }

    public int MaTruyen { get; set; }

    public int SoThuTuChuong { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? NoiDung { get; set; }

    public bool MienPhi { get; set; }

    public int? LuotXem { get; set; }

    public DateTime? NgayPhatHanh { get; set; }

    public virtual ICollection<BinhLuanChuong> BinhLuanChuongs { get; set; } = new List<BinhLuanChuong>();

    public virtual Truyen MaTruyenNavigation { get; set; } = null!;

    public virtual ICollection<TienDoDoc> TienDoDocs { get; set; } = new List<TienDoDoc>();
}
