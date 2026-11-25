using System.ComponentModel.DataAnnotations;

namespace WebDocTruyen.Models
{
    public class Banner
    {
        [Key]
        public int MaBanner { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề banner")]
        [StringLength(150)]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập hoặc tải lên đường dẫn ảnh")]
        [StringLength(500)]
        public string DuongDanAnh { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LienKet { get; set; } // Ví dụ: /Truyen/Details/1 hoặc /DangKyGoi

        public int ThuTu { get; set; } = 1; // Thứ tự hiển thị trên slide

        public bool HoatDong { get; set; } = true; // Bật / tắt hiển thị

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}