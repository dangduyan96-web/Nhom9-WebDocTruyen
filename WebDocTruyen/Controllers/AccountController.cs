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
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.TaiKhoan = (model.TaiKhoan ?? string.Empty).Trim();
        if (!ModelState.IsValid)
            return View(model);

        var normalizedAccount = model.TaiKhoan.ToLowerInvariant();
        var user = await _context.NguoiDungs.FirstOrDefaultAsync(x =>
            x.TenDangNhap.ToLower() == normalizedAccount || x.Email.ToLower() == normalizedAccount);

        if (user == null || user.IsKhoa || string.IsNullOrWhiteSpace(user.MatKhauHash))
        {
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không đúng hoặc tài khoản đã bị khóa.");
            return View(model);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.MatKhauHash, model.MatKhau);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không đúng hoặc tài khoản đã bị khóa.");
            return View(model);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.MatKhauHash = _passwordHasher.HashPassword(user, model.MatKhau);
            await _context.SaveChangesAsync();
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
        HttpContext.Session.SetString("TaiKhoan", user.TenDangNhap);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
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
}
