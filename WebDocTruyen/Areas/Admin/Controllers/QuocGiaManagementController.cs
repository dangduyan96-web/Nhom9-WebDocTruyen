using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuocGiaManagementController : Controller
    {
        private readonly AppDbContext _context;

        public QuocGiaManagementController(AppDbContext context)
        {
            _context = context;
        }

        private bool CheckAdmin()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan)) return false;

            var vaiTro = HttpContext.Session.GetString("VaiTro") ?? "";
            if (vaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                vaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                vaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var user = _context.NguoiDungs
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefault(x => x.TenDangNhap == taiKhoan);

            if (user == null || user.IsKhoa == true) return false;

            string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;
            return user.MaVaiTro == 1
                || tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Admin/QuocGiaManagement
        public IActionResult Index()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var list = QuocGiaStore.GetAll();
            return View(list);
        }

        // GET: /Admin/QuocGiaManagement/Create
        public IActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            return View(new QuocGiaItem());
        }

        // POST: /Admin/QuocGiaManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuocGiaItem model)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            if (string.IsNullOrWhiteSpace(model.TenQuocGia))
            {
                ModelState.AddModelError("TenQuocGia", "Vui lòng nhập tên quốc gia.");
                return View(model);
            }

            QuocGiaStore.Add(model);
            TempData["SuccessMessage"] = $"Đã thêm quốc gia '{model.TenQuocGia}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/QuocGiaManagement/Edit/5
        public IActionResult Edit(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var item = QuocGiaStore.GetById(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: /Admin/QuocGiaManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuocGiaItem model)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            if (id != model.MaQuocGia) return NotFound();

            if (string.IsNullOrWhiteSpace(model.TenQuocGia))
            {
                ModelState.AddModelError("TenQuocGia", "Vui lòng nhập tên quốc gia.");
                return View(model);
            }

            QuocGiaStore.Update(model);
            TempData["SuccessMessage"] = $"Đã cập nhật quốc gia '{model.TenQuocGia}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/QuocGiaManagement/Delete/5
        public IActionResult Delete(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var item = QuocGiaStore.GetById(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: /Admin/QuocGiaManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var item = QuocGiaStore.GetById(id);
            if (item != null)
            {
                QuocGiaStore.Delete(id);
                TempData["SuccessMessage"] = $"Đã xóa quốc gia '{item.TenQuocGia}'.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}