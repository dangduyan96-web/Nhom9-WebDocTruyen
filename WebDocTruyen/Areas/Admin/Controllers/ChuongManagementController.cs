using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    // Model riêng cho form thêm/sửa chương (không bind trực tiếp entity để tránh lỗi validate)
    public class ChuongFormViewModel
    {
        public int MaChuong { get; set; }

        public int MaTruyen { get; set; }

        [Display(Name = "Số thứ tự chương")]
        public int? SoThuTuChuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề chương.")]
        [StringLength(255, ErrorMessage = "Tiêu đề tối đa 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }

        public bool MienPhi { get; set; } = true;
    }

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

        private async Task<int> LaySoChuongTiepTheo(int maTruyen)
        {
            var max = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == maTruyen)
                .MaxAsync(c => (int?)c.SoThuTuChuong);
            return (max ?? 0) + 1;
        }

        // ================= DANH SÁCH CHƯƠNG =================
        // GET: Admin/ChuongManagement/Index?maTruyen=1
        public async Task<IActionResult> Index(int? maTruyen)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (maTruyen == null) return RedirectToAction("Index", "TruyenManagement");

            var truyen = await _context.Truyens.AsNoTracking().FirstOrDefaultAsync(t => t.MaTruyen == maTruyen);
            if (truyen == null) return NotFound();

            var dsChuong = await _context.ChuongTruyens
                .AsNoTracking()
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

        // ================= THÊM CHƯƠNG =================
        // GET: Admin/ChuongManagement/Create?maTruyen=1
        public async Task<IActionResult> Create(int? maTruyen)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (maTruyen == null) return NotFound();

            var truyen = await _context.Truyens.AsNoTracking().FirstOrDefaultAsync(t => t.MaTruyen == maTruyen);
            if (truyen == null) return NotFound();

            ViewBag.Truyen = truyen;
            return View(new ChuongFormViewModel
            {
                MaTruyen = maTruyen.Value,
                SoThuTuChuong = await LaySoChuongTiepTheo(maTruyen.Value),
                MienPhi = true
            });
        }

        // POST: Admin/ChuongManagement/Create  -> lưu xong tự quay về danh sách chương
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChuongFormViewModel model)
        {
            if (!CheckAdmin()) return RedirectLogin();

            var truyen = await _context.Truyens.FirstOrDefaultAsync(t => t.MaTruyen == model.MaTruyen);
            if (truyen == null) return NotFound();

            // Bỏ trống số thứ tự thì tự lấy số kế tiếp
            if (model.SoThuTuChuong == null)
            {
                ModelState.Remove(nameof(model.SoThuTuChuong));
                model.SoThuTuChuong = await LaySoChuongTiepTheo(model.MaTruyen);
            }

            if (model.SoThuTuChuong <= 0)
                ModelState.AddModelError(nameof(model.SoThuTuChuong), "Số thứ tự chương phải lớn hơn 0.");
            else if (await _context.ChuongTruyens.AnyAsync(c => c.MaTruyen == model.MaTruyen && c.SoThuTuChuong == model.SoThuTuChuong))
                ModelState.AddModelError(nameof(model.SoThuTuChuong), "Số thứ tự chương này đã tồn tại trong truyện.");

            if (!ModelState.IsValid)
            {
                ViewBag.Truyen = truyen;
                return View(model);
            }

            try
            {
                var chuong = new ChuongTruyen
                {
                    MaTruyen = model.MaTruyen,
                    SoThuTuChuong = model.SoThuTuChuong!.Value,
                    TieuDe = model.TieuDe.Trim(),
                    NoiDung = model.NoiDung,
                    MienPhi = model.MienPhi,
                    LuotXem = 0,
                    NgayPhatHanh = DateTime.Now
                };
                _context.ChuongTruyens.Add(chuong);
                truyen.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm chương {chuong.SoThuTuChuong}: {chuong.TieuDe}";
                return RedirectToAction(nameof(Index), new { maTruyen = model.MaTruyen });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu chương: " + (ex.InnerException?.Message ?? ex.Message));
                ViewBag.Truyen = truyen;
                return View(model);
            }
        }

        // ================= SỬA CHƯƠNG =================
        // GET: Admin/ChuongManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (id == null) return NotFound();

            var chuong = await _context.ChuongTruyens
                .AsNoTracking()
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == id);
            if (chuong == null) return NotFound();

            ViewBag.Truyen = chuong.MaTruyenNavigation;
            return View(new ChuongFormViewModel
            {
                MaChuong = chuong.MaChuong,
                MaTruyen = chuong.MaTruyen,
                SoThuTuChuong = chuong.SoThuTuChuong,
                TieuDe = chuong.TieuDe,
                NoiDung = chuong.NoiDung,
                MienPhi = chuong.MienPhi
            });
        }

        // POST: Admin/ChuongManagement/Edit  -> lưu xong tự quay về danh sách chương
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ChuongFormViewModel model)
        {
            if (!CheckAdmin()) return RedirectLogin();

            var chuong = await _context.ChuongTruyens
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == model.MaChuong);
            if (chuong == null) return NotFound();

            model.MaTruyen = chuong.MaTruyen;

            if (model.SoThuTuChuong == null || model.SoThuTuChuong <= 0)
            {
                ModelState.Remove(nameof(model.SoThuTuChuong));
                ModelState.AddModelError(nameof(model.SoThuTuChuong), "Số thứ tự chương phải lớn hơn 0.");
            }
            else if (await _context.ChuongTruyens.AnyAsync(c => c.MaTruyen == chuong.MaTruyen
                        && c.SoThuTuChuong == model.SoThuTuChuong && c.MaChuong != model.MaChuong))
                ModelState.AddModelError(nameof(model.SoThuTuChuong), "Số thứ tự chương này đã tồn tại trong truyện.");

            if (!ModelState.IsValid)
            {
                ViewBag.Truyen = chuong.MaTruyenNavigation;
                return View(model);
            }

            try
            {
                chuong.SoThuTuChuong = model.SoThuTuChuong!.Value;
                chuong.TieuDe = model.TieuDe.Trim();
                chuong.NoiDung = model.NoiDung;
                chuong.MienPhi = model.MienPhi;
                chuong.MaTruyenNavigation.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã cập nhật chương {chuong.SoThuTuChuong}: {chuong.TieuDe}";
                return RedirectToAction(nameof(Index), new { maTruyen = chuong.MaTruyen });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu chương: " + (ex.InnerException?.Message ?? ex.Message));
                ViewBag.Truyen = chuong.MaTruyenNavigation;
                return View(model);
            }
        }

        // ================= XÓA CHƯƠNG =================
        // GET: Admin/ChuongManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckAdmin()) return RedirectLogin();
            if (id == null) return NotFound();

            var chuong = await _context.ChuongTruyens
                .AsNoTracking()
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
            try
            {
                // Xóa dữ liệu phụ thuộc để không vướng khóa ngoại
                _context.BinhLuanChuongs.RemoveRange(chuong.BinhLuanChuongs);
                _context.TienDoDocs.RemoveRange(chuong.TienDoDocs);
                _context.ChuongTruyens.Remove(chuong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa chương.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa chương: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Index), new { maTruyen });
        }
    }
}
