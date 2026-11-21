using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Helpers;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers
{
    [Route("api/KhuyenMai")]
    [ApiController]
    public class KhuyenMaiApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KhuyenMaiApiController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int?> ResolveUserIdAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue) return userId;

            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan)) return null;

            var user = await _context.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
            return user?.MaNguoiDung;
        }

        // GET: /api/KhuyenMai/KiemTra?loai=truyen|goi&id=5&ma=WELCOME10
        // Giá gốc luôn lấy từ database, không tin giá do trình duyệt gửi lên
        [HttpGet("KiemTra")]
        public async Task<IActionResult> KiemTra([FromQuery] string? loai, [FromQuery] int id, [FromQuery] string? ma)
        {
            decimal giaGoc;

            if (string.Equals(loai, "truyen", StringComparison.OrdinalIgnoreCase))
            {
                var truyen = await _context.Truyens.AsNoTracking().FirstOrDefaultAsync(t => t.MaTruyen == id);
                if (truyen == null)
                    return NotFound(new { hopLe = false, thongBao = "Truyện không tồn tại." });

                giaGoc = (truyen.GiaBan ?? 0) > 0 ? truyen.GiaBan!.Value : 49000;
            }
            else if (string.Equals(loai, "goi", StringComparison.OrdinalIgnoreCase))
            {
                var goi = await _context.GoiDocs.AsNoTracking().FirstOrDefaultAsync(g => g.MaGoiDoc == id && g.IsHoatDong == true);
                if (goi == null)
                    return NotFound(new { hopLe = false, thongBao = "Gói đọc không tồn tại hoặc đã ngừng bán." });

                giaGoc = goi.GiaTien;
            }
            else
            {
                return BadRequest(new { hopLe = false, thongBao = "Loại thanh toán không hợp lệ." });
            }

            var userId = await ResolveUserIdAsync();
            var kq = await KhuyenMaiHelper.KiemTraAsync(_context, ma, giaGoc, userId);

            return Ok(new
            {
                hopLe = kq.HopLe,
                thongBao = kq.ThongBao,
                maGiamGia = kq.MaGiamGia,
                soTienGiam = kq.SoTienGiam,
                giaGoc,
                giaSauGiam = kq.GiaSauGiam,
                giaSauGiamFormatted = kq.GiaSauGiam.ToString("N0") + " đ"
            });
        }
    }
}
