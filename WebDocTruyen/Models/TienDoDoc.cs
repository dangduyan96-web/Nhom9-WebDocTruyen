using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class TienDoDoc
{
    public int MaNguoiDung { get; set; }

    public int MaTruyen { get; set; }

    public int MaChuongGanNhat { get; set; }

    public int? SoLanDoc { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ChuongTruyen MaChuongGanNhatNavigation { get; set; } = null!;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Truyen MaTruyenNavigation { get; set; } = null!;
}
