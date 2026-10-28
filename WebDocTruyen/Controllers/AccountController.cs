using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<NguoiDung> _passwordHasher = new();

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.TenDangNhap = (model.TenDangNhap ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        model.HoTen = (model.HoTen ?? string.Empty).Trim();

        if (!ModelState.IsValid)
            return View(model);

        if (await _context.NguoiDungs.AnyAsync(x => x.TenDangNhap == model.TenDangNhap))
        {
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập này đã được sử dụng.");
            return View(model);
        }

        if (await _context.NguoiDungs.AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký.");
            return View(model);
        }

        var user = new NguoiDung
        {
            MaVaiTro = 4,
            TenDangNhap = model.TenDangNhap,
            Email = model.Email,
            HoTen = model.HoTen,
            IsKhoa = false,
            NgayTao = DateTime.Now
        };
        user.MatKhauHash = _passwordHasher.HashPassword(user, model.MatKhau);

        _context.NguoiDungs.Add(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản. Email hoặc tên đăng nhập có thể đã tồn tại.");
            return View(model);
        }

        ViewBag.Registered = true;
        ModelState.Clear();
        return View(new RegisterViewModel());
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var input = (model.TenDangNhapHoacEmail ?? string.Empty).Trim();

        // Lấy thông tin người dùng kèm theo bảng VaiTro
        var user = await _context.NguoiDungs
            .Include(x => x.MaVaiTroNavigation)
            .FirstOrDefaultAsync(x => x.TenDangNhap == input || x.Email == input);

        if (user == null || user.IsKhoa == true)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại hoặc đã bị khóa.");
            return View(model);
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.MatKhauHash, model.MatKhau);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
            return View(model);
        }

        // Kiểm tra xem có phải Admin không (Dựa vào MaVaiTro = 1 hoặc tên vai trò)
        string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;

        bool isAdmin = (user.MaVaiTro == 1) ||
                       tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                       tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                       tenVaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);

        // Lưu Session tài khoản
        HttpContext.Session.SetString("TaiKhoan", user.TenDangNhap);
        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung); // Các chức năng yêu thích, gợi ý, tiến độ đọc cần key này

        if (isAdmin)
        {
            // Nếu MaVaiTroNavigation chưa kịp load tên, gán luôn "Quản trị viên" vào Session để không bị lỗi phân quyền ở Dashboard
            if (string.IsNullOrEmpty(tenVaiTro))
            {
                tenVaiTro = "Quản trị viên";
            }
            HttpContext.Session.SetString("VaiTro", tenVaiTro);

            // Chuyển hướng thẳng vào trang quản trị Admin Area
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        // Người dùng thường
        HttpContext.Session.SetString("VaiTro", string.IsNullOrEmpty(tenVaiTro) ? "Thành viên" : tenVaiTro);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Xóa toàn bộ Session khi đăng xuất
        return RedirectToAction("Index", "Home");
    }
}
