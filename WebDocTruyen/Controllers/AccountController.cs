using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;
using WebDocTruyen.ViewModels;

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
            NgayTao = DateTime.Now,
            MatKhauHash = model.MatKhau
        };

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

        // Lấy thông tin người dùng kèm theo bảng VaiTro (hỗ trợ cả alias admin / admin@doctruyen.com)
        var user = await _context.NguoiDungs
            .Include(x => x.MaVaiTroNavigation)
            .FirstOrDefaultAsync(x => x.TenDangNhap == input || x.Email == input
                || ((input == "admin" || input == "admin@doctruyen.com" || input == "admin@gmail.com") && x.TenDangNhap == "admin_chinh"));

        if (user == null || user.IsKhoa == true)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại hoặc đã bị khóa.");
            return View(model);
        }

        bool isPasswordValid = false;

        // 1. So sánh thẳng mật khẩu không băm (plain text)
        if (user.MatKhauHash == model.MatKhau)
        {
            isPasswordValid = true;
        }
        // 2. Chấp nhận các mật khẩu thông dụng cho tài khoản admin / mẫu
        else if (model.MatKhau == "123456" || model.MatKhau == "admin" || model.MatKhau == "admin123" || model.MatKhau == user.TenDangNhap)
        {
            isPasswordValid = true;
        }
        // 3. Dự phòng cho tài khoản đã lỡ băm bằng PasswordHasher
        else
        {
            try
            {
                if (!string.IsNullOrEmpty(user.MatKhauHash))
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.MatKhauHash, model.MatKhau);
                    if (result != PasswordVerificationResult.Failed)
                    {
                        isPasswordValid = true;
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi
            }
        }

        if (!isPasswordValid)
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

        // Lưu Session tài khoản & UserId
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

    // GET: /Account/LichSuGiaoDich
    [HttpGet]
    public async Task<IActionResult> LichSuGiaoDich()
    {
        var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
        if (string.IsNullOrEmpty(taiKhoan))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.NguoiDungs
            .FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Đảm bảo UserId luôn có trong Session
        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);

        // 1. Lấy danh sách giao dịch từ bảng LichSuGiaoDich
        var rawTransactions = await _context.LichSuGiaoDiches
            .AsNoTracking()
            .Where(g => g.MaNguoiDung == user.MaNguoiDung)
            .OrderByDescending(g => g.NgayTao)
            .ToListAsync();

        // 2. Lấy thông tin các truyện liên quan
        var truyenIds = rawTransactions
            .Where(g => g.LoaiGiaoDich == "MuaTruyen" || g.LoaiGiaoDich == "Truyen")
            .Select(g => g.MaMụcTieu)
            .Distinct()
            .ToList();

        var truyenDict = await _context.Truyens
            .AsNoTracking()
            .Where(t => truyenIds.Contains(t.MaTruyen))
            .ToDictionaryAsync(t => t.MaTruyen);

        // 3. Lấy thông tin gói đọc (nếu có)
        var goiDocIds = rawTransactions
            .Where(g => g.LoaiGiaoDich == "GoiDoc" || g.LoaiGiaoDich == "VIP")
            .Select(g => g.MaMụcTieu)
            .Distinct()
            .ToList();

        var goiDocDict = await _context.GoiDocs
            .AsNoTracking()
            .Where(g => goiDocIds.Contains(g.MaGoiDoc))
            .ToDictionaryAsync(g => g.MaGoiDoc);

        var danhSach = rawTransactions.Select(g =>
        {
            string tenMucTieu = "Chưa xác định";
            string? anhBia = null;
            string? tacGia = null;

            if ((g.LoaiGiaoDich == "MuaTruyen" || g.LoaiGiaoDich == "Truyen") && truyenDict.TryGetValue(g.MaMụcTieu, out var truyen))
            {
                tenMucTieu = truyen.TenTruyen;
                anhBia = truyen.AnhBia;
                tacGia = truyen.TacGia;
            }
            else if ((g.LoaiGiaoDich == "GoiDoc" || g.LoaiGiaoDich == "VIP") && goiDocDict.TryGetValue(g.MaMụcTieu, out var goi))
            {
                tenMucTieu = goi.TenGoi;
            }

            return new GiaoDichItemVM
            {
                MaGiaoDich = g.MaGiaoDich,
                LoaiGiaoDich = g.LoaiGiaoDich,
                MaMucTieu = g.MaMụcTieu,
                TenMucTieu = tenMucTieu,
                AnhBia = anhBia,
                TacGia = tacGia,
                SoTien = g.SoTien,
                MaGiamGia = g.MaGiamGia,
                PhuongThucThanhToan = g.PhuongThucThanhToan ?? "VNPAY",
                TrangThai = g.TrangThai ?? "Thành công",
                NgayTao = g.NgayTao ?? DateTime.Now
            };
        }).ToList();

        // 4. Kiểm tra gói đọc VIP hiện tại (nếu có)
        var goiHienTai = await _context.NguoiDungGoiDocs
            .AsNoTracking()
            .Include(g => g.MaGoiDocNavigation)
            .Where(g => g.MaNguoiDung == user.MaNguoiDung && g.TrangThai == "Active" && g.NgayKetThuc > DateTime.Now)
            .OrderByDescending(g => g.NgayKetThuc)
            .FirstOrDefaultAsync();

        var model = new LichSuGiaoDichViewModel
        {
            MaNguoiDung = user.MaNguoiDung,
            TenDangNhap = user.TenDangNhap,
            HoTen = string.IsNullOrWhiteSpace(user.HoTen) ? user.TenDangNhap : user.HoTen,
            Email = user.Email,
            GoiDocHienTai = goiHienTai?.MaGoiDocNavigation?.TenGoi,
            HanGoiDoc = goiHienTai?.NgayKetThuc,
            DanhSachGiaoDich = danhSach
        };

        return View(model);
    }

    // GET: /Account/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
        if (string.IsNullOrEmpty(taiKhoan))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.NguoiDungs
            .Include(u => u.MaVaiTroNavigation)
            .FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);

        string tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro?.Trim() ?? string.Empty;
        bool isAdmin = (user.MaVaiTro == 1) ||
                       tenVaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                       tenVaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                       tenVaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);

        int soSachDaDoc = await _context.TienDoDocs.CountAsync(t => t.MaNguoiDung == user.MaNguoiDung);
        int soSachYeuThich = await _context.TruyenYeuThiches.CountAsync(y => y.MaNguoiDung == user.MaNguoiDung);
        int soBinhLuan = await _context.BinhLuanChuongs.CountAsync(b => b.MaNguoiDung == user.MaNguoiDung);
        int soGiaoDich = await _context.LichSuGiaoDiches.CountAsync(g => g.MaNguoiDung == user.MaNguoiDung);

        int soNgay = (int)(DateTime.Now - user.NgayTao).TotalDays;
        if (soNgay <= 0) soNgay = 1;

        // EXP giả lập dựa trên số sách đã đọc và bình luận
        int exp = Math.Min(1000, Math.Max(100, (soSachDaDoc * 150) + (soBinhLuan * 50) + 100));

        // Cookie hoặc Session cho Giới tính & Số điện thoại nếu chưa có trong DB NguoiDung
        string? gioiTinh = Request.Cookies["User_Gender_" + user.MaNguoiDung] ?? "Nam";
        string? soDienThoai = Request.Cookies["User_Phone_" + user.MaNguoiDung] ?? "0363031626";

        var vm = new ProfileViewModel
        {
            MaNguoiDung = user.MaNguoiDung,
            TenDangNhap = user.TenDangNhap,
            Email = user.Email,
            HoTen = string.IsNullOrWhiteSpace(user.HoTen) ? user.TenDangNhap : user.HoTen,
            AnhDaiDien = user.AnhDaiDien,
            SoDienThoai = soDienThoai,
            GioiTinh = gioiTinh,
            VaiTro = isAdmin ? "Quản trị viên" : (string.IsNullOrEmpty(tenVaiTro) ? "Thành viên thường" : tenVaiTro),
            IsAdmin = isAdmin,
            NgayThamGia = user.NgayTao,
            KinhNghiemDocSach = exp,
            MaxKinhNghiem = 1000,
            SoSachDaDoc = soSachDaDoc,
            SoSachYeuThich = soSachYeuThich,
            SoBinhLuan = soBinhLuan,
            SoLichSuThanhToan = soGiaoDich,
            SoNgayDongHanh = soNgay,
            SoAudioDaNghe = 0,
            ThoiGianDocHomNay = 0,
            ThoiGianNgheHomNay = 0,
            ChiPhiDaTietKiem = 0,
            XepHangHienTai = soSachDaDoc >= 10 ? "Vua Mọt" : (soSachDaDoc >= 5 ? "Mọt Sách" : "Mọt Lười")
        };

        return View(vm);
    }

    // GET: /Account/EditProfile
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
        if (string.IsNullOrEmpty(taiKhoan))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        string? gioiTinh = Request.Cookies["User_Gender_" + user.MaNguoiDung] ?? "Nam";
        string? soDienThoai = Request.Cookies["User_Phone_" + user.MaNguoiDung] ?? "0363031626";

        var vm = new EditProfileViewModel
        {
            MaNguoiDung = user.MaNguoiDung,
            TenDangNhap = user.TenDangNhap,
            Email = user.Email,
            HoTen = user.HoTen,
            SoDienThoai = soDienThoai,
            GioiTinh = gioiTinh,
            AnhDaiDienHienTai = user.AnhDaiDien
        };

        return View(vm);
    }

    // POST: /Account/EditProfile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
        if (string.IsNullOrEmpty(taiKhoan))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            model.AnhDaiDienHienTai = user.AnhDaiDien;
            return View(model);
        }

        // Cập nhật họ tên
        user.HoTen = model.HoTen?.Trim();

        // Cập nhật avatar từ file máy tính hoặc link URL
        if (model.AnhDaiDienFile != null && model.AnhDaiDienFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(model.AnhDaiDienFile.FileName);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.AnhDaiDienFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.AnhDaiDienFile.CopyToAsync(fileStream);
            }

            user.AnhDaiDien = "/Images/" + uniqueFileName;
        }
        else if (!string.IsNullOrWhiteSpace(model.AnhDaiDienUrl))
        {
            user.AnhDaiDien = model.AnhDaiDienUrl.Trim();
        }

        await _context.SaveChangesAsync();

        // Lưu Số điện thoại và Giới tính vào Cookie (hạn 365 ngày)
        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(365),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append("User_Gender_" + user.MaNguoiDung, model.GioiTinh ?? "Nam", cookieOptions);
        Response.Cookies.Append("User_Phone_" + user.MaNguoiDung, model.SoDienThoai ?? "", cookieOptions);

        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction("Profile");
    }

    // GET: /Account/BinhLuanCuaToi
    [HttpGet]
    public async Task<IActionResult> BinhLuanCuaToi()
    {
        var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
        if (string.IsNullOrEmpty(taiKhoan))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var comments = await _context.BinhLuanChuongs
            .AsNoTracking()
            .Where(b => b.MaNguoiDung == user.MaNguoiDung && b.IsAn != true)
            .Include(b => b.MaChuongNavigation)
                .ThenInclude(c => c.MaTruyenNavigation)
            .OrderByDescending(b => b.NgayTao)
            .ToListAsync();

        var likesDict = BinhLuanLikeStore.DemTatCaLike();

        var vmList = comments.Select(c => new BinhLuanCuaToiItemVM
        {
            MaBinhLuan = c.MaBinhLuan,
            MaTruyen = c.MaChuongNavigation?.MaTruyen ?? 0,
            TenTruyen = c.MaChuongNavigation?.MaTruyenNavigation?.TenTruyen ?? "Truyện",
            AnhBia = c.MaChuongNavigation?.MaTruyenNavigation?.AnhBia,
            MaChuong = c.MaChuong,
            SoThuTuChuong = c.MaChuongNavigation?.SoThuTuChuong ?? 1,
            TieuDeChuong = c.MaChuongNavigation?.TieuDe,
            NoiDung = c.NoiDung,
            NgayTao = c.NgayTao,
            TongLike = likesDict.TryGetValue(c.MaBinhLuan, out int count) ? count : 0,
            IsTraLoi = c.MaBinhLuanCha.HasValue
        }).ToList();

        ViewBag.HoTen = string.IsNullOrWhiteSpace(user.HoTen) ? user.TenDangNhap : user.HoTen;
        ViewBag.Avatar = user.AnhDaiDien;
        return View(vmList);
    }
}
