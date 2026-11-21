using WebDocTruyen.Models;

namespace WebDocTruyen.ViewModels;

public class ChuongItemVM
{
    public int MaChuong { get; set; }
    public int SoThuTuChuong { get; set; }
    public string TieuDe { get; set; } = null!;
    public bool MienPhi { get; set; }
    public int? LuotXem { get; set; }
    public DateTime? NgayPhatHanh { get; set; }
}

public class TheLoaiItemVM
{
    public int MaTheLoai { get; set; }
    public string TenTheLoai { get; set; } = null!;
}

public class TruyenDetailViewModel
{
    public Truyen Truyen { get; set; } = null!;
    public List<string> TheLoais { get; set; } = new();
    public List<TheLoaiItemVM> DanhSachTheLoai { get; set; } = new();
    public List<ChuongItemVM> Chuongs { get; set; } = new();
    public double DiemTrungBinh { get; set; }
    public int SoDanhGia { get; set; }
    public bool DaDangNhap { get; set; }
    public bool DaTheoDoi { get; set; }
    public bool DaThich { get; set; }
    public bool DaYeuThich { get; set; }
    public bool CoQuyenDoc { get; set; }      // đã mua hoặc có gói còn hạn
    public bool DaMuaLe { get; set; }         // người dùng đã mua lẻ truyện này
    public int? MaChuongDangDoc { get; set; } // để nút "Đọc tiếp"
}