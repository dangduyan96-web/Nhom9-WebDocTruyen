using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoiDocApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GoiDocApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetGoiDocs()
        {
            // Lọc các gói đang hoạt động theo IsHoatDong
            var dsGoiDoc = await _context.GoiDocs
                .Where(g => g.IsHoatDong == true)
                .Select(g => new
                {
                    g.MaGoiDoc,
                    g.TenGoi,
                    g.SoNgayHieuLuc,
                    g.GiaTien,
                    g.MoTa
                })
                .ToListAsync();

            return Ok(dsGoiDoc);
        }
    }
}