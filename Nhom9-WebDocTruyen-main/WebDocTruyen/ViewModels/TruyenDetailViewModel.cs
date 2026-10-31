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
    public bool DaYeuThich { get; set; }
    public bool CoQuyenDoc { get; set; }      // đã mua hoặc có gói còn hạn
    public int? MaChuongDangDoc { get; set; } // để nút "Đọc tiếp"
}