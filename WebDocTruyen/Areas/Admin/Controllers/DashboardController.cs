using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Areas.Admin.ViewModels;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");

            // Nếu chưa đăng nhập thì đá về trang đăng nhập chung
            if (string.IsNullOrEmpty(taiKhoan))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var vm = new DashboardViewModel();

            // 1. Thống kê tổng số truyện, người dùng, lượt xem
            vm.TongSoTruyen = await _context.Truyens.CountAsync();
            vm.TongSoNguoiDung = await _context.NguoiDungs.CountAsync();
            vm.TongLuotXem = await _context.Truyens.SumAsync(t => (long)(t.LuotXem ?? 0));

            // 2. Thống kê Doanh thu từ bảng LichSuGiaoDich
            var giaoDichThanhCong = await _context.LichSuGiaoDiches
                .Where(g => g.TrangThai == "Thành công" || g.TrangThai == "Success")
                .ToListAsync();

            vm.TongDoanhThu = giaoDichThanhCong.Sum(g => g.SoTien);

            var donMuaTruyen = giaoDichThanhCong.Where(g => g.LoaiGiaoDich == "MuaTruyen" || g.LoaiGiaoDich == "Truyen").ToList();
            var donGoiDoc = giaoDichThanhCong.Where(g => g.LoaiGiaoDich == "GoiDoc" || g.LoaiGiaoDich == "VIP").ToList();

            vm.DoanhThuMuaTruyen = donMuaTruyen.Sum(g => g.SoTien);
            vm.SoDonMuaTruyen = donMuaTruyen.Count;

            vm.DoanhThuGoiDoc = donGoiDoc.Sum(g => g.SoTien);
            vm.SoDonMuaGoiDoc = donGoiDoc.Count;

            // 3. Top 6 truyện có lượt xem cao nhất
            vm.TopTruyen = await _context.Truyens
                .OrderByDescending(t => t.LuotXem ?? 0)
                .Take(6)
                .Select(t => new TopTruyenLuotXemVM
                {
                    MaTruyen = t.MaTruyen,
                    TenTruyen = t.TenTruyen,
                    AnhBia = t.AnhBia,
                    TacGia = t.TacGia,
                    LuotXem = t.LuotXem ?? 0,
                    SoChuong = t.ChuongTruyens.Count,
                    LuotThich = t.LuotThich ?? 0
                })
                .ToListAsync();

            // 4. Lấy 8 giao dịch mới nhất
            var listGd = await _context.LichSuGiaoDiches
                .Include(g => g.MaNguoiDungNavigation)
                .OrderByDescending(g => g.NgayTao)
                .Take(8)
                .ToListAsync();

            // Cần lấy tên truyện/gói đọc để hiển thị
            var listTruyenMap = await _context.Truyens.ToDictionaryAsync(t => t.MaTruyen, t => t.TenTruyen);
            var listGoiDocMap = await _context.GoiDocs.ToDictionaryAsync(g => g.MaGoiDoc, g => g.TenGoi);

            vm.GiaoDichMoiNhat = listGd.Select(g =>
            {
                string mucTieu = "Chưa xác định";
                if (g.LoaiGiaoDich == "MuaTruyen" || g.LoaiGiaoDich == "Truyen")
                {
                    if (listTruyenMap.TryGetValue(g.MaMụcTieu, out var tenTruyen))
                        mucTieu = tenTruyen;
                }
                else
                {
                    if (listGoiDocMap.TryGetValue(g.MaMụcTieu, out var tenGoi))
                        mucTieu = tenGoi;
                }

                return new GiaoDichGanDayVM
                {
                    MaGiaoDich = g.MaGiaoDich,
                    TenNguoiDung = g.MaNguoiDungNavigation?.HoTen ?? g.MaNguoiDungNavigation?.TenDangNhap ?? "Khách",
                    LoaiGiaoDich = g.LoaiGiaoDich,
                    TenMucTieu = mucTieu,
                    SoTien = g.SoTien,
                    PhuongThuc = g.PhuongThucThanhToan ?? "VNPAY",
                    TrangThai = g.TrangThai,
                    NgayTao = g.NgayTao ?? DateTime.Now
                };
            }).ToList();

            // 5. Thống kê doanh thu theo 6 tháng gần nhất
            var now = DateTime.Now;
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var gds = giaoDichThanhCong.Where(g => g.NgayTao.HasValue 
                    && g.NgayTao.Value.Month == targetMonth.Month 
                    && g.NgayTao.Value.Year == targetMonth.Year).ToList();

                vm.DoanhThuTheoThang.Add(new DoanhThuTheoThangVM
                {
                    Thang = targetMonth.ToString("MM/yyyy"),
                    DoanhThu = gds.Sum(g => g.SoTien),
                    SoGiaoDich = gds.Count
                });
            }

            return View(vm);
        }
    }
}
