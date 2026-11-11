using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebDocTruyen.Models;
using System.Linq;

namespace WebDocTruyen.Controllers
{
    [Route("api/Truyen")]
    [ApiController]
    public class TruyenAPIController : ControllerBase
    {

        private readonly AppDbContext _context;

        public TruyenAPIController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Truyen (Lấy danh sách truyện)
        [HttpGet]
        public async Task<IActionResult> GetDanhSachTruyen()
        {
            var dsTruyen = await _context.Truyens
                .Select(t => new
                {
                    t.MaTruyen,
                    t.TenTruyen,
                    t.Slug,
                    t.AnhBia,
                    t.TacGia,
                    t.LuotXem,
                    t.TrangThai
                })
                .ToListAsync();

            return Ok(dsTruyen);
        }

        // GET: api/Truyen/5 (Lấy chi tiết truyện + danh sách chương)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietTruyen(int id)
        {
            var truyen = await _context.Truyens
                .Include(t => t.ChuongTruyens)
                .FirstOrDefaultAsync(t => t.MaTruyen == id);

            if (truyen == null)
            {
                return NotFound(new { message = "Không tìm thấy truyện!" });
            }

            return Ok(truyen);
        }

        // POST: api/Truyen/{maTruyen}/TienDoDoc
        // Lưu chương gần nhất của người dùng đang đăng nhập.
        [HttpPost("{maTruyen:int}/TienDoDoc")]
        public async Task<IActionResult> LuuTienDoDoc(int maTruyen, [FromBody] LuuTienDoDocRequest request)
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrWhiteSpace(taiKhoan))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập để lưu tiến độ đọc." });
            }

            var userId = await _context.NguoiDungs
                .Where(u => u.TenDangNhap == taiKhoan)
                .Select(u => (int?)u.MaNguoiDung)
                .SingleOrDefaultAsync();

            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Không tìm thấy tài khoản đang đăng nhập." });
            }

            var chapterExists = await _context.ChuongTruyens
                .AnyAsync(c => c.MaChuong == request.MaChuongGanNhat && c.MaTruyen == maTruyen);

            if (!chapterExists)
            {
                return NotFound(new { message = "Chương không tồn tại trong truyện này." });
            }

            var progress = await _context.TienDoDocs
                .SingleOrDefaultAsync(p => p.MaNguoiDung == userId.Value && p.MaTruyen == maTruyen);

            if (progress == null)
            {
                progress = new TienDoDoc
                {
                    MaNguoiDung = userId.Value,
                    MaTruyen = maTruyen,
                    MaChuongGanNhat = request.MaChuongGanNhat,
                    SoLanDoc = 1,
                    NgayCapNhat = DateTime.Now
                };
                _context.TienDoDocs.Add(progress);
            }
            else
            {
                progress.MaChuongGanNhat = request.MaChuongGanNhat;
                progress.SoLanDoc = (progress.SoLanDoc ?? 0) + 1;
                progress.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã lưu tiến độ đọc.",
                progress.MaTruyen,
                progress.MaChuongGanNhat,
                progress.NgayCapNhat
            });
        }

        // ================= THÊM HÀM TÌM KIẾM NHANH =================
        [HttpGet("QuickSearch")]
        public async Task<IActionResult> QuickSearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Ok(new List<object>());
            }

            var results = await _context.Truyens
                .Where(t => t.TenTruyen.Contains(keyword) || (t.TacGia != null && t.TacGia.Contains(keyword)))
                .OrderByDescending(t => t.LuotXem)
                .Take(5)
                .Select(t => new
                {
                    id = t.MaTruyen,
                    title = t.TenTruyen,
                    slug = t.Slug,
                    author = string.IsNullOrEmpty(t.TacGia) ? "Đang cập nhật" : t.TacGia,
                    cover = t.AnhBia,
                    views = t.LuotXem ?? 0
                })
                .ToListAsync();

            return Ok(results);
        }



    }

}
