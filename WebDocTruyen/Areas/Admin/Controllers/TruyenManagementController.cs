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

        // Hàm kiểm tra quyền Admin qua Session
        // Hàm kiểm tra quyền Admin an toàn (Kiểm tra trực tiếp từ Database dựa vào Session tài khoản)
        private bool CheckAdmin()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan))
            {
                return false;
            }

            // Tìm thông tin người dùng trong DB kèm theo bảng VaiTro
            var user = _context.NguoiDungs
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefault(x => x.TenDangNhap == taiKhoan);

            if (user == null || user.IsKhoa == true)
            {
                return false;
            }

            // Kiểm tra xem có phải MaVaiTro == 1 hoặc tên vai trò là Admin / Quản trị viên (không phân biệt hoa thường)
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
        public IActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Truyen truyen, IFormFile? file)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });

            ModelState.Remove("MaTheLoais");
            ModelState.Remove("ChuongTruyens");
            ModelState.Remove("BinhLuanChuongs");
            ModelState.Remove("DanhGiaTruyens");
            ModelState.Remove("NguoiDung");
            ModelState.Remove("Slug");
            ModelState.Remove("AnhBia"); // Bỏ qua validate trường AnhBia vì lấy trực tiếp từ file upload

            if (ModelState.IsValid)
            {
                truyen.LuotXem = 0;
                truyen.NgayTao = DateTime.Now;
                if (string.IsNullOrEmpty(truyen.TrangThai)) truyen.TrangThai = "Đang tiến hành";
                if (string.IsNullOrEmpty(truyen.Slug)) truyen.Slug = Guid.NewGuid().ToString().Substring(0, 8);

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
                    truyen.AnhBia = "/Images/" + uniqueFileName; // Lưu đường dẫn tương đối để hiển thị trên web
                }
                else
                {
                    truyen.AnhBia = "/Images/default.jpg"; // Ảnh mặc định nếu không chọn file
                }

                _context.Add(truyen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(truyen);
        }

        // GET: Admin/TruyenManagement/Edit/5 (Form sửa truyện)
        public async Task<IActionResult> Edit(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (id == null) return NotFound();

            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen == null) return NotFound();

            return View(truyen);
        }

        // POST: Admin/TruyenManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Truyen truyen, IFormFile? file)
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
                    var existingTruyen = await _context.Truyens.FindAsync(id);
                    if (existingTruyen == null) return NotFound();

                    existingTruyen.TenTruyen = truyen.TenTruyen;
                    existingTruyen.TacGia = truyen.TacGia;
                    existingTruyen.MoTa = truyen.MoTa;
                    existingTruyen.TrangThai = truyen.TrangThai;
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