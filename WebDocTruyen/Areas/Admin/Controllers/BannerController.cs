using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BannerController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool CheckAdmin()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (!string.IsNullOrWhiteSpace(taiKhoan))
            {
                var user = _context.NguoiDungs
                    .Include(x => x.MaVaiTroNavigation)
                    .FirstOrDefault(x => x.TenDangNhap == taiKhoan);

                if (user != null && user.IsKhoa != true)
                {
                    string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;
                    if (user.MaVaiTro == 1 ||
                        tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                        tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                        tenVaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            var sessionRole = (HttpContext.Session.GetString("VaiTro") ?? string.Empty).Trim();
            return sessionRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || sessionRole.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase)
                || sessionRole.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string?> SaveBannerFileAsync(IFormFile? fileAnh)
        {
            if (fileAnh == null || fileAnh.Length == 0)
                return null;

            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "banners");
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(fileAnh.FileName)}";
            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await fileAnh.CopyToAsync(stream);

            return "/uploads/banners/" + fileName;
        }

        public IActionResult Index()
        {
            if (!CheckAdmin())
                return RedirectToAction("Login", "Account", new { area = "" });

            var list = _context.Banners
                .OrderBy(b => b.ThuTu)
                .ThenByDescending(b => b.MaBanner)
                .ToList();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner model, IFormFile? fileAnh)
        {
            if (!CheckAdmin())
                return RedirectToAction("Login", "Account", new { area = "" });

            if (model.ThuTu < 1)
            {
                TempData["Error"] = "Số thứ tự (TT) phải lớn hơn hoặc bằng 1.";
                return RedirectToAction(nameof(Index));
            }

            bool trungThuTu = await _context.Banners.AnyAsync(b => b.ThuTu == model.ThuTu);
            if (trungThuTu)
            {
                TempData["Error"] = $"Số thứ tự TT = {model.ThuTu} đã tồn tại ở banner khác. Vui lòng chọn số TT khác!";
                return RedirectToAction(nameof(Index));
            }

            var uploadedPath = await SaveBannerFileAsync(fileAnh);
            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                model.DuongDanAnh = uploadedPath;
            }

            if (string.IsNullOrWhiteSpace(model.DuongDanAnh))
            {
                TempData["Error"] = "Vui lòng chọn file ảnh hoặc nhập link ảnh banner.";
                return RedirectToAction(nameof(Index));
            }

            model.TieuDe = model.TieuDe?.Trim() ?? string.Empty;
            model.LienKet = string.IsNullOrWhiteSpace(model.LienKet) ? null : model.LienKet.Trim();
            model.NgayTao = DateTime.Now;

            _context.Banners.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã thêm banner mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Banner model, IFormFile? fileAnh)
        {
            if (!CheckAdmin())
                return RedirectToAction("Login", "Account", new { area = "" });

            var banner = await _context.Banners.FindAsync(model.MaBanner);
            if (banner == null)
            {
                TempData["Error"] = "Không tìm thấy banner cần sửa.";
                return RedirectToAction(nameof(Index));
            }

            if (model.ThuTu < 1)
            {
                TempData["Error"] = "Số thứ tự (TT) phải lớn hơn hoặc bằng 1.";
                return RedirectToAction(nameof(Index));
            }

            bool trungThuTu = await _context.Banners
                .AnyAsync(b => b.ThuTu == model.ThuTu && b.MaBanner != model.MaBanner);
            if (trungThuTu)
            {
                TempData["Error"] = $"Số thứ tự TT = {model.ThuTu} đã được dùng bởi banner khác. Vui lòng chọn số TT khác!";
                return RedirectToAction(nameof(Index));
            }

            var uploadedPath = await SaveBannerFileAsync(fileAnh);
            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                banner.DuongDanAnh = uploadedPath;
            }
            else if (!string.IsNullOrWhiteSpace(model.DuongDanAnh))
            {
                banner.DuongDanAnh = model.DuongDanAnh.Trim();
            }

            if (string.IsNullOrWhiteSpace(banner.DuongDanAnh))
            {
                TempData["Error"] = "Ảnh banner không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            banner.TieuDe = model.TieuDe?.Trim() ?? banner.TieuDe;
            banner.LienKet = string.IsNullOrWhiteSpace(model.LienKet) ? null : model.LienKet.Trim();
            banner.ThuTu = model.ThuTu;
            banner.HoatDong = model.HoatDong;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật banner thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!CheckAdmin())
                return RedirectToAction("Login", "Account", new { area = "" });

            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.HoatDong = !banner.HoatDong;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật trạng thái banner.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckAdmin())
                return RedirectToAction("Login", "Account", new { area = "" });

            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa banner.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}