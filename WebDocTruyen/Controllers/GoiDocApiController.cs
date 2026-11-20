using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers
{
    [Route("api/GoiDoc")]
    [Route("api/GoiDocApi")]
    [ApiController]
    public class GoiDocApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GoiDocApiController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/GoiDoc (Lấy danh sách các gói đọc VIP đang hoạt động)
        [HttpGet]
        public async Task<IActionResult> GetGoiDocs()
        {
            var dsGoiDoc = await _context.GoiDocs
                .Where(g => g.IsHoatDong == true)
                .OrderBy(g => g.GiaTien)
                .Select(g => new
                {
                    maGoiDoc = g.MaGoiDoc,
                    tenGoi = g.TenGoi,
                    soNgayHieuLuc = g.SoNgayHieuLuc,
                    giaTien = g.GiaTien,
                    moTa = g.MoTa
                })
                .ToListAsync();

            return Ok(dsGoiDoc);
        }

        // 2. GET: api/GoiDoc/UserVip?userId=5 (Kiểm tra gói VIP của người dùng trong CSDL)
        [HttpGet("UserVip")]
        public async Task<IActionResult> GetUserVip([FromQuery] int userId)
        {
            if (userId <= 0) return BadRequest(new { isVip = false, message = "UserId không hợp lệ." });

            var vip = await _context.NguoiDungGoiDocs
                .AsNoTracking()
                .Include(g => g.MaGoiDocNavigation)
                .Where(g => g.MaNguoiDung == userId && g.TrangThai == "Active" && g.NgayKetThuc > DateTime.Now)
                .OrderByDescending(g => g.NgayKetThuc)
                .FirstOrDefaultAsync();

            if (vip == null)
            {
                // Kiểm tra xem có phải tài khoản Admin (mặc định hưởng quyền VIP)
                var user = await _context.NguoiDungs.FindAsync(userId);
                if (user != null && user.MaVaiTro == 1)
                {
                    return Ok(new
                    {
                        isVip = true,
                        tenGoi = "Admin VIP Vĩnh Viễn",
                        ngayKetThuc = DateTime.Now.AddYears(10),
                        soNgayConLai = 3650
                    });
                }
                return Ok(new { isVip = false });
            }

            return Ok(new
            {
                isVip = true,
                tenGoi = vip.MaGoiDocNavigation?.TenGoi ?? "Gói VIP",
                ngayKetThuc = vip.NgayKetThuc,
                soNgayConLai = (int)Math.Max(0, (vip.NgayKetThuc - DateTime.Now).TotalDays)
            });
        }

        // 3. POST: api/GoiDoc/KichHoat (Kích hoạt gói VIP lưu trực tiếp vào SQL Server)
        public class KichHoatGoiRequest
        {
            public int MaNguoiDung { get; set; }
            public int MaGoiDoc { get; set; }
            public string? PhuongThuc { get; set; } = "VietQR";
        }

        [HttpPost("KichHoat")]
        public async Task<IActionResult> KichHoatGoi([FromBody] KichHoatGoiRequest req)
        {
            if (req.MaNguoiDung <= 0)
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập để kích hoạt gói VIP." });
            }

            var goi = await _context.GoiDocs.FindAsync(req.MaGoiDoc);
            if (goi == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy gói đọc tương ứng." });
            }

            var now = DateTime.Now;
            var conHan = await _context.NguoiDungGoiDocs
                .Where(x => x.MaNguoiDung == req.MaNguoiDung && x.TrangThai == "Active" && x.NgayKetThuc > now)
                .OrderByDescending(x => x.NgayKetThuc)
                .FirstOrDefaultAsync();

            var batDau = conHan?.NgayKetThuc ?? now;
            var ketThuc = batDau.AddDays(goi.SoNgayHieuLuc);

            var ndgd = new NguoiDungGoiDoc
            {
                MaNguoiDung = req.MaNguoiDung,
                MaGoiDoc = goi.MaGoiDoc,
                NgayBatDau = batDau,
                NgayKetThuc = ketThuc,
                TrangThai = "Active"
            };
            _context.NguoiDungGoiDocs.Add(ndgd);

            var gd = new LichSuGiaoDich
            {
                MaNguoiDung = req.MaNguoiDung,
                LoaiGiaoDich = "GoiDoc",
                MaMụcTieu = goi.MaGoiDoc,
                SoTien = goi.GiaTien,
                PhuongThucThanhToan = string.IsNullOrEmpty(req.PhuongThuc) ? "VietQR" : req.PhuongThuc,
                TrangThai = "Thành công",
                NgayTao = now
            };
            _context.LichSuGiaoDiches.Add(gd);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Kích hoạt thành công {goi.TenGoi}!",
                isVip = true,
                tenGoiVip = goi.TenGoi,
                hanGoiVip = ketThuc.ToString("o")
            });
        }
    }
}
