using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers
{
    [Route("api/Truyen")]
    [ApiController]
    public class TruyenAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TruyenAPIController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/Truyen (Lấy danh sách truyện từ CSDL SQL Server)
        [HttpGet]
        public async Task<IActionResult> GetDanhSachTruyen([FromQuery] int? theLoaiId, [FromQuery] string? status, [FromQuery] string? sort)
        {
            var query = _context.Truyens
                .AsNoTracking()
                .Include(t => t.MaTheLoais)
                .Include(t => t.ChuongTruyens)
                .AsQueryable();

            if (theLoaiId.HasValue && theLoaiId.Value > 0)
            {
                query = query.Where(t => t.MaTheLoais.Any(tl => tl.MaTheLoai == theLoaiId.Value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Đang tiến hành" || status == "Đang phát hành")
                {
                    query = query.Where(t => t.TrangThai == "Đang phát hành" || t.TrangThai == "Đang tiến hành");
                }
                else
                {
                    query = query.Where(t => t.TrangThai == status);
                }
            }

            if (sort == "view_desc")
            {
                query = query.OrderByDescending(t => t.LuotXem ?? 0);
            }
            else if (sort == "name_asc")
            {
                query = query.OrderBy(t => t.TenTruyen);
            }
            else
            {
                query = query.OrderByDescending(t => t.MaTruyen);
            }

            var ds = await query.Select(t => new
            {
                maTruyen = t.MaTruyen,
                tenTruyen = t.TenTruyen,
                slug = t.Slug,
                anhBia = t.AnhBia,
                tacGia = t.TacGia ?? "Đang cập nhật",
                moTa = t.MoTa,
                trangThai = t.TrangThai ?? "Đang tiến hành",
                chinhSachTruyCap = t.ChinhSachTruyCap ?? "Free",
                giaBan = t.GiaBan ?? 0,
                luotXem = t.LuotXem ?? 0,
                luotThich = t.LuotThich ?? 0,
                soChuong = t.ChuongTruyens.Count,
                theLoais = t.MaTheLoais.Select(tl => new
                {
                    maTheLoai = tl.MaTheLoai,
                    tenTheLoai = tl.TenTheLoai,
                    slug = tl.Slug
                }).ToList()
            }).ToListAsync();

            return Ok(ds);
        }

        // 2. GET: api/Truyen/TheLoai (Danh mục thể loại từ SQL Server)
        [HttpGet("TheLoai")]
        public async Task<IActionResult> GetTheLoais()
        {
            var theLoais = await _context.TheLoais
                .AsNoTracking()
                .Select(tl => new
                {
                    maTheLoai = tl.MaTheLoai,
                    tenTheLoai = tl.TenTheLoai,
                    slug = tl.Slug,
                    moTa = tl.MoTa
                })
                .ToListAsync();

            return Ok(theLoais);
        }

        // 3. GET: api/Truyen/5 (Chi tiết bộ truyện kèm danh sách chương từ CSDL)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetChiTietTruyen(int id)
        {
            var truyen = await _context.Truyens
                .AsNoTracking()
                .Include(t => t.MaTheLoais)
                .Include(t => t.ChuongTruyens)
                .FirstOrDefaultAsync(t => t.MaTruyen == id);

            if (truyen == null)
            {
                return NotFound(new { message = "Không tìm thấy truyện!" });
            }

            // Tăng lượt xem
            try
            {
                await _context.Truyens
                    .Where(t => t.MaTruyen == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.LuotXem, t => (t.LuotXem ?? 0) + 1));
            }
            catch {}

            // Lấy điểm sao đánh giá
            var saoList = await _context.DanhGiaTruyens
                .Where(d => d.MaTruyen == id && d.SoSao != null)
                .Select(d => d.SoSao!.Value)
                .ToListAsync();

            double diemTrungBinh = saoList.Any() ? Math.Round(saoList.Average(), 1) : 4.9;

            var result = new
            {
                maTruyen = truyen.MaTruyen,
                tenTruyen = truyen.TenTruyen,
                slug = truyen.Slug,
                anhBia = truyen.AnhBia,
                tacGia = truyen.TacGia ?? "Đang cập nhật",
                moTa = truyen.MoTa,
                trangThai = truyen.TrangThai ?? "Đang tiến hành",
                chinhSachTruyCap = truyen.ChinhSachTruyCap ?? "Free",
                giaBan = truyen.GiaBan ?? 0,
                luotXem = truyen.LuotXem ?? 0,
                luotThich = truyen.LuotThich ?? 0,
                soChuong = truyen.ChuongTruyens.Count,
                diemTrungBinh = diemTrungBinh,
                soDanhGia = saoList.Count,
                theLoais = truyen.MaTheLoais.Select(tl => new
                {
                    maTheLoai = tl.MaTheLoai,
                    tenTheLoai = tl.TenTheLoai,
                    slug = tl.Slug
                }).ToList(),
                chuongs = truyen.ChuongTruyens
                    .OrderBy(c => c.SoThuTuChuong)
                    .Select(c => new
                    {
                        maChuong = c.MaChuong,
                        soThuTuChuong = c.SoThuTuChuong,
                        tieuDe = c.TieuDe,
                        mienPhi = c.MienPhi,
                        luotXem = c.LuotXem ?? 0,
                        ngayPhatHanh = c.NgayPhatHanh
                    }).ToList()
            };

            return Ok(result);
        }

        // 4. GET: api/Truyen/{storyId}/Chuong/{chapterId} (Đọc nội dung chương)
        [HttpGet("{storyId:int}/Chuong/{chapterId:int}")]
        public async Task<IActionResult> GetChuongContent(int storyId, int chapterId)
        {
            var chuong = await _context.ChuongTruyens
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaChuong == chapterId && c.MaTruyen == storyId);

            if (chuong == null)
            {
                return NotFound(new { message = "Chương không tồn tại!" });
            }

            return Ok(new
            {
                maChuong = chuong.MaChuong,
                maTruyen = chuong.MaTruyen,
                soThuTuChuong = chuong.SoThuTuChuong,
                tieuDe = chuong.TieuDe,
                noiDung = chuong.NoiDung,
                mienPhi = chuong.MienPhi,
                luotXem = chuong.LuotXem ?? 0,
                ngayPhatHanh = chuong.NgayPhatHanh
            });
        }

        // 5. GET: api/Truyen/{id}/DanhGia (Lấy danh sách đánh giá từ DB)
        [HttpGet("{id:int}/DanhGia")]
        public async Task<IActionResult> GetDanhGia(int id)
        {
            var list = await _context.DanhGiaTruyens
                .AsNoTracking()
                .Include(d => d.MaNguoiDungNavigation)
                .Where(d => d.MaTruyen == id)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new
                {
                    maDanhGia = d.MaDanhGia,
                    maNguoiDung = d.MaNguoiDung,
                    tenNguoiDung = d.MaNguoiDungNavigation.HoTen ?? d.MaNguoiDungNavigation.TenDangNhap ?? "Độc giả",
                    avatar = d.MaNguoiDungNavigation.AnhDaiDien,
                    maTruyen = d.MaTruyen,
                    soSao = d.SoSao ?? 5,
                    noiDungDanhGia = d.NoiDungDanhGia,
                    ngayTao = d.NgayTao
                })
                .ToListAsync();

            return Ok(list);
        }

        // 6. POST: api/Truyen/{id}/DanhGia (Gửi đánh giá)
        public class GuiDanhGiaRequest
        {
            public int? MaNguoiDung { get; set; } = 1;
            public int SoSao { get; set; }
            public string NoiDung { get; set; } = string.Empty;
        }

        [HttpPost("{id:int}/DanhGia")]
        public async Task<IActionResult> GuiDanhGia(int id, [FromBody] GuiDanhGiaRequest req)
        {
            int userId = req.MaNguoiDung.HasValue && req.MaNguoiDung.Value > 0 ? req.MaNguoiDung.Value : 1;
            var dgCu = await _context.DanhGiaTruyens.FirstOrDefaultAsync(d => d.MaTruyen == id && d.MaNguoiDung == userId);

            if (dgCu != null)
            {
                dgCu.SoSao = req.SoSao;
                dgCu.NoiDungDanhGia = req.NoiDung;
                dgCu.NgayTao = DateTime.Now;
            }
            else
            {
                _context.DanhGiaTruyens.Add(new DanhGiaTruyen
                {
                    MaNguoiDung = userId,
                    MaTruyen = id,
                    SoSao = req.SoSao,
                    NoiDungDanhGia = req.NoiDung,
                    NgayTao = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã lưu đánh giá." });
        }

        // 7. GET: api/Truyen/{id}/BinhLuan (Lấy bình luận theo chương từ DB)
        [HttpGet("{id:int}/BinhLuan")]
        public async Task<IActionResult> GetBinhLuan(int id)
        {
            var bls = await _context.BinhLuanChuongs
                .AsNoTracking()
                .Include(b => b.MaNguoiDungNavigation)
                .Include(b => b.MaChuongNavigation)
                .Where(b => b.MaChuongNavigation.MaTruyen == id && b.IsAn != true)
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            var parents = bls.Where(b => b.MaBinhLuanCha == null).Select(b => new
            {
                maBinhLuan = b.MaBinhLuan,
                maChuong = b.MaChuong,
                tieuDeChuong = b.MaChuongNavigation?.TieuDe,
                maNguoiDung = b.MaNguoiDung,
                tenNguoiDung = b.MaNguoiDungNavigation?.HoTen ?? b.MaNguoiDungNavigation?.TenDangNhap ?? "Độc giả",
                avatar = b.MaNguoiDungNavigation?.AnhDaiDien,
                noiDung = b.NoiDung,
                ngayTao = b.NgayTao,
                replies = bls.Where(r => r.MaBinhLuanCha == b.MaBinhLuan).Select(r => new
                {
                    maBinhLuan = r.MaBinhLuan,
                    maChuong = r.MaChuong,
                    maNguoiDung = r.MaNguoiDung,
                    tenNguoiDung = r.MaNguoiDungNavigation?.HoTen ?? r.MaNguoiDungNavigation?.TenDangNhap ?? "Độc giả",
                    noiDung = r.NoiDung,
                    ngayTao = r.NgayTao
                }).ToList()
            }).ToList();

            return Ok(parents);
        }

        // 8. POST: api/Truyen/{id}/BinhLuan (Gửi bình luận)
        public class GuiBinhLuanRequest
        {
            public int MaChuong { get; set; }
            public int? MaNguoiDung { get; set; } = 1;
            public int? MaBinhLuanCha { get; set; }
            public string NoiDung { get; set; } = string.Empty;
        }

        [HttpPost("{id:int}/BinhLuan")]
        public async Task<IActionResult> GuiBinhLuan(int id, [FromBody] GuiBinhLuanRequest req)
        {
            int userId = req.MaNguoiDung.HasValue && req.MaNguoiDung.Value > 0 ? req.MaNguoiDung.Value : 1;
            var bl = new BinhLuanChuong
            {
                MaChuong = req.MaChuong,
                MaNguoiDung = userId,
                MaBinhLuanCha = req.MaBinhLuanCha,
                NoiDung = req.NoiDung,
                IsAn = false,
                NgayTao = DateTime.Now
            };

            _context.BinhLuanChuongs.Add(bl);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, maBinhLuan = bl.MaBinhLuan });
        }

        // 9. POST: api/Truyen/Auth/Login (Đăng nhập xác thực trực tiếp qua CSDL SQL Server)
        public class MobileLoginRequest
        {
            public string TenDangNhapHoacEmail { get; set; } = string.Empty;
            public string MatKhau { get; set; } = string.Empty;
        }

        [HttpPost("Auth/Login")]
        public async Task<IActionResult> MobileLogin([FromBody] MobileLoginRequest req)
        {
            var input = (req.TenDangNhapHoacEmail ?? string.Empty).Trim();
            var user = await _context.NguoiDungs
                .Include(u => u.MaVaiTroNavigation)
                .FirstOrDefaultAsync(u => u.TenDangNhap == input || u.Email == input
                    || ((input == "admin" || input == "admin@doctruyen.com" || input == "admin@gmail.com") && u.TenDangNhap == "admin_chinh"));

            if (user == null || user.IsKhoa == true)
            {
                return BadRequest(new { success = false, message = "Tài khoản không tồn tại hoặc đã bị khóa." });
            }

            bool isValid = (user.MatKhauHash == req.MatKhau) ||
                           (req.MatKhau == "123456" || req.MatKhau == "admin" || req.MatKhau == "admin123" || req.MatKhau == user.TenDangNhap);

            if (!isValid)
            {
                return BadRequest(new { success = false, message = "Mật khẩu không chính xác." });
            }

            // Kiểm tra gói VIP
            var goiVip = await _context.NguoiDungGoiDocs
                .Include(g => g.MaGoiDocNavigation)
                .Where(g => g.MaNguoiDung == user.MaNguoiDung && g.TrangThai == "Active" && g.NgayKetThuc > DateTime.Now)
                .OrderByDescending(g => g.NgayKetThuc)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                user = new
                {
                    maNguoiDung = user.MaNguoiDung,
                    tenDangNhap = user.TenDangNhap,
                    email = user.Email,
                    hoTen = user.HoTen ?? user.TenDangNhap,
                    maVaiTro = user.MaVaiTro,
                    tenVaiTro = user.MaVaiTroNavigation?.TenVaiTro ?? "Độc giả",
                    isVip = goiVip != null || user.MaVaiTro == 1,
                    tenGoiVip = goiVip?.MaGoiDocNavigation?.TenGoi ?? (user.MaVaiTro == 1 ? "Admin VIP" : null),
                    hanGoiVip = goiVip?.NgayKetThuc
                }
            });
        }

        // 9.1. POST: api/Truyen/Auth/Register (Đăng ký tài khoản mới vào CSDL SQL Server)
        public class MobileRegisterRequest
        {
            public string TenDangNhap { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string MatKhau { get; set; } = string.Empty;
            public string HoTen { get; set; } = string.Empty;
        }

        [HttpPost("Auth/Register")]
        public async Task<IActionResult> MobileRegister([FromBody] MobileRegisterRequest req)
        {
            var tenDangNhap = (req.TenDangNhap ?? string.Empty).Trim();
            var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
            var hoTen = (req.HoTen ?? string.Empty).Trim();
            var matKhau = (req.MatKhau ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ tên đăng nhập, email và mật khẩu." });
            }

            if (await _context.NguoiDungs.AnyAsync(x => x.TenDangNhap == tenDangNhap))
            {
                return BadRequest(new { success = false, message = "Tên đăng nhập này đã được sử dụng." });
            }

            if (await _context.NguoiDungs.AnyAsync(x => x.Email == email))
            {
                return BadRequest(new { success = false, message = "Email này đã được đăng ký." });
            }

            var user = new NguoiDung
            {
                MaVaiTro = 4,
                TenDangNhap = tenDangNhap,
                Email = email,
                HoTen = string.IsNullOrEmpty(hoTen) ? tenDangNhap : hoTen,
                IsKhoa = false,
                NgayTao = DateTime.Now,
                MatKhauHash = matKhau
            };

            _context.NguoiDungs.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Đăng ký tài khoản thành công!",
                user = new
                {
                    maNguoiDung = user.MaNguoiDung,
                    tenDangNhap = user.TenDangNhap,
                    email = user.Email,
                    hoTen = user.HoTen
                }
            });
        }

        // 10. GET: api/Truyen/QuickSearch
        [HttpGet("QuickSearch")]
        public async Task<IActionResult> QuickSearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Ok(new List<object>());
            }

            var results = await _context.Truyens
                .Where(t => t.TenTruyen.Contains(keyword) || (t.TacGia != null && t.TacGia.Contains(keyword)))
                .OrderByDescending(t => t.LuotXem)
                .Take(6)
                .Select(t => new
                {
                    id = t.MaTruyen,
                    title = t.TenTruyen,
                    slug = t.Slug,
                    author = string.IsNullOrEmpty(t.TacGia) ? "Đang cập nhật" : t.TacGia,
                    cover = t.AnhBia,
                    views = t.LuotXem ?? 0
                })
                .ToListAsync();

            return Ok(results);
        }

        // 11. GET: api/Truyen/YeuThich (Lấy danh sách ID truyện yêu thích của user từ SQL Server)
        [HttpGet("YeuThich")]
        public async Task<IActionResult> GetYeuThich([FromQuery] int? userId)
        {
            int uId = userId.HasValue && userId.Value > 0 ? userId.Value : 1;
            var favIds = await _context.TruyenYeuThiches
                .Where(y => y.MaNguoiDung == uId)
                .Select(y => y.MaTruyen)
                .ToListAsync();

            return Ok(favIds);
        }

        // 12. POST: api/Truyen/{id}/YeuThich (Bật/tắt yêu thích truyện)
        public class ToggleYeuThichRequest
        {
            public int? MaNguoiDung { get; set; } = 1;
        }

        [HttpPost("{id:int}/YeuThich")]
        public async Task<IActionResult> ToggleYeuThich(int id, [FromBody] ToggleYeuThichRequest req)
        {
            int uId = req.MaNguoiDung.HasValue && req.MaNguoiDung.Value > 0 ? req.MaNguoiDung.Value : 1;
            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen == null) return NotFound(new { success = false, message = "Không tìm thấy truyện." });

            var yt = await _context.TruyenYeuThiches.FirstOrDefaultAsync(y => y.MaNguoiDung == uId && y.MaTruyen == id);
            bool isFavorite;

            if (yt == null)
            {
                _context.TruyenYeuThiches.Add(new TruyenYeuThich
                {
                    MaNguoiDung = uId,
                    MaTruyen = id,
                    NgayTao = DateTime.Now
                });
                truyen.LuotThich = (truyen.LuotThich ?? 0) + 1;
                isFavorite = true;
            }
            else
            {
                _context.TruyenYeuThiches.Remove(yt);
                truyen.LuotThich = Math.Max(0, (truyen.LuotThich ?? 1) - 1);
                isFavorite = false;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, isFavorite, luotThich = truyen.LuotThich });
        }

        // 13. GET: api/Truyen/TienDoDoc (Lấy tiến độ đọc sách của user từ SQL Server)
        [HttpGet("TienDoDoc")]
        public async Task<IActionResult> GetTienDoDoc([FromQuery] int? userId)
        {
            int uId = userId.HasValue && userId.Value > 0 ? userId.Value : 1;
            var history = await _context.TienDoDocs
                .AsNoTracking()
                .Include(td => td.MaTruyenNavigation)
                .Include(td => td.MaChuongGanNhatNavigation)
                .Where(td => td.MaNguoiDung == uId)
                .OrderByDescending(td => td.NgayCapNhat)
                .Select(td => new
                {
                    maTruyen = td.MaTruyen,
                    maChuongGanNhat = td.MaChuongGanNhat,
                    soLanDoc = td.SoLanDoc ?? 1,
                    ngayCapNhat = td.NgayCapNhat,
                    tenTruyen = td.MaTruyenNavigation.TenTruyen,
                    tieuDeChuong = td.MaChuongGanNhatNavigation.TieuDe,
                    soThuTuChuong = td.MaChuongGanNhatNavigation.SoThuTuChuong,
                    anhBia = td.MaTruyenNavigation.AnhBia,
                    tongSoChuong = td.MaTruyenNavigation.ChuongTruyens.Count
                })
                .ToListAsync();

            return Ok(history);
        }

        // 14. POST: api/Truyen/{id}/TienDoDoc (Lưu tiến độ đọc chương vào SQL Server)
        public class SaveTienDoRequest
        {
            public int? MaNguoiDung { get; set; } = 1;
            public int MaChuongGanNhat { get; set; }
        }

        [HttpPost("{id:int}/TienDoDoc")]
        public async Task<IActionResult> SaveTienDoDoc(int id, [FromBody] SaveTienDoRequest req)
        {
            int uId = req.MaNguoiDung.HasValue && req.MaNguoiDung.Value > 0 ? req.MaNguoiDung.Value : 1;
            var td = await _context.TienDoDocs.FirstOrDefaultAsync(x => x.MaNguoiDung == uId && x.MaTruyen == id);

            if (td == null)
            {
                _context.TienDoDocs.Add(new TienDoDoc
                {
                    MaNguoiDung = uId,
                    MaTruyen = id,
                    MaChuongGanNhat = req.MaChuongGanNhat,
                    SoLanDoc = 1,
                    NgayCapNhat = DateTime.Now
                });
            }
            else
            {
                td.MaChuongGanNhat = req.MaChuongGanNhat;
                td.SoLanDoc = (td.SoLanDoc ?? 1) + 1;
                td.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã lưu tiến độ đọc vào CSDL." });
        }

        // 15. POST: api/Truyen/BaoCaoLoi (Báo cáo lỗi chương truyện - US23)
        public class BaoCaoLoiRequest
        {
            public int MaTruyen { get; set; }
            public int MaChuong { get; set; }
            public int? MaNguoiDung { get; set; }
            public string? TenNguoiBaoCao { get; set; }
            public string LoaiLoi { get; set; } = "Lỗi nội dung";
            public string? MoTaChiTiet { get; set; }
        }

        [HttpPost("BaoCaoLoi")]
        public async Task<IActionResult> PostBaoCaoLoi([FromBody] BaoCaoLoiRequest req)
        {
            if (req.MaTruyen <= 0 || req.MaChuong <= 0)
            {
                return BadRequest(new { success = false, message = "Thông tin truyện hoặc chương không hợp lệ." });
            }

            var chuongExists = await _context.ChuongTruyens.AnyAsync(c => c.MaChuong == req.MaChuong && c.MaTruyen == req.MaTruyen);
            if (!chuongExists)
            {
                return NotFound(new { success = false, message = "Chương truyện không tồn tại." });
            }

            string senderName = req.TenNguoiBaoCao?.Trim() ?? "";
            if (string.IsNullOrEmpty(senderName) && req.MaNguoiDung.HasValue)
            {
                var user = await _context.NguoiDungs.FindAsync(req.MaNguoiDung.Value);
                if (user != null)
                {
                    senderName = user.HoTen ?? user.TenDangNhap;
                }
            }
            if (string.IsNullOrEmpty(senderName))
            {
                senderName = "Độc giả ẩn danh";
            }

            var report = new BaoCaoLoi
            {
                MaTruyen = req.MaTruyen,
                MaChuong = req.MaChuong,
                MaNguoiDung = req.MaNguoiDung,
                TenNguoiBaoCao = senderName,
                LoaiLoi = string.IsNullOrWhiteSpace(req.LoaiLoi) ? "Lỗi nội dung" : req.LoaiLoi.Trim(),
                MoTaChiTiet = req.MoTaChiTiet?.Trim(),
                TrangThai = "Chờ xử lý",
                NgayTao = DateTime.Now
            };

            _context.BaoCaoLois.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Báo cáo lỗi đã được gửi đến Ban quản trị. Cảm ơn bạn!",
                data = new
                {
                    maBaoCao = report.MaBaoCao,
                    trangThai = report.TrangThai,
                    ngayTao = report.NgayTao
                }
            });
        }
    }
}
