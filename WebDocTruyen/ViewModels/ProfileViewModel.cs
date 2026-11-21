using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebDocTruyen.ViewModels;

public class ProfileViewModel
{
    public int MaNguoiDung { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? AnhDaiDien { get; set; }
    public string? SoDienThoai { get; set; }
    public string? GioiTinh { get; set; } // "Nam" hoặc "Nữ"
    public string VaiTro { get; set; } = "Thành viên thường";
    public bool IsAdmin { get; set; }
    public DateTime NgayThamGia { get; set; }

    // Dành cho mục EXP / Đọc sách
    public int KinhNghiemDocSach { get; set; }
    public int MaxKinhNghiem { get; set; } = 1000;

    // Danh sách menu bên trái
    public int SoSachDaDoc { get; set; }
    public int SoSachYeuThich { get; set; }
    public int SoBinhLuan { get; set; }
    public int SoLichSuThanhToan { get; set; }

    // Mục Thành tích (Achievements)
    public int SoNgayDongHanh { get; set; } = 1;
    public int SoAudioDaNghe { get; set; } = 0;
    public int ThoiGianDocHomNay { get; set; } = 0; // phút
    public int ThoiGianNgheHomNay { get; set; } = 0; // phút
    public decimal ChiPhiDaTietKiem { get; set; } = 0;
    public string XepHangHienTai { get; set; } = "Mọt Lười"; // Vua Mọt, Mọt Thông Thái, Mọt Sách, Mọt Lười
}

public class EditProfileViewModel
{
    public int MaNguoiDung { get; set; }

    public string TenDangNhap { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên / Nickname.")]
    [StringLength(255)]
    [Display(Name = "Họ và Tên / Nickname")]
    public string? HoTen { get; set; }

    [Display(Name = "Số điện thoại")]
    [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "Số điện thoại không hợp lệ (9-12 chữ số).")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; } = "Nam";

    public string? AnhDaiDienHienTai { get; set; }

    [Display(Name = "Chọn Ảnh Đại Diện Từ Máy Tính")]
    public IFormFile? AnhDaiDienFile { get; set; }

    [Display(Name = "Hoặc Nhập Link Ảnh (URL)")]
    public string? AnhDaiDienUrl { get; set; }
}
