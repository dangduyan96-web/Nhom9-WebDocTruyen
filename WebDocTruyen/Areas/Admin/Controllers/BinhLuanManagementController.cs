using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Areas.Admin.ViewModels;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BinhLuanManagementController : Controller
    {
        private const int PageSize = 10;
        private readonly AppDbContext _context;

        public BinhLuanManagementController(AppDbContext context)
        {
            _context = context;
        }

        private bool CheckAdmin()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan)) return false;

            var user = _context.NguoiDungs
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefault(x => x.TenDangNhap == taiKhoan);

            if (user == null || user.IsKhoa == true) return false;

            string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;
            return user.MaVaiTro == 1
                || tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult RedirectToLogin()
            => RedirectToAction("Login", "Account", new { area = "" });

        // GET: Admin/BinhLuanManagement?keyword=...&trangThai=all|hien|an&page=1
        public async Task<IActionResult> Index(string? keyword, string? trangThai, int page = 1)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            keyword = keyword?.Trim();
            trangThai = string.IsNullOrWhiteSpace(trangThai) ? "all" : trangThai;

            var query = _context.BinhLuanChuongs.AsNoTracking().AsQueryable();

            if (trangThai == "hien")
                query = query.Where(x => x.IsAn != true);
            else if (trangThai == "an")
                query = query.Where(x => x.IsAn == true);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.NoiDung.Contains(keyword)
                    || x.MaNguoiDungNavigation.TenDangNhap.Contains(keyword)
                    || (x.MaNguoiDungNavigation.HoTen != null && x.MaNguoiDungNavigation.HoTen.Contains(keyword))
                    || x.MaChuongNavigation.MaTruyenNavigation.TenTruyen.Contains(keyword));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .ThenByDescending(x => x.MaBinhLuan)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new BinhLuanAdminItemVM
                {
                    MaBinhLuan = x.MaBinhLuan,
                    NoiDung = x.NoiDung,
                    TenDangNhap = x.MaNguoiDungNavigation.TenDangNhap,
                    HoTen = x.MaNguoiDungNavigation.HoTen,
                    MaTruyen = x.MaChuongNavigation.MaTruyen,
                    TenTruyen = x.MaChuongNavigation.MaTruyenNavigation.TenTruyen,
                    SoThuTuChuong = x.MaChuongNavigation.SoThuTuChuong,
                    LaTraLoi = x.MaBinhLuanCha != null,
                    SoTraLoi = x.InverseMaBinhLuanChaNavigation.Count,
                    IsAn = x.IsAn == true,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword ?? "";
            ViewBag.TrangThai = trangThai;
            ViewBag.TongBinhLuan = await _context.BinhLuanChuongs.CountAsync();
            ViewBag.TongAn = await _context.BinhLuanChuongs.CountAsync(x => x.IsAn == true);

            return View(items);
        }

        // POST: Admin/BinhLuanManagement/ToggleAn/5 - ẩn hoặc hiện lại bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAn(int id, string? keyword, string? trangThai, int page = 1)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var binhLuan = await _context.BinhLuanChuongs
                .Include(x => x.InverseMaBinhLuanChaNavigation)
                .FirstOrDefaultAsync(x => x.MaBinhLuan == id);

            if (binhLuan == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận.";
                return RedirectToAction(nameof(Index), new { keyword, trangThai, page });
            }

            bool anMoi = binhLuan.IsAn != true;

            if (!anMoi && binhLuan.MaBinhLuanCha.HasValue)
            {
                // Không cho hiện lại câu trả lời khi bình luận gốc của nó vẫn đang bị ẩn
                bool gocAn = await _context.BinhLuanChuongs
                    .AnyAsync(x => x.MaBinhLuan == binhLuan.MaBinhLuanCha.Value && x.IsAn == true);
                if (gocAn)
                {
                    TempData["Error"] = "Bình luận gốc của câu trả lời này đang bị ẩn. Hãy hiện lại bình luận gốc trước.";
                    return RedirectToAction(nameof(Index), new { keyword, trangThai, page });
                }
            }

            binhLuan.IsAn = anMoi;

            // Ẩn bình luận gốc thì ẩn luôn các câu trả lời bên dưới
            int soTraLoiBiAn = 0;
            if (anMoi)
            {
                foreach (var traLoi in binhLuan.InverseMaBinhLuanChaNavigation.Where(r => r.IsAn != true))
                {
                    traLoi.IsAn = true;
                    soTraLoiBiAn++;
                }
            }

            await _context.SaveChangesAsync();

            if (!anMoi)
                TempData["Success"] = "Đã hiện lại bình luận.";
            else if (soTraLoiBiAn > 0)
                TempData["Success"] = $"Đã ẩn bình luận và {soTraLoiBiAn} câu trả lời bên dưới.";
            else
                TempData["Success"] = "Đã ẩn bình luận.";

            return RedirectToAction(nameof(Index), new { keyword, trangThai, page });
        }
    }
}
