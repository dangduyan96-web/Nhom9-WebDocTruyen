namespace WebDocTruyen.Areas.Admin.ViewModels
{
    // Dữ liệu form tạo / sửa mã khuyến mãi (để các trường là nullable, tự kiểm tra và báo lỗi tiếng Việt)
    public class KhuyenMaiFormViewModel
    {
        public int MaKhuyenMai { get; set; }
        public string? MaGiamGia { get; set; }
        public decimal? SoTienGiam { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool IsHoatDong { get; set; } = true;
    }
}
