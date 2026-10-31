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

        // GET: /Truyen/Doc/5
<<<<<<< HEAD
        // Hiển thị nội dung chương và tự lưu tiến độ cho người dùng đã đăng nhập.
=======
        // Hiển thị nội dung chương & Giới hạn khách chưa đăng nhập chỉ được đọc 3 chương đầu (US04)
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
        public async Task<IActionResult> Doc(int id)
        {
            var chuong = await _context.ChuongTruyens
                .AsNoTracking()
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == id);

            if (chuong == null) return NotFound();

            var truyen = chuong.MaTruyenNavigation;
<<<<<<< HEAD
            if (!chuong.MienPhi && truyen.ChinhSachTruyCap != "Free")
            {
                var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (string.IsNullOrWhiteSpace(taiKhoan))
                    return RedirectToAction("Login", "Account");

                var userId = await _context.NguoiDungs
=======

            // Lấy tài khoản & UserId từ Session
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            int? userId = HttpContext.Session.GetInt32("UserId");
            bool isLoggedIn = !string.IsNullOrWhiteSpace(taiKhoan) || userId.HasValue;

            // ================= US04: XỬ LÝ KHÁCH VẮNG LAI (GUEST USER) =================
            // Giới hạn chỉ cho phép đọc miễn phí 3 chương đầu (SoThuTuChuong <= 3)
            if (!isLoggedIn && chuong.SoThuTuChuong > 3)
            {
                TempData["Message"] = "Bạn cần đăng nhập để tiếp tục đọc các chương từ chương 4 trở đi!";
                return RedirectToAction("Login", "Account");
            }
            // =========================================================================

            // Kiểm tra phân quyền đối với truyện trả phí / chương mất phí
            if (!chuong.MienPhi && truyen.ChinhSachTruyCap != "Free")
            {
                if (!isLoggedIn)
                    return RedirectToAction("Login", "Account");

                var uId = userId ?? await _context.NguoiDungs
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
                    .Where(u => u.TenDangNhap == taiKhoan)
                    .Select(u => (int?)u.MaNguoiDung)
                    .SingleOrDefaultAsync();

<<<<<<< HEAD
                var daMua = userId.HasValue && await _context.TruyenDaMuas
                    .AnyAsync(m => m.MaNguoiDung == userId.Value && m.MaTruyen == truyen.MaTruyen);

                var coGoi = userId.HasValue && await _context.NguoiDungGoiDocs
                    .AnyAsync(g => g.MaNguoiDung == userId.Value
=======
                var daMua = uId.HasValue && await _context.TruyenDaMuas
                    .AnyAsync(m => m.MaNguoiDung == uId.Value && m.MaTruyen == truyen.MaTruyen);

                var coGoi = uId.HasValue && await _context.NguoiDungGoiDocs
                    .AnyAsync(g => g.MaNguoiDung == uId.Value
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
                                && g.TrangThai == "Active"
                                && g.NgayKetThuc > DateTime.Now);

                if (!daMua && !coGoi)
                    return RedirectToAction(nameof(Details), new { id = truyen.MaTruyen });
            }

<<<<<<< HEAD
=======
            // Tự động lưu tiến độ đọc nếu người dùng đã đăng nhập
            if (isLoggedIn && userId.HasValue)
            {
                var progress = await _context.TienDoDocs
                    .SingleOrDefaultAsync(p => p.MaNguoiDung == userId.Value && p.MaTruyen == truyen.MaTruyen);

                if (progress == null)
                {
                    _context.TienDoDocs.Add(new TienDoDoc
                    {
                        MaNguoiDung = userId.Value,
                        MaTruyen = truyen.MaTruyen,
                        MaChuongGanNhat = chuong.MaChuong,
                        SoLanDoc = 1,
                        NgayCapNhat = DateTime.Now
                    });
                }
                else
                {
                    progress.MaChuongGanNhat = chuong.MaChuong;
                    progress.SoLanDoc = (progress.SoLanDoc ?? 0) + 1;
                    progress.NgayCapNhat = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }

            // Lấy ID chương trước & chương sau để phân trang
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
            ViewBag.ChuongTruoc = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == chuong.MaTruyen && c.SoThuTuChuong < chuong.SoThuTuChuong)
                .OrderByDescending(c => c.SoThuTuChuong)
                .Select(c => (int?)c.MaChuong)
                .FirstOrDefaultAsync();

            ViewBag.ChuongSau = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == chuong.MaTruyen && c.SoThuTuChuong > chuong.SoThuTuChuong)
                .OrderBy(c => c.SoThuTuChuong)
                .Select(c => (int?)c.MaChuong)
                .FirstOrDefaultAsync();

            return View(chuong);
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
<<<<<<< HEAD
                // Truyện miễn phí thì ai cũng đọc được (giá trị mặc định trong DB là "Free")
                CoQuyenDoc = truyen.ChinhSachTruyCap == "Free"
            };

            // Phần dành riêng cho người đã đăng nhập (cùng key Session với GetRecommend)
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // Phiên đăng nhập hiện tại lưu tên tài khoản; tra MaNguoiDung để tải tiến độ đọc.
                var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (!string.IsNullOrWhiteSpace(taiKhoan))
                {
                    userId = await _context.NguoiDungs
                        .Where(u => u.TenDangNhap == taiKhoan)
                        .Select(u => (int?)u.MaNguoiDung)
                        .SingleOrDefaultAsync();
                }
            }

=======
                CoQuyenDoc = truyen.ChinhSachTruyCap == "Free"
            };

            int? userId = HttpContext.Session.GetInt32("UserId");
            vm.DaDangNhap = userId.HasValue;
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
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
<<<<<<< HEAD
    }
}
=======

        // POST: /Truyen/ToggleYeuThich (AJAX) - bật/tắt theo dõi truyện
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
>>>>>>> e8181448355fc186b244aabfc48d299a16714d59
