using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class BinhLuanChuong
{
    public int MaBinhLuan { get; set; }

    public int MaChuong { get; set; }

    public int MaNguoiDung { get; set; }

    public string NoiDung { get; set; } = null!;

    public int? MaBinhLuanCha { get; set; }

    public bool? IsAn { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<BinhLuanChuong> InverseMaBinhLuanChaNavigation { get; set; } = new List<BinhLuanChuong>();

    public virtual BinhLuanChuong? MaBinhLuanChaNavigation { get; set; }

    public virtual ChuongTruyen MaChuongNavigation { get; set; } = null!;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
