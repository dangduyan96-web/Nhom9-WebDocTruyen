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

        // 1. Danh sách gói đọc
        public async Task<IActionResult> Index()
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var listGoiDoc = await _context.GoiDocs
                .Include(g => g.NguoiDungGoiDocs)
                .OrderBy(g => g.MaGoiDoc)
                .ToListAsync();
            return View(listGoiDoc);
        }

        // Bật / tạm ẩn gói đọc nhanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHoatDong(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var goi = await _context.GoiDocs.FindAsync(id);
            if (goi != null)
            {
                goi.IsHoatDong = goi.IsHoatDong != true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
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
                // Mặc định gói mới tạo sẽ được hoạt động
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
            // So sánh id trên URL với MaGoiDoc
            if (id != goiDoc.MaGoiDoc) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(goiDoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goiDoc);
        }
    }
}