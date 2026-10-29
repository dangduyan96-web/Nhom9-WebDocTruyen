using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChuongManagementController : Controller
    {
        private readonly AppDbContext _context;

        public ChuongManagementController(AppDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin (giống TruyenManagementController)
        private bool CheckAdmin()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan)) return false;

            var user = _context.NguoiDungs
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefault(x => x.TenDangNhap == taiKhoan);

            if (user == null || user.IsKhoa == true) return false;

            string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;

            return (user.MaVaiTro == 1) ||
                   tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                   tenVaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult RedirectLogin() =>
            RedirectToAction("Login", "Account", new { area = "" });

        private void BoQuaValidateNavigation()
        {
            ModelState.Remove("MaTruyenNavigation");
            ModelState.Remove("BinhLuanChuongs");
            ModelState.Remove("TienDoDocs");
        }

        // GET: Admin/ChuongManagement/Index?maTruyen=1 (Danh sách chương của 1 truyện)
        public async Task<IActionResult> Index(int? maTruyen)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (maTruyen == null) return RedirectToAction("Index", "TruyenManagement");

            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen == null) return NotFound();

            var dsChuong = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == maTruyen)
                .OrderBy(c => c.SoThuTuChuong)
                .Select(c => new ChuongTruyen
                {
                    MaChuong = c.MaChuong,
                    MaTruyen = c.MaTruyen,
                    SoThuTuChuong = c.SoThuTuChuong,
                    TieuDe = c.TieuDe,
                    MienPhi = c.MienPhi,
                    LuotXem = c.LuotXem,
                    NgayPhatHanh = c.NgayPhatHanh
                })
                .ToListAsync();   // Không load NoiDung cho nhẹ

            ViewBag.Truyen = truyen;
            return View(dsChuong);
        }

        // GET: Admin/ChuongManagement/Create?maTruyen=1
        public async Task<IActionResult> Create(int? maTruyen)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (maTruyen == null) return NotFound();

            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen == null) return NotFound();

            int soTiepTheo = (await _context.ChuongTruyens
                .Where(c => c.MaTruyen == maTruyen)
                .MaxAsync(c => (int?)c.SoThuTuChuong) ?? 0) + 1;

            ViewBag.Truyen = truyen;
            return View(new ChuongTruyen { MaTruyen = maTruyen.Value, SoThuTuChuong = soTiepTheo, MienPhi = true });
        }

        // POST: Admin/ChuongManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChuongTruyen chuong)
        {
            if (!CheckAdmin()) return RedirectLogin();
            BoQuaValidateNavigation();

            var truyen = await _context.Truyens.FindAsync(chuong.MaTruyen);
            if (truyen == null) return NotFound();

            if (chuong.SoThuTuChuong <= 0)
                ModelState.AddModelError(nameof(chuong.SoThuTuChuong), "Số thứ tự chương phải lớn hơn 0.");

            if (await _context.ChuongTruyens.AnyAsync(c => c.MaTruyen == chuong.MaTruyen && c.SoThuTuChuong == chuong.SoThuTuChuong))
                ModelState.AddModelError(nameof(chuong.SoThuTuChuong), "Số thứ tự chương này đã tồn tại trong truyện.");

            if (ModelState.IsValid)
            {
                chuong.LuotXem = 0;
                chuong.NgayPhatHanh = DateTime.Now;
                truyen.NgayCapNhat = DateTime.Now;

                _context.ChuongTruyens.Add(chuong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm chương mới.";
                return RedirectToAction(nameof(Index), new { maTruyen = chuong.MaTruyen });
            }

            ViewBag.Truyen = truyen;
            return View(chuong);
        }

        // GET: Admin/ChuongManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (id == null) return NotFound();

            var chuong = await _context.ChuongTruyens
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == id);
            if (chuong == null) return NotFound();

            ViewBag.Truyen = chuong.MaTruyenNavigation;
            return View(chuong);
        }

        // POST: Admin/ChuongManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChuongTruyen chuong)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (id != chuong.MaChuong) return NotFound();
            BoQuaValidateNavigation();

            var existing = await _context.ChuongTruyens
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == id);
            if (existing == null) return NotFound();

            if (chuong.SoThuTuChuong <= 0)
                ModelState.AddModelError(nameof(chuong.SoThuTuChuong), "Số thứ tự chương phải lớn hơn 0.");

            if (await _context.ChuongTruyens.AnyAsync(c => c.MaTruyen == existing.MaTruyen
                    && c.SoThuTuChuong == chuong.SoThuTuChuong && c.MaChuong != id))
                ModelState.AddModelError(nameof(chuong.SoThuTuChuong), "Số thứ tự chương này đã tồn tại trong truyện.");

            if (ModelState.IsValid)
            {
                existing.SoThuTuChuong = chuong.SoThuTuChuong;
                existing.TieuDe = chuong.TieuDe;
                existing.NoiDung = chuong.NoiDung;
                existing.MienPhi = chuong.MienPhi;
                existing.MaTruyenNavigation.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật chương.";
                return RedirectToAction(nameof(Index), new { maTruyen = existing.MaTruyen });
            }

            chuong.MaTruyen = existing.MaTruyen;
            ViewBag.Truyen = existing.MaTruyenNavigation;
            return View(chuong);
        }

        // GET: Admin/ChuongManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (id == null) return NotFound();

            var chuong = await _context.ChuongTruyens
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == id);
            if (chuong == null) return NotFound();

            return View(chuong);
        }

        // POST: Admin/ChuongManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckAdmin()) return RedirectLogin();

            var chuong = await _context.ChuongTruyens
                .Include(c => c.BinhLuanChuongs)
                .Include(c => c.TienDoDocs)
                .FirstOrDefaultAsync(c => c.MaChuong == id);
            if (chuong == null) return RedirectToAction("Index", "TruyenManagement");

            int maTruyen = chuong.MaTruyen;

            // Xóa dữ liệu phụ thuộc để không vướng khóa ngoại
            _context.BinhLuanChuongs.RemoveRange(chuong.BinhLuanChuongs);
            _context.TienDoDocs.RemoveRange(chuong.TienDoDocs);
            _context.ChuongTruyens.Remove(chuong);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa chương.";
            return RedirectToAction(nameof(Index), new { maTruyen });
        }
    }
}
