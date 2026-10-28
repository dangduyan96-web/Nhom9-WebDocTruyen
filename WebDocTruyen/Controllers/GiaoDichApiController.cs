using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;
using WebDocTruyen.ViewModels;

namespace WebDocTruyen.Controllers
{
    [Route("api/GiaoDich")]
    [ApiController]
    public class GiaoDichApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GiaoDichApiController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy UserId từ query hoặc Session
        private int? ResolveUserId(int? userId)
        {
            if (userId.HasValue && userId.Value > 0)
            {
                return userId.Value;
            }

            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue)
            {
                return sessionUserId.Value;
            }

            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            if (!string.IsNullOrEmpty(taiKhoan))
            {
                var user = _context.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == taiKhoan);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
                    return user.MaNguoiDung;
                }
            }

            return null;
        }

        // GET: /api/GiaoDich/LichSu?userId=...&loaiGiaoDich=...&trangThai=...
        [HttpGet("LichSu")]
        public async Task<IActionResult> GetLichSuGiaoDich([FromQuery] int? userId, [FromQuery] string? loaiGiaoDich, [FromQuery] string? trangThai)
        {
            int? currentUserId = ResolveUserId(userId);
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để xem lịch sử giao dịch." });
            }

            var user = await _context.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.MaNguoiDung == currentUserId.Value);

            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var query = _context.LichSuGiaoDiches
                .AsNoTracking()
                .Where(g => g.MaNguoiDung == currentUserId.Value);

            if (!string.IsNullOrWhiteSpace(loaiGiaoDich) && !loaiGiaoDich.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(g => g.LoaiGiaoDich == loaiGiaoDich);
            }

            if (!string.IsNullOrWhiteSpace(trangThai) && !trangThai.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(g => g.TrangThai == trangThai);
            }

            var rawList = await query
                .OrderByDescending(g => g.NgayTao)
                .ToListAsync();

            // Lấy danh sách truyện liên quan
            var truyenIds = rawList
                .Where(g => g.LoaiGiaoDich == "MuaTruyen" || g.LoaiGiaoDich == "Truyen")
                .Select(g => g.MaMụcTieu)
                .Distinct()
                .ToList();

            var truyenDict = await _context.Truyens
                .AsNoTracking()
                .Where(t => truyenIds.Contains(t.MaTruyen))
                .ToDictionaryAsync(t => t.MaTruyen);

            // Lấy danh sách gói đọc liên quan (nếu có)
            var goiDocIds = rawList
                .Where(g => g.LoaiGiaoDich == "GoiDoc" || g.LoaiGiaoDich == "VIP")
                .Select(g => g.MaMụcTieu)
                .Distinct()
                .ToList();

            var goiDocDict = await _context.GoiDocs
                .AsNoTracking()
                .Where(g => goiDocIds.Contains(g.MaGoiDoc))
                .ToDictionaryAsync(g => g.MaGoiDoc);

            var result = rawList.Select(g =>
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

                return new
                {
                    maGiaoDich = g.MaGiaoDich,
                    maGiaoDichFormatted = $"#GD{g.MaGiaoDich:D5}",
                    loaiGiaoDich = g.LoaiGiaoDich,
                    tenLoaiGiaoDich = g.LoaiGiaoDich switch
                    {
                        "MuaTruyen" => "Mua truyện lẻ",
                        "Truyen" => "Mua truyện lẻ",
                        "GoiDoc" => "Gói đọc theo tháng",
                        "VIP" => "Đăng ký VIP",
                        _ => "Mua truyện lẻ"
                    },
                    maMucTieu = g.MaMụcTieu,
                    tenMucTieu = tenMucTieu,
                    anhBia = anhBia,
                    tacGia = tacGia,
                    soTien = g.SoTien,
                    soTienFormatted = string.Format("{0:N0} đ", g.SoTien),
                    maGiamGia = g.MaGiamGia,
                    phuongThucThanhToan = g.PhuongThucThanhToan ?? "VNPAY",
                    trangThai = g.TrangThai,
                    ngayTao = g.NgayTao ?? DateTime.Now,
                    ngayTaoFormatted = (g.NgayTao ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm")
                };
            }).ToList();

            var tongTien = result.Where(r => r.trangThai == "Thành công" || r.trangThai == "Success" || r.trangThai == "Hoàn thành").Sum(r => r.soTien);
            var soTruyenDaMua = result.Count(r => (r.loaiGiaoDich == "MuaTruyen" || r.loaiGiaoDich == "Truyen") && (r.trangThai == "Thành công" || r.trangThai == "Success" || r.trangThai == "Hoàn thành"));

            return Ok(new
            {
                success = true,
                nguoiDung = new
                {
                    maNguoiDung = user.MaNguoiDung,
                    tenDangNhap = user.TenDangNhap,
                    hoTen = user.HoTen,
                    email = user.Email
                },
                thongKe = new
                {
                    tongSoGiaoDich = result.Count,
                    tongTienDaChi = tongTien,
                    tongTienDaChiFormatted = string.Format("{0:N0} đ", tongTien),
                    soTruyenDaMua = soTruyenDaMua
                },
                data = result
            });
        }

        // GET: /api/GiaoDich/ChiTiet/{id}
        [HttpGet("ChiTiet/{id}")]
        public async Task<IActionResult> GetChiTietGiaoDich(int id)
        {
            var giaoDich = await _context.LichSuGiaoDiches
                .AsNoTracking()
                .Include(g => g.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(g => g.MaGiaoDich == id);

            if (giaoDich == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy giao dịch!" });
            }

            string tenMucTieu = "Chưa xác định";
            string? anhBia = null;
            if (giaoDich.LoaiGiaoDich == "MuaTruyen" || giaoDich.LoaiGiaoDich == "Truyen")
            {
                var truyen = await _context.Truyens.FindAsync(giaoDich.MaMụcTieu);
                if (truyen != null)
                {
                    tenMucTieu = truyen.TenTruyen;
                    anhBia = truyen.AnhBia;
                }
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    maGiaoDich = giaoDich.MaGiaoDich,
                    maGiaoDichFormatted = $"#GD{giaoDich.MaGiaoDich:D5}",
                    loaiGiaoDich = giaoDich.LoaiGiaoDich,
                    maMucTieu = giaoDich.MaMụcTieu,
                    tenMucTieu = tenMucTieu,
                    anhBia = anhBia,
                    soTien = giaoDich.SoTien,
                    soTienFormatted = string.Format("{0:N0} đ", giaoDich.SoTien),
                    maGiamGia = giaoDich.MaGiamGia,
                    phuongThucThanhToan = giaoDich.PhuongThucThanhToan,
                    trangThai = giaoDich.TrangThai,
                    ngayTao = giaoDich.NgayTao,
                    ngayTaoFormatted = (giaoDich.NgayTao ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        // POST: /api/GiaoDich/MuaTruyen
        public class MuaTruyenRequest
        {
            public int? MaNguoiDung { get; set; }
            public int MaTruyen { get; set; }
            public string? PhuongThucThanhToan { get; set; } = "VNPAY";
            public string? MaGiamGia { get; set; }
        }

        [HttpPost("MuaTruyen")]
        public async Task<IActionResult> MuaTruyen([FromBody] MuaTruyenRequest request)
        {
            int? currentUserId = ResolveUserId(request.MaNguoiDung);
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để thực hiện giao dịch." });
            }

            var truyen = await _context.Truyens.FindAsync(request.MaTruyen);
            if (truyen == null)
            {
                return NotFound(new { success = false, message = "Truyện không tồn tại." });
            }

            // Kiểm tra đã mua truyện này chưa
            bool daMua = await _context.TruyenDaMuas.AnyAsync(m => m.MaNguoiDung == currentUserId.Value && m.MaTruyen == request.MaTruyen);
            if (daMua)
            {
                return BadRequest(new { success = false, message = "Bạn đã mua bộ truyện này rồi." });
            }

            decimal gia = (truyen.GiaBan ?? 0) > 0 ? truyen.GiaBan!.Value : 49000;

            // Thêm vào TruyenDaMua
            var truyenDaMua = new TruyenDaMua
            {
                MaNguoiDung = currentUserId.Value,
                MaTruyen = request.MaTruyen,
                GiaDaTra = gia,
                NgayMua = DateTime.Now
            };
            _context.TruyenDaMuas.Add(truyenDaMua);

            // Thêm vào LichSuGiaoDich
            var giaoDich = new LichSuGiaoDich
            {
                MaNguoiDung = currentUserId.Value,
                LoaiGiaoDich = "MuaTruyen",
                MaMụcTieu = request.MaTruyen,
                SoTien = gia,
                MaGiamGia = request.MaGiamGia,
                PhuongThucThanhToan = string.IsNullOrEmpty(request.PhuongThucThanhToan) ? "VNPAY" : request.PhuongThucThanhToan,
                TrangThai = "Thành công",
                NgayTao = DateTime.Now
            };
            _context.LichSuGiaoDiches.Add(giaoDich);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Thanh toán mua truyện thành công!",
                maGiaoDich = giaoDich.MaGiaoDich,
                tenTruyen = truyen.TenTruyen,
                soTien = gia
            });
        }
    }
}
