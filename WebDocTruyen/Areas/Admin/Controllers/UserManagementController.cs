using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;

        public UserManagementController(AppDbContext context)
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

        // GET: Admin/UserManagement
        public async Task<IActionResult> Index()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var dsUser = await _context.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .OrderByDescending(u => u.MaNguoiDung)
                .ToListAsync();

            return View(dsUser);
        }
        // Khóa hoặc Mở khóa tài khoản nhanh
        public async Task<IActionResult> ToggleLock(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user != null)
            {
                // Không cho phép tự khóa chính tài khoản admin đang đăng nhập
                var currentTaiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (user.TenDangNhap == currentTaiKhoan)
                {
                    TempData["Error"] = "Bạn không thể khóa tài khoản của chính mình!";
                    return RedirectToAction(nameof(Index));
                }

                // Sửa lại chỗ này: dùng dấu ! trực tiếp vì IsKhoa là kiểu bool
                user.IsKhoa = !user.IsKhoa;

                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}