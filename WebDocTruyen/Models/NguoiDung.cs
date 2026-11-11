using System;
using System.Collections.Generic;

namespace WebDocTruyen.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public int MaVaiTro { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? AnhDaiDien { get; set; }

    public bool IsKhoa { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? ThoiGianHetHanToken { get; set; }

    public string? TokenQuenMatKhau { get; set; }

    public DateTime? ThoiGianHetHanTokenQuenMatKhau { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ICollection<BinhLuanChuong> BinhLuanChuongs { get; set; } = new List<BinhLuanChuong>();

    public virtual ICollection<DanhGiaTruyen> DanhGiaTruyens { get; set; } = new List<DanhGiaTruyen>();

    public virtual ICollection<LichSuGiaoDich> LichSuGiaoDiches { get; set; } = new List<LichSuGiaoDich>();

    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    public virtual ICollection<NguoiDungGoiDoc> NguoiDungGoiDocs { get; set; } = new List<NguoiDungGoiDoc>();

    public virtual ICollection<TienDoDoc> TienDoDocs { get; set; } = new List<TienDoDoc>();

    public virtual ICollection<TinNhanHoTro> TinNhanHoTros { get; set; } = new List<TinNhanHoTro>();

    public virtual ICollection<TruyenDaMua> TruyenDaMuas { get; set; } = new List<TruyenDaMua>();

    public virtual ICollection<TruyenYeuThich> TruyenYeuThiches { get; set; } = new List<TruyenYeuThich>();

    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
