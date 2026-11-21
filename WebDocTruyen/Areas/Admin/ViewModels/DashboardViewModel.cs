namespace WebDocTruyen.Areas.Admin.ViewModels
{
    public class TopTruyenLuotXemVM
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = null!;
        public string? AnhBia { get; set; }
        public string? TacGia { get; set; }
        public int LuotXem { get; set; }
        public int SoChuong { get; set; }
        public int LuotThich { get; set; }
    }

    public class DoanhThuTheoThangVM
    {
        public string Thang { get; set; } = null!;
        public decimal DoanhThu { get; set; }
        public int SoGiaoDich { get; set; }
    }

    public class GiaoDichGanDayVM
    {
        public int MaGiaoDich { get; set; }
        public string TenNguoiDung { get; set; } = null!;
        public string LoaiGiaoDich { get; set; } = null!;
        public string TenMucTieu { get; set; } = null!;
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public DateTime NgayTao { get; set; }
    }

    public class DashboardViewModel
    {
        // Thống kê tổng hợp
        public int TongSoTruyen { get; set; }
        public int TongSoNguoiDung { get; set; }
        public long TongLuotXem { get; set; }
        public decimal TongDoanhThu { get; set; }

        // Doanh thu chi tiết
        public decimal DoanhThuMuaTruyen { get; set; }
        public decimal DoanhThuGoiDoc { get; set; }
        public int SoDonMuaTruyen { get; set; }
        public int SoDonMuaGoiDoc { get; set; }

        // Top truyện xem nhiều nhất
        public List<TopTruyenLuotXemVM> TopTruyen { get; set; } = new();

        // Giao dịch gần đây
        public List<GiaoDichGanDayVM> GiaoDichMoiNhat { get; set; } = new();

        // Doanh thu theo tháng (cho biểu đồ / danh sách)
        public List<DoanhThuTheoThangVM> DoanhThuTheoThang { get; set; } = new();
    }
}
