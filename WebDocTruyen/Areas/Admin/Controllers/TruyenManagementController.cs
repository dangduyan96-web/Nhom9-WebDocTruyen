using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TruyenManagementController : Controller
    {
        private readonly AppDbContext _context;

        public TruyenManagementController(AppDbContext context)
        {
            _context = context;
        }

        // Hàm kiểm tra quyền Admin an toàn (Kiểm tra trực tiếp từ Database dựa vào Session tài khoản)
        private bool CheckAdmin()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan))
            {
                return false;
            }

            var user = _context.NguoiDungs
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefault(x => x.TenDangNhap == taiKhoan);

            if (user == null || user.IsKhoa == true)
            {
                return false;
            }

            string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;

            bool isAdmin = (user.MaVaiTro == 1) ||
                           tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                           tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                           tenVaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);

            return isAdmin;
        }

        // GET: Admin/TruyenManagement/Index (Danh sách truyện)
        public async Task<IActionResult> Index()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var dsTruyen = await _context.Truyens
                .OrderByDescending(t => t.MaTruyen)
                .ToListAsync();

            return View(dsTruyen);
        }

        // GET: Admin/TruyenManagement/Create (Form thêm truyện)
        public async Task<IActionResult> Create()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            // Lấy danh sách thể loại truyền ra View để hiển thị checkbox
            ViewBag.TheLoaiList = await _context.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync();
            return View();
        }

        // POST: Admin/TruyenManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Truyen truyen, IFormFile? file, int[] selectedTheLoais)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            ModelState.Remove("MaTheLoais");
            ModelState.Remove("ChuongTruyens");
            ModelState.Remove("BinhLuanChuongs");
            ModelState.Remove("DanhGiaTruyens");
            ModelState.Remove("NguoiDung");
            ModelState.Remove("Slug");
            ModelState.Remove("AnhBia");

            if (ModelState.IsValid)
            {
                truyen.LuotXem = 0;
                truyen.NgayTao = DateTime.Now;
                if (string.IsNullOrEmpty(truyen.TrangThai)) truyen.TrangThai = "Đang tiến hành";
                if (string.IsNullOrEmpty(truyen.Slug)) truyen.Slug = Guid.NewGuid().ToString().Substring(0, 8);
                truyen.ChinhSachTruyCap = string.IsNullOrEmpty(truyen.ChinhSachTruyCap) ? "Free" : truyen.ChinhSachTruyCap;
                truyen.GiaBan = truyen.ChinhSachTruyCap == "Paid" ? (truyen.GiaBan ?? 49000) : 0;

                // --- XỬ LÝ UPLOAD ẢNH BÌA ---
                if (file != null && file.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    truyen.AnhBia = "/Images/" + uniqueFileName;
                }
                else
                {
                    truyen.AnhBia = "/Images/default.jpg";
                }

                // --- XỬ LÝ GẮN THỂ LOẠI ĐƯỢC CHỌN ---
                if (selectedTheLoais != null && selectedTheLoais.Length > 0)
                {
                    truyen.MaTheLoais = await _context.TheLoais
                        .Where(t => selectedTheLoais.Contains(t.MaTheLoai))
                        .ToListAsync();
                }

                _context.Add(truyen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TheLoaiList = await _context.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync();
            return View(truyen);
        }

        // GET: Admin/TruyenManagement/Edit/5 (Form sửa truyện)
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id == null) return NotFound();

            var truyen = await _context.Truyens
                .Include(t => t.MaTheLoais)
                .FirstOrDefaultAsync(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();

            ViewBag.TheLoaiList = await _context.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync();
            // Lưu danh sách ID thể loại truyện đang có để check sẵn trong View
            ViewBag.SelectedTheLoais = truyen.MaTheLoais.Select(t => t.MaTheLoai).ToList();

            return View(truyen);
        }

        // POST: Admin/TruyenManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Truyen truyen, IFormFile? file, int[] selectedTheLoais)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id != truyen.MaTruyen) return NotFound();

            ModelState.Remove("MaTheLoais");
            ModelState.Remove("ChuongTruyens");
            ModelState.Remove("BinhLuanChuongs");
            ModelState.Remove("DanhGiaTruyens");
            ModelState.Remove("NguoiDung");
            ModelState.Remove("AnhBia");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTruyen = await _context.Truyens
                        .Include(t => t.MaTheLoais)
                        .FirstOrDefaultAsync(t => t.MaTruyen == id);

                    if (existingTruyen == null) return NotFound();

                    existingTruyen.TenTruyen = truyen.TenTruyen;
                    existingTruyen.TacGia = truyen.TacGia;
                    existingTruyen.MoTa = truyen.MoTa;
                    existingTruyen.TrangThai = truyen.TrangThai;
                    existingTruyen.ChinhSachTruyCap = string.IsNullOrEmpty(truyen.ChinhSachTruyCap) ? "Free" : truyen.ChinhSachTruyCap;
                    existingTruyen.GiaBan = truyen.ChinhSachTruyCap == "Paid" ? (truyen.GiaBan ?? 49000) : 0;
                    if (!string.IsNullOrEmpty(truyen.Slug))
                    {
                        existingTruyen.Slug = truyen.Slug;
                    }

                    // --- XỬ LÝ UPLOAD ẢNH MỚI NẾU CÓ CHỌN ---
                    if (file != null && file.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        existingTruyen.AnhBia = "/Images/" + uniqueFileName;
                    }

                    // --- CẬP NHẬT LẠI THỂ LOẠI (MANY-TO-MANY) ---
                    existingTruyen.MaTheLoais.Clear();
                    if (selectedTheLoais != null && selectedTheLoais.Length > 0)
                    {
                        var categories = await _context.TheLoais
                            .Where(t => selectedTheLoais.Contains(t.MaTheLoai))
                            .ToListAsync();
                        foreach (var cat in categories)
                        {
                            existingTruyen.MaTheLoais.Add(cat);
                        }
                    }

                    _context.Update(existingTruyen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Truyens.Any(e => e.MaTruyen == truyen.MaTruyen))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TheLoaiList = await _context.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync();
            ViewBag.SelectedTheLoais = selectedTheLoais?.ToList() ?? new List<int>();
            return View(truyen);
        }

        // GET: Admin/TruyenManagement/Delete/5 (Xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id == null) return NotFound();

            var truyen = await _context.Truyens
                .FirstOrDefaultAsync(m => m.MaTruyen == id);
            if (truyen == null) return NotFound();

            return View(truyen);
        }

        // POST: Admin/TruyenManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen != null)
            {
                _context.Truyens.Remove(truyen);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}