using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GoiDocController : Controller
    {
        private readonly AppDbContext _context;

        public GoiDocController(AppDbContext context) => _context = context;

        private async Task<bool> IsAdminAsync()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrWhiteSpace(taiKhoan)) return false;

            var user = await _context.NguoiDungs
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x => x.TenDangNhap == taiKhoan);
            if (user == null || user.IsKhoa) return false;

            var role = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;
            return user.MaVaiTro == 1
                || role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<IActionResult?> RequireAdminAsync()
        {
            if (await IsAdminAsync()) return null;
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        private async Task ValidatePackageAsync(GoiDoc goiDoc)
        {
            if (string.IsNullOrWhiteSpace(goiDoc.TenGoi))
                ModelState.AddModelError(nameof(goiDoc.TenGoi), "Vui lòng nhập tên gói.");
            else if (goiDoc.TenGoi.Trim().Length > 100)
                ModelState.AddModelError(nameof(goiDoc.TenGoi), "Tên gói không được vượt quá 100 ký tự.");
            else if (await _context.GoiDocs.AnyAsync(g => g.MaGoiDoc != goiDoc.MaGoiDoc && g.TenGoi.ToLower() == goiDoc.TenGoi.Trim().ToLower()))
                ModelState.AddModelError(nameof(goiDoc.TenGoi), "Tên gói này đã tồn tại.");

            if (goiDoc.GiaTien < 0)
                ModelState.AddModelError(nameof(goiDoc.GiaTien), "Giá gói phải lớn hơn hoặc bằng 0.");
            if (goiDoc.SoNgayHieuLuc <= 0)
                ModelState.AddModelError(nameof(goiDoc.SoNgayHieuLuc), "Thời hạn phải lớn hơn 0 ngày.");
            if (goiDoc.MoTa?.Length > 500)
                ModelState.AddModelError(nameof(goiDoc.MoTa), "Mô tả không được vượt quá 500 ký tự.");
        }

        public async Task<IActionResult> Index()
        {
            var denied = await RequireAdminAsync();
            if (denied != null) return denied;

            var packages = await _context.GoiDocs
                .OrderBy(g => g.GiaTien)
                .ThenBy(g => g.TenGoi)
                .ToListAsync();
            return View(packages);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var denied = await RequireAdminAsync();
            if (denied != null) return denied;
            return View(new GoiDoc { SoNgayHieuLuc = 30, IsHoatDong = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GoiDoc goiDoc)
        {
            var denied = await RequireAdminAsync();
            if (denied != null) return denied;

            goiDoc.TenGoi = goiDoc.TenGoi?.Trim()!;
            await ValidatePackageAsync(goiDoc);
            if (!ModelState.IsValid) return View(goiDoc);

            goiDoc.IsHoatDong = goiDoc.IsHoatDong == true;
            _context.GoiDocs.Add(goiDoc);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã tạo gói đọc mới.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var denied = await RequireAdminAsync();
            if (denied != null) return denied;
            if (id == null) return NotFound();

            var goiDoc = await _context.GoiDocs.FindAsync(id.Value);
            return goiDoc == null ? NotFound() : View(goiDoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GoiDoc goiDoc)
        {
            var denied = await RequireAdminAsync();
            if (denied != null) return denied;
            if (id != goiDoc.MaGoiDoc) return NotFound();

            goiDoc.TenGoi = goiDoc.TenGoi?.Trim()!;
            await ValidatePackageAsync(goiDoc);
            if (!ModelState.IsValid) return View(goiDoc);

            var existing = await _context.GoiDocs.FindAsync(id);
            if (existing == null) return NotFound();
            existing.TenGoi = goiDoc.TenGoi;
            existing.GiaTien = goiDoc.GiaTien;
            existing.SoNgayHieuLuc = goiDoc.SoNgayHieuLuc;
            existing.MoTa = goiDoc.MoTa?.Trim();
            existing.IsHoatDong = goiDoc.IsHoatDong == true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật gói đọc.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var denied = await RequireAdminAsync();
            if (denied != null) return denied;

            var package = await _context.GoiDocs
                .Include(g => g.NguoiDungGoiDocs)
                .FirstOrDefaultAsync(g => g.MaGoiDoc == id);
            if (package == null) return NotFound();

            if (package.NguoiDungGoiDocs.Count > 0)
            {
                TempData["Error"] = "Gói này đã có người đăng ký nên không thể xóa. Hãy chỉnh sửa và tạm ẩn gói để giữ lịch sử đăng ký.";
                return RedirectToAction(nameof(Index));
            }

            _context.GoiDocs.Remove(package);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa gói đọc.";
            return RedirectToAction(nameof(Index));
        }
    }
}
