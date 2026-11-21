namespace WebDocTruyen.Areas.Admin.ViewModels
{
    // Một dòng bình luận trong trang kiểm duyệt (chỉ lấy các cột cần thiết, không tải nội dung chương)
    public class BinhLuanAdminItemVM
    {
        public int MaBinhLuan { get; set; }
        public string NoiDung { get; set; } = "";
        public string TenDangNhap { get; set; } = "";
        public string? HoTen { get; set; }
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = "";
        public int SoThuTuChuong { get; set; }
        public bool LaTraLoi { get; set; }
        public int SoTraLoi { get; set; }
        public bool IsAn { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}
