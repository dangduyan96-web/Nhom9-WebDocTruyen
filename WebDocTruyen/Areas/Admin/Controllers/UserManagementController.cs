using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Admin/UserManagement/Create
        public async Task<IActionResult> Create()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            ViewBag.VaiTroList = new SelectList(await _context.VaiTros.ToListAsync(), "MaVaiTro", "TenVaiTro");
            return View();
        }

        // POST: Admin/UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguoiDung nguoiDung, string matKhau)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            ModelState.Remove("MaVaiTroNavigation");
            ModelState.Remove("MatKhauHash");
            ModelState.Remove("BinhLuanChuongs");
            ModelState.Remove("DanhGiaTruyens");
            ModelState.Remove("LichSuGiaoDiches");
            ModelState.Remove("NguoiDungGoiDocs");
            ModelState.Remove("TienDoDocs");
            ModelState.Remove("TinNhanHoTros");
            ModelState.Remove("TruyenDaMuas");
            ModelState.Remove("TruyenYeuThiches");
            ModelState.Remove("Truyens");

            if (ModelState.IsValid)
            {
                if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == nguoiDung.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại.");
                    ViewBag.VaiTroList = new SelectList(await _context.VaiTros.ToListAsync(), "MaVaiTro", "TenVaiTro", nguoiDung.MaVaiTro);
                    return View(nguoiDung);
                }

                // Mã hóa mật khẩu chuẩn qua PasswordHasher
                string rawPassword = !string.IsNullOrEmpty(matKhau) ? matKhau : "123456";
                var passwordHasher = new PasswordHasher<NguoiDung>();
                nguoiDung.MatKhauHash = passwordHasher.HashPassword(nguoiDung, rawPassword);

                nguoiDung.IsKhoa = false;
                nguoiDung.NgayTao = DateTime.Now;

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VaiTroList = new SelectList(await _context.VaiTros.ToListAsync(), "MaVaiTro", "TenVaiTro", nguoiDung.MaVaiTro);
            return View(nguoiDung);
        }

        // GET: Admin/UserManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id == null) return NotFound();

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.VaiTroList = new SelectList(await _context.VaiTros.ToListAsync(), "MaVaiTro", "TenVaiTro", user.MaVaiTro);
            return View(user);
        }

        // POST: Admin/UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NguoiDung nguoiDung, string? newPassword)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id != nguoiDung.MaNguoiDung) return NotFound();

            ModelState.Remove("MaVaiTroNavigation");
            ModelState.Remove("MatKhauHash");
            ModelState.Remove("BinhLuanChuongs");
            ModelState.Remove("DanhGiaTruyens");
            ModelState.Remove("LichSuGiaoDiches");
            ModelState.Remove("NguoiDungGoiDocs");
            ModelState.Remove("TienDoDocs");
            ModelState.Remove("TinNhanHoTros");
            ModelState.Remove("TruyenDaMuas");
            ModelState.Remove("TruyenYeuThiches");
            ModelState.Remove("Truyens");

            if (ModelState.IsValid)
            {
                var existingUser = await _context.NguoiDungs.FindAsync(id);
                if (existingUser == null) return NotFound();

                existingUser.HoTen = nguoiDung.HoTen;
                existingUser.Email = nguoiDung.Email;
                existingUser.MaVaiTro = nguoiDung.MaVaiTro;
                existingUser.IsKhoa = nguoiDung.IsKhoa;

                // Nếu nhập mật khẩu mới, mã hóa và cập nhật
                if (!string.IsNullOrEmpty(newPassword))
                {
                    var passwordHasher = new PasswordHasher<NguoiDung>();
                    existingUser.MatKhauHash = passwordHasher.HashPassword(existingUser, newPassword);
                }

                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VaiTroList = new SelectList(await _context.VaiTros.ToListAsync(), "MaVaiTro", "TenVaiTro", nguoiDung.MaVaiTro);
            return View(nguoiDung);
        }

        // Khóa hoặc Mở khóa tài khoản nhanh
        public async Task<IActionResult> ToggleLock(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user != null)
            {
                var currentTaiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (user.TenDangNhap == currentTaiKhoan)
                {
                    TempData["Error"] = "Bạn không thể khóa tài khoản của chính mình!";
                    return RedirectToAction(nameof(Index));
                }

                user.IsKhoa = !user.IsKhoa;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Xóa tài khoản
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var user = await _context.NguoiDungs
                .Include(u => u.TienDoDocs)
                .Include(u => u.TruyenYeuThiches)
                .Include(u => u.TruyenDaMuas)
                .Include(u => u.LichSuGiaoDiches)
                .Include(u => u.BinhLuanChuongs)
                .Include(u => u.DanhGiaTruyens)
                .FirstOrDefaultAsync(u => u.MaNguoiDung == id);

            if (user != null)
            {
                var currentTaiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (user.TenDangNhap == currentTaiKhoan)
                {
                    TempData["Error"] = "Bạn không thể xóa tài khoản của chính mình đang đăng nhập!";
                    return RedirectToAction(nameof(Index));
                }

                _context.TienDoDocs.RemoveRange(user.TienDoDocs);
                _context.TruyenYeuThiches.RemoveRange(user.TruyenYeuThiches);
                _context.TruyenDaMuas.RemoveRange(user.TruyenDaMuas);
                _context.LichSuGiaoDiches.RemoveRange(user.LichSuGiaoDiches);
                _context.BinhLuanChuongs.RemoveRange(user.BinhLuanChuongs);
                _context.DanhGiaTruyens.RemoveRange(user.DanhGiaTruyens);

                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}