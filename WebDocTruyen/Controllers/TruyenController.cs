using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebDocTruyen.Models;
using System.Linq;

namespace WebDocTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TruyenController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TruyenController(AppDbContext context)
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

        // GET: api/Truyen/{maTruyen}/TienDoDoc
        [HttpGet("{id:int}/TienDoDoc")]
        public async Task<IActionResult> GetTienDoDoc(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { message = "Vui lòng đăng nhập để xem tiến độ đọc." });

            var progress = await _context.TienDoDocs
                .Where(p => p.MaNguoiDung == userId.Value && p.MaTruyen == id)
                .Select(p => new
                {
                    p.MaNguoiDung,
                    p.MaTruyen,
                    p.MaChuongGanNhat,
                    p.SoLanDoc,
                    p.NgayCapNhat,
                    p.MaChuongGanNhatNavigation.SoThuTuChuong,
                    p.MaChuongGanNhatNavigation.TieuDe
                })
                .FirstOrDefaultAsync();

            return progress == null ? NotFound(new { message = "Chưa có tiến độ đọc cho truyện này." }) : Ok(progress);
        }

        // POST: api/Truyen/{maTruyen}/TienDoDoc
        [HttpPost("{id:int}/TienDoDoc")]
        public async Task<IActionResult> LuuTienDoDoc(int id, [FromBody] LuuTienDoDocRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { message = "Vui lòng đăng nhập để lưu tiến độ đọc." });

            if (request.MaChuongGanNhat <= 0)
                return BadRequest(new { message = "Mã chương không hợp lệ." });

            var chapter = await _context.ChuongTruyens
                .Where(c => c.MaChuong == request.MaChuongGanNhat && c.MaTruyen == id)
                .Select(c => new { c.MaChuong, c.MienPhi })
                .FirstOrDefaultAsync();

            if (chapter == null)
                return BadRequest(new { message = "Chương không thuộc truyện đã chọn." });

            if (!chapter.MienPhi)
            {
                var now = DateTime.Now;
                var hasAccess = await _context.TruyenDaMuas
                    .AnyAsync(p => p.MaNguoiDung == userId.Value && p.MaTruyen == id)
                    || await _context.NguoiDungGoiDocs.AnyAsync(p =>
                        p.MaNguoiDung == userId.Value
                        && p.TrangThai == "Active"
                        && p.NgayBatDau <= now
                        && p.NgayKetThuc >= now);

                if (!hasAccess)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Bạn chưa có quyền đọc chương này." });
            }

            var progress = await _context.TienDoDocs.FindAsync(userId.Value, id);
            if (progress == null)
            {
                progress = new TienDoDoc
                {
                    MaNguoiDung = userId.Value,
                    MaTruyen = id,
                    MaChuongGanNhat = chapter.MaChuong,
                    SoLanDoc = 1,
                    NgayCapNhat = DateTime.Now
                };
                _context.TienDoDocs.Add(progress);
            }
            else
            {
                if (progress.MaChuongGanNhat != chapter.MaChuong)
                    progress.SoLanDoc = (progress.SoLanDoc ?? 0) + 1;

                progress.MaChuongGanNhat = chapter.MaChuong;
                progress.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Đã lưu tiến độ đọc.",
                progress.MaTruyen,
                progress.MaChuongGanNhat,
                progress.SoLanDoc,
                progress.NgayCapNhat
            });
        }



    }

}
