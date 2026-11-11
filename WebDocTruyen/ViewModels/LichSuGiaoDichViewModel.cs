using System;
using System.Collections.Generic;
using System.Linq;

namespace WebDocTruyen.ViewModels
{
    public class GiaoDichItemVM
    {
        public int MaGiaoDich { get; set; }
        public string MaGiaoDichFormatted => $"#GD{MaGiaoDich:D5}";
        public string LoaiGiaoDich { get; set; } = "MuaTruyen";

        public string TenLoaiGiaoDich => LoaiGiaoDich switch
        {
            "MuaTruyen" => "Mua truyện lẻ",
            "Truyen" => "Mua truyện lẻ",
            "GoiDoc" => "Gói đọc theo tháng",
            "VIP" => "Đăng ký VIP",
            _ => "Mua truyện lẻ"
        };

        public int MaMucTieu { get; set; }
        public string TenMucTieu { get; set; } = "Chưa xác định";
        public string? AnhBia { get; set; }
        public string? TacGia { get; set; }
        public decimal SoTien { get; set; }
        public string? MaGiamGia { get; set; }
        public string PhuongThucThanhToan { get; set; } = "VNPAY";
        public string TrangThai { get; set; } = "Thành công";
        public DateTime NgayTao { get; set; }
    }

    public class LichSuGiaoDichViewModel
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int TongSoGiaoDich => DanhSachGiaoDich.Count;
        public decimal TongTienDaChi => DanhSachGiaoDich
            .Where(g => g.TrangThai == "Thành công" || g.TrangThai == "Success" || g.TrangThai == "Hoàn thành")
            .Sum(g => g.SoTien);

        public int SoTruyenDaMua => DanhSachGiaoDich
            .Count(g => (g.LoaiGiaoDich == "MuaTruyen" || g.LoaiGiaoDich == "Truyen")
                     && (g.TrangThai == "Thành công" || g.TrangThai == "Success" || g.TrangThai == "Hoàn thành"));

        public string? GoiDocHienTai { get; set; }
        public DateTime? HanGoiDoc { get; set; }

        public List<GiaoDichItemVM> DanhSachGiaoDich { get; set; } = new();
    }
}
