using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;
using WebDocTruyen.ViewModels;

namespace WebDocTruyen.Controllers
{
    public class TruyenController : Controller
    {
        private readonly AppDbContext _context;

        public TruyenController(AppDbContext context)
        {
            _context = context;
        }

        // Link "Xem tất cả" ở trang chủ: tạm chuyển về trang chủ
        public IActionResult Index() => RedirectToAction("Index", "Home");

        // GET: /Truyen/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var truyen = await _context.Truyens
                .AsNoTracking()
                .Include(t => t.MaTheLoais)
                .FirstOrDefaultAsync(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();

            // Tăng lượt xem bằng 1 câu UPDATE
            await _context.Truyens
                .Where(t => t.MaTruyen == id)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.LuotXem, t => (t.LuotXem ?? 0) + 1));

            // Chỉ lấy thông tin chương, KHÔNG lấy NoiDung
            var chuongs = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == id)
                .OrderBy(c => c.SoThuTuChuong)
                .Select(c => new ChuongItemVM
                {
                    MaChuong = c.MaChuong,
                    SoThuTuChuong = c.SoThuTuChuong,
                    TieuDe = c.TieuDe,
                    MienPhi = c.MienPhi,
                    LuotXem = c.LuotXem,
                    NgayPhatHanh = c.NgayPhatHanh
                })
                .ToListAsync();

            var sao = await _context.DanhGiaTruyens
                .Where(d => d.MaTruyen == id && d.SoSao != null)
                .Select(d => d.SoSao!.Value)
                .ToListAsync();

            var vm = new TruyenDetailViewModel
            {
                Truyen = truyen,
                TheLoais = truyen.MaTheLoais.Select(tl => tl.TenTheLoai).ToList(),
                Chuongs = chuongs,
                SoDanhGia = sao.Count,
                DiemTrungBinh = sao.Any() ? Math.Round(sao.Average(), 1) : 0,
                // Truyện miễn phí thì ai cũng đọc được (giá trị mặc định trong DB là "Free")
                CoQuyenDoc = truyen.ChinhSachTruyCap == "Free"
            };

            // Phần dành riêng cho người đã đăng nhập (cùng key Session với GetRecommend)
            int? userId = HttpContext.Session.GetInt32("UserId");
            vm.DaDangNhap = userId.HasValue;
            if (userId.HasValue)
            {
                vm.DaYeuThich = await _context.TruyenYeuThiches
                    .AnyAsync(y => y.MaNguoiDung == userId && y.MaTruyen == id);

                vm.MaChuongDangDoc = await _context.TienDoDocs
                    .Where(t => t.MaNguoiDung == userId && t.MaTruyen == id)
                    .Select(t => (int?)t.MaChuongGanNhat)
                    .FirstOrDefaultAsync();

                if (!vm.CoQuyenDoc)
                {
                    bool daMua = await _context.TruyenDaMuas
                        .AnyAsync(m => m.MaNguoiDung == userId && m.MaTruyen == id);

                    bool coGoi = await _context.NguoiDungGoiDocs
                        .AnyAsync(g => g.MaNguoiDung == userId
                                    && g.TrangThai == "Active"
                                    && g.NgayKetThuc > DateTime.Now);

                    vm.CoQuyenDoc = daMua || coGoi;
                }
            }

            return View(vm);
        }

        // POST: /Truyen/ToggleYeuThich  (AJAX) - bật/tắt theo dõi truyện
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleYeuThich(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { message = "Vui lòng đăng nhập để theo dõi truyện." });

            if (!await _context.Truyens.AnyAsync(t => t.MaTruyen == id))
                return NotFound(new { message = "Không tìm thấy truyện." });

            var yeuThich = await _context.TruyenYeuThiches
                .FirstOrDefaultAsync(y => y.MaNguoiDung == userId.Value && y.MaTruyen == id);

            bool daYeuThich;
            if (yeuThich == null)
            {
                _context.TruyenYeuThiches.Add(new TruyenYeuThich
                {
                    MaNguoiDung = userId.Value,
                    MaTruyen = id,
                    NgayTao = DateTime.Now
                });
                daYeuThich = true;
            }
            else
            {
                _context.TruyenYeuThiches.Remove(yeuThich);
                daYeuThich = false;
            }
            await _context.SaveChangesAsync();

            // Cập nhật số lượt thích của truyện theo số người đang theo dõi
            int tong = await _context.TruyenYeuThiches.CountAsync(y => y.MaTruyen == id);
            await _context.Truyens
                .Where(t => t.MaTruyen == id)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.LuotThich, tong));

            return Json(new { daYeuThich, luotThich = tong });
        }

        // GET: /Truyen/YeuThich - danh sách truyện đang theo dõi
        [HttpGet]
        public async Task<IActionResult> YeuThich()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var ds = await _context.TruyenYeuThiches
                .Where(y => y.MaNguoiDung == userId.Value)
                .OrderByDescending(y => y.NgayTao)
                .Select(y => new YeuThichItemVM
                {
                    MaTruyen = y.MaTruyen,
                    TenTruyen = y.MaTruyenNavigation.TenTruyen,
                    AnhBia = y.MaTruyenNavigation.AnhBia,
                    TacGia = y.MaTruyenNavigation.TacGia,
                    TrangThai = y.MaTruyenNavigation.TrangThai,
                    LuotXem = y.MaTruyenNavigation.LuotXem ?? 0,
                    SoChuong = y.MaTruyenNavigation.ChuongTruyens.Count,
                    NgayTheoDoi = y.NgayTao
                })
                .ToListAsync();

            return View(ds);
        }
    }
}