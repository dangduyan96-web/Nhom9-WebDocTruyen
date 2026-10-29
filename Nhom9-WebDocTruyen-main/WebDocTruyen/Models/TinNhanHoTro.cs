using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class TinNhanHoTro
{
    public int MaTinNhan { get; set; }

    public int MaNguoiDung { get; set; }

    public string VaiTroNguoiGui { get; set; } = null!;

    public int MaNguoiGui { get; set; }

    public string NoiDung { get; set; } = null!;

    public bool? DaDoc { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
