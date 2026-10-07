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
}
