using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Areas.Admin.ViewModels;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhuyenMaiController : Controller
    {
        private const int PageSize = 7;
        private static readonly Regex MaHopLe = new(@"^[A-Z0-9_-]{3,20}$");
        private readonly AppDbContext _context;

        public KhuyenMaiController(AppDbContext context)
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

        private static string ChuanHoaMa(string? ma)
            => new string((ma ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

        // Số lần mã đã được ghi nhận trong lịch sử giao dịch
        private Task<int> DemLuotDungAsync(string maGiamGia)
            => _context.LichSuGiaoDiches.CountAsync(g => g.MaGiamGia == maGiamGia);

        // Kiểm tra dữ liệu form, báo lỗi vào ModelState
        private async Task KiemTraAsync(KhuyenMaiFormViewModel vm, bool laTaoMoi)
        {
            if (laTaoMoi)
            {
                vm.MaGiamGia = ChuanHoaMa(vm.MaGiamGia);
                if (string.IsNullOrEmpty(vm.MaGiamGia))
                    ModelState.AddModelError("MaGiamGia", "Vui lòng nhập mã giảm giá.");
                else if (!MaHopLe.IsMatch(vm.MaGiamGia))
                    ModelState.AddModelError("MaGiamGia", "Mã giảm giá chỉ gồm chữ cái không dấu, số, dấu gạch ngang hoặc gạch dưới (3–20 ký tự).");
                else if (await _context.KhuyenMais.AnyAsync(k => k.MaGiamGia == vm.MaGiamGia))
                    ModelState.AddModelError("MaGiamGia", "Mã giảm giá này đã tồn tại.");
            }

            if (!vm.SoTienGiam.HasValue || vm.SoTienGiam.Value <= 0)
                ModelState.AddModelError("SoTienGiam", "Số tiền giảm phải lớn hơn 0.");

            if (!vm.NgayBatDau.HasValue)
                ModelState.AddModelError("NgayBatDau", "Vui lòng chọn ngày bắt đầu.");
            if (!vm.NgayKetThuc.HasValue)
                ModelState.AddModelError("NgayKetThuc", "Vui lòng chọn ngày kết thúc.");

            if (vm.NgayBatDau.HasValue && vm.NgayKetThuc.HasValue
                && vm.NgayKetThuc.Value.Date < vm.NgayBatDau.Value.Date)
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
        }

        // 1. Danh sách mã khuyến mãi (7 mục / trang)
        public async Task<IActionResult> Index(string? keyword, string? trangThai, int page = 1)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            keyword = keyword?.Trim();
            trangThai = string.IsNullOrWhiteSpace(trangThai) ? "all" : trangThai;
            var now = DateTime.Now;

            var query = _context.KhuyenMais.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(k => k.MaGiamGia.Contains(keyword));

            query = trangThai switch
            {
                "hieuluc" => query.Where(k => k.IsHoatDong == true && k.NgayBatDau <= now && k.NgayKetThuc >= now),
                "saptoi" => query.Where(k => k.IsHoatDong == true && k.NgayBatDau > now),
                "hethan" => query.Where(k => k.IsHoatDong == true && k.NgayKetThuc < now),
                "tamdung" => query.Where(k => k.IsHoatDong != true),
                _ => query
            };

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var list = await query
                .OrderByDescending(k => k.NgayKetThuc)
                .ThenByDescending(k => k.MaKhuyenMai)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Đếm lượt dùng của các mã trong trang hiện tại
            var cacMa = list.Select(k => k.MaGiamGia).ToList();
            var luotDungRaw = await _context.LichSuGiaoDiches
                .AsNoTracking()
                .Where(g => g.MaGiamGia != null && cacMa.Contains(g.MaGiamGia))
                .GroupBy(g => g.MaGiamGia!)
                .Select(g => new { Ma = g.Key, SoLan = g.Count() })
                .ToListAsync();

            ViewBag.LuotDung = luotDungRaw.ToDictionary(x => x.Ma, x => x.SoLan, StringComparer.OrdinalIgnoreCase);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword ?? "";
            ViewBag.TrangThai = trangThai;
            ViewBag.TongMa = await _context.KhuyenMais.CountAsync();
            ViewBag.DangHieuLuc = await _context.KhuyenMais.CountAsync(k => k.IsHoatDong == true && k.NgayBatDau <= now && k.NgayKetThuc >= now);

            return View(list);
        }

        // 2. Giao diện tạo mã mới
        public IActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToLogin();

            return View(new KhuyenMaiFormViewModel
            {
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddDays(30),
                IsHoatDong = true
            });
        }

        // 3. Xử lý tạo mã mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMaiFormViewModel vm)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            await KiemTraAsync(vm, laTaoMoi: true);
            if (!ModelState.IsValid) return View(vm);

            _context.KhuyenMais.Add(new KhuyenMai
            {
                MaGiamGia = vm.MaGiamGia!,
                SoTienGiam = vm.SoTienGiam!.Value,
                NgayBatDau = vm.NgayBatDau!.Value.Date,
                // Ngày kết thúc tính đến hết ngày đó
                NgayKetThuc = vm.NgayKetThuc!.Value.Date.AddDays(1).AddSeconds(-1),
                IsHoatDong = vm.IsHoatDong
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo mã giảm giá {vm.MaGiamGia}.";
            return RedirectToAction(nameof(Index));
        }

        // 4. Giao diện sửa mã (không cho đổi chính mã giảm giá)
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (id == null) return NotFound();

            var km = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhuyenMai == id);
            if (km == null) return NotFound();

            ViewBag.LuotDung = await DemLuotDungAsync(km.MaGiamGia);
            return View(new KhuyenMaiFormViewModel
            {
                MaKhuyenMai = km.MaKhuyenMai,
                MaGiamGia = km.MaGiamGia,
                SoTienGiam = km.SoTienGiam,
                NgayBatDau = km.NgayBatDau,
                NgayKetThuc = km.NgayKetThuc,
                IsHoatDong = km.IsHoatDong == true
            });
        }

        // 5. Xử lý lưu chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhuyenMaiFormViewModel vm)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (id != vm.MaKhuyenMai) return NotFound();

            var km = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id);
            if (km == null) return NotFound();

            await KiemTraAsync(vm, laTaoMoi: false);
            vm.MaGiamGia = km.MaGiamGia; // mã giảm giá giữ nguyên
            if (!ModelState.IsValid)
            {
                ViewBag.LuotDung = await DemLuotDungAsync(km.MaGiamGia);
                return View(vm);
            }

            km.SoTienGiam = vm.SoTienGiam!.Value;
            km.NgayBatDau = vm.NgayBatDau!.Value.Date;
            km.NgayKetThuc = vm.NgayKetThuc!.Value.Date.AddDays(1).AddSeconds(-1);
            km.IsHoatDong = vm.IsHoatDong;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật mã giảm giá {km.MaGiamGia}.";
            return RedirectToAction(nameof(Index));
        }

        // Bật / tạm dừng mã nhanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHoatDong(int id, string? keyword, string? trangThai, int page = 1)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var km = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id);
            if (km != null)
            {
                km.IsHoatDong = km.IsHoatDong != true;
                await _context.SaveChangesAsync();
                TempData["Success"] = km.IsHoatDong == true
                    ? $"Đã kích hoạt mã {km.MaGiamGia}."
                    : $"Đã tạm dừng mã {km.MaGiamGia}.";
            }
            return RedirectToAction(nameof(Index), new { keyword, trangThai, page });
        }

        // 6. GET: Admin/KhuyenMai/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (id == null) return NotFound();

            var km = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhuyenMai == id);
            if (km == null) return NotFound();

            ViewBag.LuotDung = await DemLuotDungAsync(km.MaGiamGia);
            return View(km);
        }

        // 7. POST: Admin/KhuyenMai/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var km = await _context.KhuyenMais.FirstOrDefaultAsync(k => k.MaKhuyenMai == id);
            if (km == null) return RedirectToAction(nameof(Index));

            // Mã đã có trong lịch sử giao dịch thì không xóa để giữ dữ liệu đối soát
            if (await DemLuotDungAsync(km.MaGiamGia) > 0)
            {
                TempData["Error"] = $"Mã {km.MaGiamGia} đã được dùng trong giao dịch nên không thể xóa. Hãy chọn \"Tạm dừng\" để ngừng áp dụng.";
                return RedirectToAction(nameof(Index));
            }

            _context.KhuyenMais.Remove(km);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa mã giảm giá {km.MaGiamGia}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
