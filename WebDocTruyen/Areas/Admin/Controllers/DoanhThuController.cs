using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DoanhThuController : Controller
    {
        private readonly AppDbContext _context;

        public DoanhThuController(AppDbContext context)
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

        private static bool IsMuaTruyen(string? loai)
            => loai == "MuaTruyen" || loai == "Truyen";

        private static bool IsGoiDoc(string? loai)
            => loai == "GoiDoc" || loai == "VIP";

        public async Task<IActionResult> Index(int? year)
        {
            if (!CheckAdmin())
                return RedirectToAction("Login", "Account", new { area = "" });

            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.SelectedYear = selectedYear;

            var gdThanhCong = await _context.LichSuGiaoDiches
                .Include(g => g.MaNguoiDungNavigation)
                .Where(g => g.TrangThai == "Thành công" || g.TrangThai == "Success")
                .ToListAsync();

            var gdInYear = gdThanhCong
                .Where(g => g.NgayTao.HasValue && g.NgayTao.Value.Year == selectedYear)
                .ToList();

            var gdTruyenInYear = gdInYear.Where(g => IsMuaTruyen(g.LoaiGiaoDich)).ToList();
            var gdGoiInYear = gdInYear.Where(g => IsGoiDoc(g.LoaiGiaoDich)).ToList();

            decimal doanhThuTruyen;
            int soLuotMuaTruyen;

            if (gdTruyenInYear.Any())
            {
                doanhThuTruyen = gdTruyenInYear.Sum(g => g.SoTien);
                soLuotMuaTruyen = gdTruyenInYear.Count;
            }
            else
            {
                var muaLeInYear = await _context.TruyenDaMuas
                    .Where(m => m.NgayMua.HasValue && m.NgayMua.Value.Year == selectedYear)
                    .ToListAsync();
                doanhThuTruyen = muaLeInYear.Sum(m => m.GiaDaTra);
                soLuotMuaTruyen = muaLeInYear.Count;
            }

            decimal doanhThuGoi;
            int soLuotMuaGoi;

            if (gdGoiInYear.Any())
            {
                doanhThuGoi = gdGoiInYear.Sum(g => g.SoTien);
                soLuotMuaGoi = gdGoiInYear.Count;
            }
            else
            {
                var goiInYear = await _context.NguoiDungGoiDocs
                    .Include(ng => ng.MaGoiDocNavigation)
                    .Where(ng => ng.NgayBatDau.Year == selectedYear)
                    .ToListAsync();
                doanhThuGoi = goiInYear.Sum(ng => ng.MaGoiDocNavigation?.GiaTien ?? 0);
                soLuotMuaGoi = goiInYear.Count;
            }

            decimal tongDoanhThu = doanhThuTruyen + doanhThuGoi;

            // Xu hướng 12 tháng: tách riêng Mua Lẻ Truyện và Gói Đọc
            var monthlyTruyen = new decimal[12];
            var monthlyGoi = new decimal[12];

            foreach (var g in gdTruyenInYear.Where(x => x.NgayTao.HasValue))
                monthlyTruyen[g.NgayTao!.Value.Month - 1] += g.SoTien;

            foreach (var g in gdGoiInYear.Where(x => x.NgayTao.HasValue))
                monthlyGoi[g.NgayTao!.Value.Month - 1] += g.SoTien;

            if (!gdTruyenInYear.Any())
            {
                var muaLeInYear = await _context.TruyenDaMuas
                    .Where(m => m.NgayMua.HasValue && m.NgayMua.Value.Year == selectedYear)
                    .ToListAsync();
                foreach (var m in muaLeInYear)
                    monthlyTruyen[m.NgayMua!.Value.Month - 1] += m.GiaDaTra;
            }

            if (!gdGoiInYear.Any())
            {
                var goiInYear = await _context.NguoiDungGoiDocs
                    .Include(ng => ng.MaGoiDocNavigation)
                    .Where(ng => ng.NgayBatDau.Year == selectedYear)
                    .ToListAsync();
                foreach (var ng in goiInYear)
                    monthlyGoi[ng.NgayBatDau.Month - 1] += ng.MaGoiDocNavigation?.GiaTien ?? 0;
            }

            var truyenMap = await _context.Truyens.ToDictionaryAsync(t => t.MaTruyen, t => t.TenTruyen);
            var goiMap = await _context.GoiDocs.ToDictionaryAsync(g => g.MaGoiDoc, g => g.TenGoi);

            // Danh sách tài khoản đã mua gói đọc trong năm (kèm tên gói & ngày)
            var taiKhoanMuaGoi = gdGoiInYear
                .OrderByDescending(g => g.NgayTao)
                .Select(g => new TaiKhoanGiaoDichVM
                {
                    TenDangNhap = g.MaNguoiDungNavigation?.TenDangNhap ?? $"user_{g.MaNguoiDung}",
                    HoTen = g.MaNguoiDungNavigation?.HoTen ?? "Độc giả",
                    TenMucTieu = goiMap.TryGetValue(g.MaMụcTieu, out var tg) ? tg : $"Gói #{g.MaMụcTieu}",
                    SoTien = g.SoTien,
                    PhuongThuc = g.PhuongThucThanhToan ?? "Mặc định",
                    NgayTao = g.NgayTao
                })
                .ToList();

            // Danh sách tài khoản đã mua lẻ truyện trong năm (kèm tên truyện & ngày)
            var taiKhoanMuaTruyen = gdTruyenInYear
                .OrderByDescending(g => g.NgayTao)
                .Select(g => new TaiKhoanGiaoDichVM
                {
                    TenDangNhap = g.MaNguoiDungNavigation?.TenDangNhap ?? $"user_{g.MaNguoiDung}",
                    HoTen = g.MaNguoiDungNavigation?.HoTen ?? "Độc giả",
                    TenMucTieu = truyenMap.TryGetValue(g.MaMụcTieu, out var tt) ? tt : $"Truyện #{g.MaMụcTieu}",
                    SoTien = g.SoTien,
                    PhuongThuc = g.PhuongThucThanhToan ?? "Mặc định",
                    NgayTao = g.NgayTao
                })
                .ToList();

            // Tất cả giao dịch thành công trong năm
            var danhSachGiaoDich = gdInYear
                .OrderByDescending(g => g.NgayTao)
                .Select(g =>
                {
                    bool isTruyen = IsMuaTruyen(g.LoaiGiaoDich);
                    string tenMucTieu = isTruyen
                        ? (truyenMap.TryGetValue(g.MaMụcTieu, out var tt) ? tt : $"Truyện #{g.MaMụcTieu}")
                        : (goiMap.TryGetValue(g.MaMụcTieu, out var tg) ? tg : $"Gói #{g.MaMụcTieu}");

                    return new GiaoDichDoanhThuVM
                    {
                        MaGiaoDich = g.MaGiaoDich,
                        TenDangNhap = g.MaNguoiDungNavigation?.TenDangNhap ?? $"user_{g.MaNguoiDung}",
                        NguoiDung = g.MaNguoiDungNavigation?.HoTen ?? g.MaNguoiDungNavigation?.TenDangNhap ?? "Khách",
                        Loai = isTruyen ? "Mua lẻ truyện" : "Gói đọc",
                        IsMuaLe = isTruyen,
                        TenMucTieu = tenMucTieu,
                        SoTien = g.SoTien,
                        PhuongThuc = g.PhuongThucThanhToan ?? "Mặc định",
                        NgayTao = g.NgayTao
                    };
                })
                .ToList();

            ViewBag.DoanhThuTruyen = doanhThuTruyen;
            ViewBag.SoLuotMuaTruyen = soLuotMuaTruyen;
            ViewBag.DoanhThuGoi = doanhThuGoi;
            ViewBag.SoLuotMuaGoi = soLuotMuaGoi;
            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.MonthlyTruyen = monthlyTruyen;
            ViewBag.MonthlyGoi = monthlyGoi;
            ViewBag.TaiKhoanMuaGoi = taiKhoanMuaGoi;
            ViewBag.TaiKhoanMuaTruyen = taiKhoanMuaTruyen;
            ViewBag.DanhSachGiaoDich = danhSachGiaoDich;

            return View();
        }
    }

    public class TaiKhoanGiaoDichVM
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenMucTieu { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public DateTime? NgayTao { get; set; }
    }

    public class GiaoDichDoanhThuVM
    {
        public int MaGiaoDich { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string NguoiDung { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty;
        public bool IsMuaLe { get; set; }
        public string TenMucTieu { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public DateTime? NgayTao { get; set; }
    }
}