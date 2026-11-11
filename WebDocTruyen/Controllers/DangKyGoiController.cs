using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers
{
    // US07: Đăng ký gói đọc theo tháng (thanh toán bằng mã QR)
    public class DangKyGoiController : Controller
    {
        private const string CHO = "Chờ thanh toán";
        private const string OK = "Thành công";
        private const string HET_HAN = "Đã hủy";

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public DangKyGoiController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private bool CoCauHinhNganHang =>
            !string.IsNullOrWhiteSpace(_config["Payment:BankId"]) &&
            !string.IsNullOrWhiteSpace(_config["Payment:AccountNo"]);

        private int HetHanPhut => int.TryParse(_config["Payment:HetHanPhut"], out var m) && m > 0 ? m : 15;

        private async Task<int?> ResolveUserIdAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue) return userId;

            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(taiKhoan)) return null;

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
            if (user == null) return null;

            HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
            return user.MaNguoiDung;
        }

        private static string NoiDungCK(int maGiaoDich) => $"GOIDOC{maGiaoDich}";

        private string KyToken(int maGiaoDich)
        {
            var key = Encoding.UTF8.GetBytes(_config["Payment:QrSecret"] ?? "qr-secret");
            using var h = new HMACSHA256(key);
            return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(maGiaoDich.ToString()))).ToLower();
        }

        // Kích hoạt gói sau khi đã thanh toán (idempotent)
        private async Task<bool> KichHoatAsync(LichSuGiaoDich gd)
        {
            if (gd.TrangThai == OK) return true;
            if (gd.TrangThai != CHO) return false;

            var goi = await _context.GoiDocs.FindAsync(gd.MaMụcTieu);
            if (goi == null) return false;

            var now = DateTime.Now;
            var conHan = await _context.NguoiDungGoiDocs
                .Where(x => x.MaNguoiDung == gd.MaNguoiDung && x.TrangThai == "Active" && x.NgayKetThuc > now)
                .OrderByDescending(x => x.NgayKetThuc)
                .FirstOrDefaultAsync();

            var batDau = conHan?.NgayKetThuc ?? now;
            _context.NguoiDungGoiDocs.Add(new NguoiDungGoiDoc
            {
                MaNguoiDung = gd.MaNguoiDung,
                MaGoiDoc = goi.MaGoiDoc,
                NgayBatDau = batDau,
                NgayKetThuc = batDau.AddDays(goi.SoNgayHieuLuc),
                TrangThai = "Active"
            });
            gd.TrangThai = OK;
            await _context.SaveChangesAsync();
            return true;
        }

        // GET: /DangKyGoi
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dsGoi = await _context.GoiDocs
                .AsNoTracking()
                .Where(g => g.IsHoatDong == true)
                .OrderBy(g => g.GiaTien)
                .ToListAsync();

            var userId = await ResolveUserIdAsync();
            ViewBag.DaDangNhap = userId.HasValue;

            if (userId.HasValue)
            {
                ViewBag.GoiHienTai = await _context.NguoiDungGoiDocs
                    .AsNoTracking()
                    .Include(x => x.MaGoiDocNavigation)
                    .Where(x => x.MaNguoiDung == userId.Value && x.TrangThai == "Active" && x.NgayKetThuc > DateTime.Now)
                    .OrderByDescending(x => x.NgayKetThuc)
                    .FirstOrDefaultAsync();
            }

            return View(dsGoi);
        }

        // POST: /DangKyGoi/TaoThanhToan (AJAX) - tạo giao dịch chờ và trả về dữ liệu mã QR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoThanhToan(int id, string? phuongThuc = "VNPAY")
        {
            var userId = await ResolveUserIdAsync();
            if (!userId.HasValue)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để đăng ký gói." });

            var goi = await _context.GoiDocs.FindAsync(id);
            if (goi == null || goi.IsHoatDong != true)
                return NotFound(new { success = false, message = "Gói đọc không tồn tại hoặc đã ngừng bán." });

            // Hủy các giao dịch chờ cũ của người dùng cho cùng gói để tránh trùng
            var cu = await _context.LichSuGiaoDiches
                .Where(g => g.MaNguoiDung == userId.Value && g.LoaiGiaoDich == "GoiDoc" && g.TrangThai == CHO)
                .ToListAsync();
            foreach (var c in cu) c.TrangThai = HET_HAN;

            var gd = new LichSuGiaoDich
            {
                MaNguoiDung = userId.Value,
                LoaiGiaoDich = "GoiDoc",
                MaMụcTieu = goi.MaGoiDoc,
                SoTien = goi.GiaTien,
                PhuongThucThanhToan = (phuongThuc?.Trim().ToLower()) == "paypal" ? "PayPal" : "VNPAY",
                TrangThai = CHO,
                NgayTao = DateTime.Now
            };
            _context.LichSuGiaoDiches.Add(gd);
            await _context.SaveChangesAsync();

            string noiDung = NoiDungCK(gd.MaGiaoDich);
            string? qrImage = null;
            string? qrText = null;
            bool isPayPal = gd.PhuongThucThanhToan == "PayPal";

            if (isPayPal)
            {
                // Đối với PayPal: Tạo QR dẫn đến trang xác nhận thanh toán PayPal chuẩn giao diện PayPal
                // Ưu tiên dùng IP mạng nội bộ để điện thoại cùng Wi-Fi quét mở được ngay
                string host = Request.Host.Value;
                if (host.StartsWith("localhost") || host.StartsWith("127.0.0.1"))
                {
                    // Lấy IP LAN nếu có thể để hỗ trợ quét từ điện thoại
                    try
                    {
                        var ipEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                        var localIp = ipEntry.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !a.ToString().StartsWith("127."));
                        if (localIp != null)
                        {
                            var port = Request.Host.Port.HasValue ? $":{Request.Host.Port}" : "";
                            host = $"{localIp}{port}";
                        }
                    }
                    catch { }
                }

                qrText = $"{Request.Scheme}://{host}/DangKyGoi/XacNhanPayPal/{gd.MaGiaoDich}?token={KyToken(gd.MaGiaoDich)}";
            }
            else if (CoCauHinhNganHang)
            {
                // VietQR: mọi app ngân hàng / ví đều quét được, tự điền sẵn số tiền + nội dung
                qrImage = $"https://img.vietqr.io/image/{_config["Payment:BankId"]}-{_config["Payment:AccountNo"]}-compact2.png" +
                          $"?amount={(long)gd.SoTien}&addInfo={Uri.EscapeDataString(noiDung)}" +
                          $"&accountName={Uri.EscapeDataString(_config["Payment:AccountName"] ?? "")}";
            }
            else
            {
                // Chế độ demo ngân hàng: QR chứa đường dẫn trang xác nhận thanh toán (quét bằng điện thoại cùng mạng)
                qrText = $"{Request.Scheme}://{Request.Host}/DangKyGoi/XacNhanQR/{gd.MaGiaoDich}?token={KyToken(gd.MaGiaoDich)}";
            }

            decimal tiGiaUSD = 25000m;
            decimal soTienUSD = Math.Round(gd.SoTien / tiGiaUSD, 2);

            return Ok(new
            {
                success = true,
                maGiaoDich = gd.MaGiaoDich,
                tenGoi = goi.TenGoi,
                soTien = gd.SoTien,
                soTienFormatted = gd.SoTien.ToString("N0") + " đ",
                soTienUSD = soTienUSD.ToString("0.00"),
                phuongThuc = gd.PhuongThucThanhToan,
                isPayPal,
                payPalCheckoutUrl = isPayPal ? qrText : null,
                noiDung,
                qrImage,
                qrText,
                demo = !CoCauHinhNganHang && !isPayPal,
                taiKhoan = CoCauHinhNganHang ? _config["Payment:AccountNo"] : null,
                tenTaiKhoan = CoCauHinhNganHang ? _config["Payment:AccountName"] : null,
                hetHanGiay = HetHanPhut * 60
            });
        }

        // GET: /DangKyGoi/TrangThai/5 (AJAX polling)
        [HttpGet]
        public async Task<IActionResult> TrangThai(int id)
        {
            var userId = await ResolveUserIdAsync();
            if (!userId.HasValue) return Unauthorized();

            var gd = await _context.LichSuGiaoDiches
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.MaNguoiDung == userId.Value && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null) return NotFound();

            if (gd.TrangThai == CHO && (DateTime.Now - (gd.NgayTao ?? DateTime.Now)).TotalMinutes > HetHanPhut)
            {
                gd.TrangThai = HET_HAN;
                await _context.SaveChangesAsync();
            }

            return Ok(new { trangThai = gd.TrangThai, daThanhToan = gd.TrangThai == OK, hetHan = gd.TrangThai == HET_HAN });
        }

        // POST: /DangKyGoi/Huy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var userId = await ResolveUserIdAsync();
            if (!userId.HasValue) return Unauthorized();

            var gd = await _context.LichSuGiaoDiches
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.MaNguoiDung == userId.Value && g.TrangThai == CHO);
            if (gd != null)
            {
                gd.TrangThai = HET_HAN;
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // POST: /DangKyGoi/KichHoatTest/5 - Dành cho kiểm thử/demo xác nhận giao dịch đã chuyển khoản thành công
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KichHoatTest(int id)
        {
            var userId = await ResolveUserIdAsync();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var gd = await _context.LichSuGiaoDiches
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.MaNguoiDung == userId.Value && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null) return NotFound(new { success = false, message = "Không tìm thấy giao dịch." });

            bool ok = await KichHoatAsync(gd);
            return Ok(new { success = ok, message = ok ? "Thành công" : "Không thể kích hoạt (có thể đã hủy hoặc hết hạn)." });
        }

        // GET: /DangKyGoi/XacNhanQR/5?token=... - trang mở khi quét QR ở chế độ demo
        [HttpGet]
        public async Task<IActionResult> XacNhanQR(int id, string? token)
        {
            if (token != KyToken(id)) return NotFound();

            var gd = await _context.LichSuGiaoDiches.AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null) return NotFound();

            var goi = await _context.GoiDocs.AsNoTracking().FirstOrDefaultAsync(g => g.MaGoiDoc == gd.MaMụcTieu);
            ViewBag.Token = token;
            ViewBag.TenGoi = goi?.TenGoi;
            return View(gd);
        }

        // POST: /DangKyGoi/XacNhanQR/5 - bấm "Thanh toán" trên điện thoại (demo)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanQR(int id, string token, bool dongY = true)
        {
            if (token != KyToken(id)) return NotFound();

            var gd = await _context.LichSuGiaoDiches.FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null) return NotFound();

            if (gd.TrangThai == CHO && (DateTime.Now - (gd.NgayTao ?? DateTime.Now)).TotalMinutes > HetHanPhut)
            {
                gd.TrangThai = HET_HAN;
                await _context.SaveChangesAsync();
            }

            bool ok = await KichHoatAsync(gd);
            TempData["Ket"] = ok ? "Thanh toán thành công! Bạn có thể quay lại máy tính." : "Giao dịch đã hết hạn hoặc không hợp lệ.";
            return RedirectToAction(nameof(XacNhanQR), new { id, token });
        }

        // GET: /DangKyGoi/XacNhanPayPal/5?token=... - Giao diện thanh toán chuẩn PayPal
        [HttpGet]
        public async Task<IActionResult> XacNhanPayPal(int id, string? token)
        {
            if (token != KyToken(id)) return NotFound();

            var gd = await _context.LichSuGiaoDiches.AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null) return NotFound();

            var goi = await _context.GoiDocs.AsNoTracking().FirstOrDefaultAsync(g => g.MaGoiDoc == gd.MaMụcTieu);
            decimal soTienUSD = Math.Round(gd.SoTien / 25000m, 2);

            ViewBag.Token = token;
            ViewBag.TenGoi = goi?.TenGoi ?? "Gói đọc WebDocTruyen";
            ViewBag.SoTienUSD = soTienUSD.ToString("0.00");
            return View(gd);
        }

        // POST: /DangKyGoi/XacNhanPayPal/5 - Xử lý khi bấm Pay with PayPal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanPayPal(int id, string token, bool confirm = true)
        {
            if (token != KyToken(id)) return NotFound();

            var gd = await _context.LichSuGiaoDiches.FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null) return NotFound();

            if (gd.TrangThai == CHO && (DateTime.Now - (gd.NgayTao ?? DateTime.Now)).TotalMinutes > HetHanPhut)
            {
                gd.TrangThai = HET_HAN;
                await _context.SaveChangesAsync();
            }

            bool ok = await KichHoatAsync(gd);
            TempData["PayPalSuccess"] = ok;
            TempData["PayPalMessage"] = ok 
                ? "You paid successfully! You can return to your computer screen." 
                : "The transaction has expired or could not be processed.";
            return RedirectToAction(nameof(XacNhanPayPal), new { id, token });
        }

        // POST: /DangKyGoi/Webhook - SePay / Casso gọi khi có tiền vào tài khoản (xác nhận tự động)
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Webhook([FromBody] WebhookBody body)
        {
            var apiKey = _config["Payment:WebhookApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey)) return NotFound();

            var auth = Request.Headers["Authorization"].ToString();
            if (auth != $"Apikey {apiKey}" && auth != apiKey) return Unauthorized();

            var m = Regex.Match(body?.Content ?? "", @"GOIDOC(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success) return Ok(new { success = true });

            int id = int.Parse(m.Groups[1].Value);
            var gd = await _context.LichSuGiaoDiches.FirstOrDefaultAsync(g => g.MaGiaoDich == id && g.LoaiGiaoDich == "GoiDoc");
            if (gd == null || (body?.TransferAmount ?? 0) < gd.SoTien) return Ok(new { success = true });

            await KichHoatAsync(gd);
            return Ok(new { success = true });
        }

        public class WebhookBody
        {
            public string? Content { get; set; }
            public decimal TransferAmount { get; set; }
        }
    }
}
