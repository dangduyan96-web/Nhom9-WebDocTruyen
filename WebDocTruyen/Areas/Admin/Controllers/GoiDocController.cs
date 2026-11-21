using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GoiDocController : Controller
    {
        private readonly AppDbContext _context;

        public GoiDocController(AppDbContext context)
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

        // 1. Danh sách gói đọc có phân trang (7 mục/trang)
        public async Task<IActionResult> Index(int page = 1)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            int pageSize = 7;
            int totalItems = await _context.GoiDocs.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var listGoiDoc = await _context.GoiDocs
                .Include(g => g.NguoiDungGoiDocs)
                .OrderBy(g => g.MaGoiDoc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(listGoiDoc);
        }

        // Bật / tạm ẩn gói đọc nhanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHoatDong(int id, int page = 1)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var goi = await _context.GoiDocs.FindAsync(id);
            if (goi != null)
            {
                goi.IsHoatDong = goi.IsHoatDong != true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { page });
        }

        // 2. Giao diện Tạo gói mới
        public IActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToLogin();
            return View();
        }

        // 3. Xử lý Thêm mới gói đọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GoiDoc goiDoc)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (ModelState.IsValid)
            {
                goiDoc.IsHoatDong = true;
                _context.GoiDocs.Add(goiDoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goiDoc);
        }

        // 4. Giao diện Chỉnh sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (id == null) return NotFound();

            var goiDoc = await _context.GoiDocs.FindAsync(id);
            if (goiDoc == null) return NotFound();

            return View(goiDoc);
        }

        // 5. Xử lý Lưu chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GoiDoc goiDoc)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (id != goiDoc.MaGoiDoc) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(goiDoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goiDoc);
        }

        // 6. GET: Admin/GoiDoc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (id == null) return NotFound();

            var goiDoc = await _context.GoiDocs
                .Include(g => g.NguoiDungGoiDocs)
                .FirstOrDefaultAsync(m => m.MaGoiDoc == id);

            if (goiDoc == null) return NotFound();

            return View(goiDoc);
        }

        // 7. POST: Admin/GoiDoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var goiDoc = await _context.GoiDocs
                .Include(g => g.NguoiDungGoiDocs)
                .FirstOrDefaultAsync(g => g.MaGoiDoc == id);

            if (goiDoc != null)
            {
                _context.NguoiDungGoiDocs.RemoveRange(goiDoc.NguoiDungGoiDocs);
                _context.GoiDocs.Remove(goiDoc);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}