using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDocTruyen.Models
{
    [Table("BaoCaoLoi")]
    public partial class BaoCaoLoi
    {
        [Key]
        public int MaBaoCao { get; set; }

        public int MaTruyen { get; set; }

        public int MaChuong { get; set; }

        public int? MaNguoiDung { get; set; }

        [StringLength(100)]
        public string? TenNguoiBaoCao { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại lỗi")]
        [StringLength(100)]
        public string LoaiLoi { get; set; } = null!;

        public string? MoTaChiTiet { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ xử lý";

        public string? GhiChuAdmin { get; set; }

        public int? NguoiXuLy { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public DateTime? NgayXuLy { get; set; }

        [ForeignKey("MaTruyen")]
        public virtual Truyen? MaTruyenNavigation { get; set; }

        [ForeignKey("MaChuong")]
        public virtual ChuongTruyen? MaChuongNavigation { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

        [ForeignKey("NguoiXuLy")]
        public virtual NguoiDung? NguoiXuLyNavigation { get; set; }
    }
}
