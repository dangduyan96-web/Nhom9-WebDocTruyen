using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebDocTruyen.Models; 

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
    }
}