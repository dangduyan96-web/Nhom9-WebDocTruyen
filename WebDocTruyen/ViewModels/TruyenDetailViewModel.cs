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

public class TruyenDetailViewModel
{
    public Truyen Truyen { get; set; } = null!;
    public List<string> TheLoais { get; set; } = new();
    public List<ChuongItemVM> Chuongs { get; set; } = new();
    public double DiemTrungBinh { get; set; }
    public int SoDanhGia { get; set; }
<<<<<<< HEAD
=======
    public bool DaDangNhap { get; set; }
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
    public bool DaYeuThich { get; set; }
    public bool CoQuyenDoc { get; set; }      // đã mua hoặc có gói còn hạn
    public int? MaChuongDangDoc { get; set; } // để nút "Đọc tiếp"
}