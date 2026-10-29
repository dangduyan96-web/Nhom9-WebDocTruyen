using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TheLoaiManagementController : Controller
    {
        private readonly AppDbContext _context;

        public TheLoaiManagementController(AppDbContext context)
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
            return (user.MaVaiTro == 1) || tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) || tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        // GET: Admin/TheLoaiManagement
        public async Task<IActionResult> Index()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var dsTheLoai = await _context.TheLoais.OrderByDescending(t => t.MaTheLoai).ToListAsync();
            return View(dsTheLoai);
        }

        // GET: Admin/TheLoaiManagement/Create
        public IActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TheLoai theLoai)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            ModelState.Remove("Truyens");
            ModelState.Remove("MaTruyens");

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(theLoai.Slug))
                {
                    theLoai.Slug = Guid.NewGuid().ToString().Substring(0, 8);
                }

                _context.Add(theLoai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(theLoai);
        }

        // GET: Admin/TheLoaiManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id == null) return NotFound();

            var theLoai = await _context.TheLoais.FindAsync(id);
            if (theLoai == null) return NotFound();

            return View(theLoai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TheLoai theLoai)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id != theLoai.MaTheLoai) return NotFound();

            ModelState.Remove("Truyens");
            ModelState.Remove("MaTruyens");

            if (ModelState.IsValid)
            {
                _context.Update(theLoai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(theLoai);
        }

        // GET: Admin/TheLoaiManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id == null) return NotFound();

            var theLoai = await _context.TheLoais.FirstOrDefaultAsync(m => m.MaTheLoai == id);
            if (theLoai == null) return NotFound();

            return View(theLoai);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var theLoai = await _context.TheLoais.FindAsync(id);
            if (theLoai != null)
            {
                _context.TheLoais.Remove(theLoai);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}