using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BaoCaoLoiController : Controller
    {
        private readonly AppDbContext _context;

        public BaoCaoLoiController(AppDbContext context)
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
            return (user.MaVaiTro == 1)
                || tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        private int? GetCurrentAdminId()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan)) return null;

            var user = _context.NguoiDungs.FirstOrDefault(x => x.TenDangNhap == taiKhoan);
            return user?.MaNguoiDung;
        }

        // 1. Danh sách báo cáo lỗi với bộ lọc và tìm kiếm (*US24)
        public async Task<IActionResult> Index(string? status, string? loaiLoi, string? search, int page = 1)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var query = _context.BaoCaoLois
                .Include(b => b.MaTruyenNavigation)
                .Include(b => b.MaChuongNavigation)
                .Include(b => b.MaNguoiDungNavigation)
                .Include(b => b.NguoiXuLyNavigation)
                .AsQueryable();

            // Thống kê nhanh các trạng thái
            ViewBag.CountTotal = await _context.BaoCaoLois.CountAsync();
            ViewBag.CountChoXuLy = await _context.BaoCaoLois.CountAsync(b => b.TrangThai == "Chờ xử lý");
            ViewBag.CountDangXuLy = await _context.BaoCaoLois.CountAsync(b => b.TrangThai == "Đang xử lý");
            ViewBag.CountDaGiaiQuyet = await _context.BaoCaoLois.CountAsync(b => b.TrangThai == "Đã giải quyết");
            ViewBag.CountDaHuy = await _context.BaoCaoLois.CountAsync(b => b.TrangThai == "Đã hủy");

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(b => b.TrangThai == status);
            }

            // Lọc theo loại lỗi
            if (!string.IsNullOrEmpty(loaiLoi) && loaiLoi != "All")
            {
                query = query.Where(b => b.LoaiLoi == loaiLoi);
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(b =>
                    (b.MaTruyenNavigation != null && b.MaTruyenNavigation.TenTruyen.ToLower().Contains(search)) ||
                    (b.MaChuongNavigation != null && b.MaChuongNavigation.TieuDe.ToLower().Contains(search)) ||
                    (b.MoTaChiTiet != null && b.MoTaChiTiet.ToLower().Contains(search)) ||
                    (b.TenNguoiBaoCao != null && b.TenNguoiBaoCao.ToLower().Contains(search)));
            }

            int pageSize = 8;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var list = await query
                .OrderByDescending(b => b.TrangThai == "Chờ xử lý")
                .ThenByDescending(b => b.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.CurrentLoaiLoi = loaiLoi ?? "All";
            ViewBag.CurrentSearch = search ?? "";

            return View(list);
        }

        // 2. Chi tiết báo cáo lỗi
        public async Task<IActionResult> Details(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var baoCao = await _context.BaoCaoLois
                .Include(b => b.MaTruyenNavigation)
                .Include(b => b.MaChuongNavigation)
                .Include(b => b.MaNguoiDungNavigation)
                .Include(b => b.NguoiXuLyNavigation)
                .FirstOrDefaultAsync(b => b.MaBaoCao == id);

            if (baoCao == null) return NotFound();

            return View(baoCao);
        }

        // 3. Cập nhật trạng thái xử lý báo cáo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string trangThai, string? ghiChuAdmin, string? returnUrl)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var baoCao = await _context.BaoCaoLois.FindAsync(id);
            if (baoCao != null)
            {
                baoCao.TrangThai = trangThai;
                baoCao.GhiChuAdmin = ghiChuAdmin;
                baoCao.NguoiXuLy = GetCurrentAdminId();
                if (trangThai == "Đã giải quyết" || trangThai == "Đã hủy")
                {
                    baoCao.NgayXuLy = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái báo cáo #{id} thành '{trangThai}' thành công!";
            }

            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa báo cáo (nếu spam hoặc sai lệch)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var baoCao = await _context.BaoCaoLois.FindAsync(id);
            if (baoCao != null)
            {
                _context.BaoCaoLois.Remove(baoCao);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa báo cáo lỗi #{id} thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
