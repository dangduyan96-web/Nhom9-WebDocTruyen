using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Action hiển thị trang chủ kèm bộ lọc và danh sách truyện
        public async Task<IActionResult> Index(int? maTheLoai, string trangThai, string sortOrder)
        {
            // Nạp danh sách Thể loại truyền sang ViewBag cho DropDownList lọc
            ViewBag.TheLoaiList = new SelectList(await _context.TheLoais.ToListAsync(), "MaTheLoai", "TenTheLoai", maTheLoai);
            ViewBag.MaTheLoai = maTheLoai;
            ViewBag.TrangThai = trangThai;
            ViewBag.SortOrder = sortOrder;

            IQueryable<Truyen> query = _context.Truyens
                .Include(t => t.ChuongTruyens)
                .Include(t => t.MaTheLoais);

            // 1. Lọc theo thể loại (Vì quan hệ N-N nên dùng Any để kiểm tra)
            if (maTheLoai.HasValue)
            {
                query = query.Where(t => t.MaTheLoais.Any(tl => tl.MaTheLoai == maTheLoai.Value));
            }

            // 2. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            // 3. Sắp xếp dữ liệu
            switch (sortOrder)
            {
                case "view_desc":
                    query = query.OrderByDescending(t => t.LuotXem);
                    break;
                case "name_asc":
                    query = query.OrderBy(t => t.TenTruyen);
                    break;
                default:
                    query = query.OrderByDescending(t => t.MaTruyen);
                    break;
            }

            // Lấy dữ liệu
            int takeCount = (maTheLoai.HasValue || !string.IsNullOrEmpty(trangThai) || !string.IsNullOrEmpty(sortOrder)) ? 24 : 8;
            var truyens = await query.Take(takeCount).ToListAsync();

            return View(truyens);
        }

        // Action AJAX gợi ý truyện
        [HttpGet]
        public async Task<IActionResult> GetRecommend()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId.HasValue)
                {
                    // Lấy truyện yêu thích của user kèm danh sách thể loại
                    var favoriteTruyen = await _context.TruyenYeuThiches
                        .Include(y => y.MaTruyenNavigation)
                            .ThenInclude(t => t.MaTheLoais)
                        .Where(y => y.MaNguoiDung == userId.Value)
                        .FirstOrDefaultAsync();

                    if (favoriteTruyen?.MaTruyenNavigation?.MaTheLoais != null && favoriteTruyen.MaTruyenNavigation.MaTheLoais.Any())
                    {
                        // Lấy mã thể loại đầu tiên trong danh sách thể loại của truyện yêu thích
                        var favCatId = favoriteTruyen.MaTruyenNavigation.MaTheLoais.First().MaTheLoai;

                        var personalizedProducts = await _context.Truyens
                            .Where(p => p.MaTheLoais.Any(tl => tl.MaTheLoai == favCatId))
                            .OrderBy(x => Guid.NewGuid())
                            .Take(4)
                            .Select(p => new
                            {
                                p.MaTruyen,
                                p.TenTruyen,
                                Anh = p.AnhBia, // Đã đổi p.Anh -> p.AnhBia
                                p.LuotXem
                            })
                            .ToListAsync();

                        if (personalizedProducts.Any())
                        {
                            return Json(personalizedProducts);
                        }
                    }
                }

                // Mặc định lấy ngẫu nhiên 4 truyện
                var defaultProducts = await _context.Truyens
                    .OrderBy(x => Guid.NewGuid())
                    .Take(4)
                    .Select(p => new
                    {
                        p.MaTruyen,
                        p.TenTruyen,
                        Anh = p.AnhBia, // Đã đổi p.Anh -> p.AnhBia
                        p.LuotXem
                    })
                    .ToListAsync();

                return Json(defaultProducts);
            }
            catch
            {
                return Json(new object[] { });
            }
        }
    }
}