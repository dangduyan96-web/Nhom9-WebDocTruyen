using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class Truyen
{
    public int MaTruyen { get; set; }

    public string TenTruyen { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? AnhBia { get; set; }

    public string? MoTa { get; set; }

    public string? TacGia { get; set; }

    public string? TrangThai { get; set; }

    public string? ChinhSachTruyCap { get; set; }

    public decimal? GiaBan { get; set; }

    public int? LuotXem { get; set; }

    public int? LuotThich { get; set; }

    public int? NguoiTao { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChuongTruyen> ChuongTruyens { get; set; } = new List<ChuongTruyen>();

    public virtual ICollection<DanhGiaTruyen> DanhGiaTruyens { get; set; } = new List<DanhGiaTruyen>();

    public virtual NguoiDung? NguoiTaoNavigation { get; set; }

    public virtual ICollection<TienDoDoc> TienDoDocs { get; set; } = new List<TienDoDoc>();

    public virtual ICollection<TruyenDaMua> TruyenDaMuas { get; set; } = new List<TruyenDaMua>();

    public virtual ICollection<TruyenYeuThich> TruyenYeuThiches { get; set; } = new List<TruyenYeuThich>();

    public virtual ICollection<TheLoai> MaTheLoais { get; set; } = new List<TheLoai>();
}
