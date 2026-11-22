using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;
using WebDocTruyen.ViewModels;

namespace WebDocTruyen.Controllers
{
    public class TruyenController : Controller
    {
        private readonly AppDbContext _context;

        public TruyenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Truyen/Doc/5
        // Hiển thị nội dung chương & Giới hạn khách chưa đăng nhập chỉ được đọc 3 chương đầu (US04)
        public async Task<IActionResult> Doc(int id)
        {
            var chuong = await _context.ChuongTruyens
                .AsNoTracking()
                .Include(c => c.MaTruyenNavigation)
                .FirstOrDefaultAsync(c => c.MaChuong == id);

            if (chuong == null) return NotFound();

            var truyen = chuong.MaTruyenNavigation;

            // Lấy tài khoản & UserId từ Session
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            int? userId = HttpContext.Session.GetInt32("UserId");
            bool isLoggedIn = !string.IsNullOrWhiteSpace(taiKhoan) || userId.HasValue;

            // ================= US04: XỬ LÝ KHÁCH VẮNG LAI (GUEST USER) =================
            // Bắt buộc đăng nhập hoặc đăng ký tài khoản để đọc truyện
            if (!isLoggedIn)
            {
                TempData["Message"] = "Vui lòng đăng nhập hoặc đăng ký tài khoản để đọc truyện!";
                return RedirectToAction("Login", "Account");
            }
            // =========================================================================

            // Kiểm tra phân quyền đối với truyện trả phí / chương mất phí
            if (!chuong.MienPhi && truyen.ChinhSachTruyCap != "Free")
            {
                if (!isLoggedIn)
                    return RedirectToAction("Login", "Account");

                var uId = userId ?? await _context.NguoiDungs
                    .Where(u => u.TenDangNhap == taiKhoan)
                    .Select(u => (int?)u.MaNguoiDung)
                    .SingleOrDefaultAsync();

                var daMua = uId.HasValue && await _context.TruyenDaMuas
                    .AnyAsync(m => m.MaNguoiDung == uId.Value && m.MaTruyen == truyen.MaTruyen);

                var coGoi = uId.HasValue && await _context.NguoiDungGoiDocs
                    .AnyAsync(g => g.MaNguoiDung == uId.Value
                                && g.TrangThai == "Active"
                                && g.NgayKetThuc > DateTime.Now);

                if (!daMua && !coGoi)
                    return RedirectToAction(nameof(Details), new { id = truyen.MaTruyen });
            }

            // Tự động lưu tiến độ đọc nếu người dùng đã đăng nhập
            if (isLoggedIn && userId.HasValue)
            {
                var progress = await _context.TienDoDocs
                    .SingleOrDefaultAsync(p => p.MaNguoiDung == userId.Value && p.MaTruyen == truyen.MaTruyen);

                if (progress == null)
                {
                    _context.TienDoDocs.Add(new TienDoDoc
                    {
                        MaNguoiDung = userId.Value,
                        MaTruyen = truyen.MaTruyen,
                        MaChuongGanNhat = chuong.MaChuong,
                        SoLanDoc = 1,
                        NgayCapNhat = DateTime.Now
                    });
                }
                else
                {
                    progress.MaChuongGanNhat = chuong.MaChuong;
                    progress.SoLanDoc = (progress.SoLanDoc ?? 0) + 1;
                    progress.NgayCapNhat = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }

            // Lấy ID chương trước & chương sau để phân trang
            ViewBag.ChuongTruoc = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == chuong.MaTruyen && c.SoThuTuChuong < chuong.SoThuTuChuong)
                .OrderByDescending(c => c.SoThuTuChuong)
                .Select(c => (int?)c.MaChuong)
                .FirstOrDefaultAsync();

            ViewBag.ChuongSau = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == chuong.MaTruyen && c.SoThuTuChuong > chuong.SoThuTuChuong)
                .OrderBy(c => c.SoThuTuChuong)
                .Select(c => (int?)c.MaChuong)
                .FirstOrDefaultAsync();

            // Lấy toàn bộ danh sách chương để hiển thị popup/select chuyển chương
            ViewBag.DanhSachChuong = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == chuong.MaTruyen)
                .OrderBy(c => c.SoThuTuChuong)
                .Select(c => new ChuongItemVM
                {
                    MaChuong = c.MaChuong,
                    SoThuTuChuong = c.SoThuTuChuong,
                    TieuDe = c.TieuDe,
                    MienPhi = c.MienPhi
                })
                .ToListAsync();

            // Kiểm tra trạng thái đã yêu thích/theo dõi của user
            if (userId.HasValue)
            {
                ViewBag.DaYeuThich = await _context.TruyenYeuThiches
                    .AnyAsync(y => y.MaNguoiDung == userId && y.MaTruyen == chuong.MaTruyen);
            }
            else
            {
                ViewBag.DaYeuThich = false;
            }

            // Lấy danh sách bình luận của truyện để hiển thị dưới phần đọc
            ViewBag.DanhSachBinhLuan = await _context.BinhLuanChuongs
                .Where(x => x.MaChuongNavigation.MaTruyen == chuong.MaTruyen && x.IsAn != true)
                .Include(x => x.MaNguoiDungNavigation)
                .Include(x => x.MaChuongNavigation)
                .Include(x => x.InverseMaBinhLuanChaNavigation)
                    .ThenInclude(r => r.MaNguoiDungNavigation)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.LikesDict = BinhLuanLikeStore.DemTatCaLike();
            ViewBag.UserLikedComments = userId.HasValue ? BinhLuanLikeStore.DanhSachDaLikeCuaUser(userId.Value) : new HashSet<int>();

            return View(chuong);
        }

        // Link "Xem tất cả" ở trang chủ: tạm chuyển về trang chủ
        public IActionResult Index() => RedirectToAction("Index", "Home");

        // GET: /Truyen/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var truyen = await _context.Truyens
                .AsNoTracking()
                .Include(t => t.MaTheLoais)
                .FirstOrDefaultAsync(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();

            // Tăng lượt xem bằng 1 câu UPDATE
            await _context.Truyens
                .Where(t => t.MaTruyen == id)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.LuotXem, t => (t.LuotXem ?? 0) + 1));

            // Chỉ lấy thông tin chương, KHÔNG lấy NoiDung
            var chuongs = await _context.ChuongTruyens
                .Where(c => c.MaTruyen == id)
                .OrderBy(c => c.SoThuTuChuong)
                .Select(c => new ChuongItemVM
                {
                    MaChuong = c.MaChuong,
                    SoThuTuChuong = c.SoThuTuChuong,
                    TieuDe = c.TieuDe,
                    MienPhi = c.MienPhi,
                    LuotXem = c.LuotXem,
                    NgayPhatHanh = c.NgayPhatHanh
                })
                .ToListAsync();

            var sao = await _context.DanhGiaTruyens
                .Where(d => d.MaTruyen == id && d.SoSao != null)
                .Select(d => d.SoSao!.Value)
                .ToListAsync();

            var vm = new TruyenDetailViewModel
            {
                Truyen = truyen,
                TheLoais = truyen.MaTheLoais.Select(tl => tl.TenTheLoai).ToList(),
                DanhSachTheLoai = truyen.MaTheLoais.Select(tl => new TheLoaiItemVM { MaTheLoai = tl.MaTheLoai, TenTheLoai = tl.TenTheLoai }).ToList(),
                Chuongs = chuongs,
                SoDanhGia = sao.Count,
                DiemTrungBinh = sao.Any() ? Math.Round(sao.Average(), 1) : 0,
                CoQuyenDoc = truyen.ChinhSachTruyCap == "Free"
            };

            // LẤY DANH SÁCH BÌNH LUẬN ĐỂ HIỂN THỊ RA VIEW DETAILS
            var danhSachBinhLuan = await _context.BinhLuanChuongs
                .Where(x => x.MaChuongNavigation.MaTruyen == id && x.IsAn != true)
                .Include(x => x.MaNguoiDungNavigation)
                .Include(x => x.MaChuongNavigation)
                .Include(x => x.InverseMaBinhLuanChaNavigation)
                    .ThenInclude(r => r.MaNguoiDungNavigation)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.DanhSachBinhLuan = danhSachBinhLuan;
            ViewBag.LikesDict = BinhLuanLikeStore.DemTatCaLike();

            int? userId = HttpContext.Session.GetInt32("UserId");
            vm.DaDangNhap = userId.HasValue;

            // LẤY DANH SÁCH ĐÁNH GIÁ CỦA TRUYỆN
            var danhSachDanhGia = await _context.DanhGiaTruyens
                .Where(x => x.MaTruyen == id)
                .Include(x => x.MaNguoiDungNavigation)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            DanhGiaTruyen? danhGiaCuaToi = null;
            if (userId.HasValue)
            {
                danhGiaCuaToi = danhSachDanhGia.FirstOrDefault(x => x.MaNguoiDung == userId.Value);
            }

            ViewBag.DanhSachDanhGia = danhSachDanhGia;
            ViewBag.DanhGiaCuaToi = danhGiaCuaToi;
            ViewBag.UserLikedComments = userId.HasValue ? BinhLuanLikeStore.DanhSachDaLikeCuaUser(userId.Value) : new HashSet<int>();
            if (userId.HasValue)
            {
                vm.DaTheoDoi = await _context.TruyenYeuThiches
                    .AnyAsync(y => y.MaNguoiDung == userId && y.MaTruyen == id);
                vm.DaYeuThich = vm.DaTheoDoi;
                vm.DaThich = ThichTruyenStore.DaThich(userId.Value, id);

                vm.MaChuongDangDoc = await _context.TienDoDocs
                    .Where(t => t.MaNguoiDung == userId && t.MaTruyen == id)
                    .Select(t => (int?)t.MaChuongGanNhat)
                    .FirstOrDefaultAsync();

                bool daMua = await _context.TruyenDaMuas
                    .AnyAsync(m => m.MaNguoiDung == userId && m.MaTruyen == id);
                vm.DaMuaLe = daMua;

                if (!vm.CoQuyenDoc)
                {
                    bool coGoi = await _context.NguoiDungGoiDocs
                        .AnyAsync(g => g.MaNguoiDung == userId
                                    && g.TrangThai == "Active"
                                    && g.NgayKetThuc > DateTime.Now);

                    vm.CoQuyenDoc = daMua || coGoi;
                }
            }

            return View(vm);
        }

        // POST: /Truyen/ToggleTheoDoi (AJAX) - bật/tắt theo dõi truyện
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTheoDoi(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { message = "Vui lòng đăng nhập để theo dõi truyện." });

            if (!await _context.Truyens.AnyAsync(t => t.MaTruyen == id))
                return NotFound(new { message = "Không tìm thấy truyện." });

            var yeuThich = await _context.TruyenYeuThiches
                .FirstOrDefaultAsync(y => y.MaNguoiDung == userId.Value && y.MaTruyen == id);

            bool daTheoDoi;
            if (yeuThich == null)
            {
                _context.TruyenYeuThiches.Add(new TruyenYeuThich
                {
                    MaNguoiDung = userId.Value,
                    MaTruyen = id,
                    NgayTao = DateTime.Now
                });
                daTheoDoi = true;
            }
            else
            {
                _context.TruyenYeuThiches.Remove(yeuThich);
                daTheoDoi = false;
            }
            await _context.SaveChangesAsync();

            int tongTheoDoi = await _context.TruyenYeuThiches.CountAsync(y => y.MaTruyen == id);
            return Json(new { daTheoDoi, tongTheoDoi });
        }

        // POST: /Truyen/ToggleThichTruyen (AJAX) - thích hoặc bỏ thích truyện
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleThichTruyen(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { message = "Vui lòng đăng nhập để thích truyện." });

            var truyen = await _context.Truyens.FirstOrDefaultAsync(t => t.MaTruyen == id);
            if (truyen == null)
                return NotFound(new { message = "Không tìm thấy truyện." });

            bool daThich = ThichTruyenStore.ToggleThich(userId.Value, id);
            int baseCount = truyen.LuotThich ?? 0;
            if (daThich)
            {
                baseCount += 1;
            }
            else
            {
                baseCount = Math.Max(0, baseCount - 1);
            }
            truyen.LuotThich = baseCount;
            await _context.SaveChangesAsync();

            return Json(new { daThich, luotThich = baseCount });
        }

        // POST: /Truyen/ToggleLikeBinhLuan (AJAX) - thích hoặc bỏ thích bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLikeBinhLuan(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized(new { message = "Vui lòng đăng nhập để thích bình luận." });

            var binhLuan = await _context.BinhLuanChuongs.FirstOrDefaultAsync(b => b.MaBinhLuan == id);
            if (binhLuan == null)
                return NotFound(new { message = "Không tìm thấy bình luận." });

            var (daLike, tongLike) = BinhLuanLikeStore.ToggleLike(userId.Value, id);
            return Json(new { daLike, tongLike });
        }

        // POST: /Truyen/ToggleYeuThich (AJAX) - tương thích ngược
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleYeuThich(int id)
        {
            return await ToggleTheoDoi(id);
        }

        // GET: /Truyen/YeuThich - danh sách truyện đang theo dõi
        [HttpGet]
        public async Task<IActionResult> YeuThich()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var ds = await _context.TruyenYeuThiches
                .Where(y => y.MaNguoiDung == userId.Value)
                .OrderByDescending(y => y.NgayTao)
                .Select(y => new YeuThichItemVM
                {
                    MaTruyen = y.MaTruyen,
                    TenTruyen = y.MaTruyenNavigation.TenTruyen,
                    AnhBia = y.MaTruyenNavigation.AnhBia,
                    TacGia = y.MaTruyenNavigation.TacGia,
                    TrangThai = y.MaTruyenNavigation.TrangThai,
                    LuotXem = y.MaTruyenNavigation.LuotXem ?? 0,
                    SoChuong = y.MaTruyenNavigation.ChuongTruyens.Count,
                    NgayTheoDoi = y.NgayTao
                })
                .ToListAsync();

            return View(ds);
        }

        // GET: /Truyen/LichSuDoc - danh sách truyện & chương đã đọc gần đây
        [HttpGet]
        public async Task<IActionResult> LichSuDoc()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (!string.IsNullOrEmpty(taiKhoan))
                {
                    userId = await _context.NguoiDungs
                        .Where(u => u.TenDangNhap == taiKhoan)
                        .Select(u => (int?)u.MaNguoiDung)
                        .FirstOrDefaultAsync();
                }
            }

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var lichSu = await _context.TienDoDocs
                .AsNoTracking()
                .Include(p => p.MaTruyenNavigation)
                .Include(p => p.MaChuongGanNhatNavigation)
                .Where(p => p.MaNguoiDung == userId.Value)
                .OrderByDescending(p => p.NgayCapNhat)
                .ToListAsync();

            return View(lichSu);
        }

        // POST: /Truyen/MuaTruyen (MVC Form hoặc AJAX cho US08 Mua lẻ từng bộ truyện)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MuaTruyen(int id, string? phuongThuc = "VNPAY")
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
                if (!string.IsNullOrEmpty(taiKhoan))
                {
                    var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
                    if (user != null)
                    {
                        userId = user.MaNguoiDung;
                        HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
                    }
                }
            }

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Truyen", new { id }) });
            }

            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen == null)
            {
                TempData["ErrorMessage"] = "Truyện không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            bool daMua = await _context.TruyenDaMuas.AnyAsync(m => m.MaNguoiDung == userId.Value && m.MaTruyen == id);
            if (daMua)
            {
                TempData["InfoMessage"] = "Bạn đã mua bộ truyện này rồi!";
                return RedirectToAction("Details", "Truyen", new { id });
            }

            decimal gia = (truyen.GiaBan ?? 0) > 0 ? truyen.GiaBan!.Value : 49000;

            var truyenDaMua = new TruyenDaMua
            {
                MaNguoiDung = userId.Value,
                MaTruyen = id,
                GiaDaTra = gia,
                NgayMua = DateTime.Now
            };
            _context.TruyenDaMuas.Add(truyenDaMua);

            string phuongThucChuan = (phuongThuc?.Trim().ToLower()) switch
            {
                "paypal" => "PayPal",
                _ => "VNPAY"
            };

            var giaoDich = new LichSuGiaoDich
            {
                MaNguoiDung = userId.Value,
                LoaiGiaoDich = "MuaTruyen",
                MaMụcTieu = id,
                SoTien = gia,
                PhuongThucThanhToan = phuongThucChuan,
                TrangThai = "Thành công",
                NgayTao = DateTime.Now
            };
            _context.LichSuGiaoDiches.Add(giaoDich);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thanh toán thành công! Bạn đã mua trọn bộ truyện '{truyen.TenTruyen}'.";
            return RedirectToAction("Details", "Truyen", new { id });
        }

        // ==========================================
        // POST: GỬI ĐÁNH GIÁ TRUYỆN (1 - 5 SAO)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiDanhGia(
            int maTruyen,
            int soSao,
            string noiDungDanhGia)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đánh giá truyện.";
                return RedirectToAction("Login", "Account");
            }

            if (soSao < 1 || soSao > 5)
            {
                TempData["ErrorMessage"] = "Số sao phải từ 1 đến 5.";
                return RedirectToAction("Details", new { id = maTruyen });
            }

            noiDungDanhGia = (noiDungDanhGia ?? "").Trim();

            if (string.IsNullOrWhiteSpace(noiDungDanhGia))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung đánh giá.";
                return RedirectToAction("Details", new { id = maTruyen });
            }

            if (noiDungDanhGia.Length > 2000)
            {
                TempData["ErrorMessage"] = "Nội dung đánh giá không được vượt quá 2000 ký tự.";
                return RedirectToAction("Details", new { id = maTruyen });
            }

            bool truyenTonTai = await _context.Truyens.AnyAsync(x => x.MaTruyen == maTruyen);
            if (!truyenTonTai) return NotFound();

            var danhGiaCu = await _context.DanhGiaTruyens
                .FirstOrDefaultAsync(x => x.MaNguoiDung == userId.Value && x.MaTruyen == maTruyen);

            if (danhGiaCu != null)
            {
                danhGiaCu.SoSao = soSao;
                danhGiaCu.NoiDungDanhGia = noiDungDanhGia;
                danhGiaCu.NgayTao = DateTime.Now;
                TempData["SuccessMessage"] = "Cập nhật đánh giá thành công!";
            }
            else
            {
                var danhGiaMoi = new DanhGiaTruyen
                {
                    MaNguoiDung = userId.Value,
                    MaTruyen = maTruyen,
                    SoSao = soSao,
                    NoiDungDanhGia = noiDungDanhGia,
                    NgayTao = DateTime.Now
                };
                _context.DanhGiaTruyens.Add(danhGiaMoi);
                TempData["SuccessMessage"] = "Gửi đánh giá thành công!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = maTruyen });
        }

        // ==========================================
        // POST: THÊM BÌNH LUẬN HOẶC TRẢ LỜI
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuan(
            int maChuong,
            int? maBinhLuanCha,
            string noiDung,
            int? maTruyen)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để bình luận.";
                return RedirectToAction("Login", "Account");
            }

            noiDung = (noiDung ?? "").Trim();

            if (string.IsNullOrWhiteSpace(noiDung))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return maTruyen.HasValue ? RedirectToAction("Details", new { id = maTruyen.Value }) : RedirectToAction("Doc", new { id = maChuong });
            }

            if (noiDung.Length > 2000)
            {
                TempData["ErrorMessage"] = "Bình luận không được vượt quá 2000 ký tự.";
                return maTruyen.HasValue ? RedirectToAction("Details", new { id = maTruyen.Value }) : RedirectToAction("Doc", new { id = maChuong });
            }

            bool chuongTonTai = await _context.ChuongTruyens.AnyAsync(x => x.MaChuong == maChuong);
            if (!chuongTonTai)
            {
                // Nếu chưa có maChuong hoặc maChuong không hợp lệ, lấy chương đầu tiên của truyện
                if (maTruyen.HasValue)
                {
                    var chuongDau = await _context.ChuongTruyens.Where(c => c.MaTruyen == maTruyen.Value).OrderBy(c => c.SoThuTuChuong).FirstOrDefaultAsync();
                    if (chuongDau != null) maChuong = chuongDau.MaChuong;
                    else return NotFound();
                }
                else return NotFound();
            }

            if (maBinhLuanCha.HasValue)
            {
                var binhLuanCha = await _context.BinhLuanChuongs
                    .FirstOrDefaultAsync(x => x.MaBinhLuan == maBinhLuanCha.Value && x.IsAn != true);

                if (binhLuanCha == null)
                {
                    TempData["ErrorMessage"] = "Bình luận cần trả lời không tồn tại.";
                    return maTruyen.HasValue ? RedirectToAction("Details", new { id = maTruyen.Value }) : RedirectToAction("Doc", new { id = maChuong });
                }
                maChuong = binhLuanCha.MaChuong;
            }

            var binhLuan = new BinhLuanChuong
            {
                MaChuong = maChuong,
                MaNguoiDung = userId.Value,
                MaBinhLuanCha = maBinhLuanCha,
                NoiDung = noiDung,
                IsAn = false,
                NgayTao = DateTime.Now
            };

            _context.BinhLuanChuongs.Add(binhLuan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = maBinhLuanCha.HasValue ? "Trả lời bình luận thành công!" : "Đăng bình luận thành công!";

            if (maTruyen.HasValue && maTruyen.Value > 0)
            {
                return RedirectToAction("Details", new { id = maTruyen.Value });
            }
            return RedirectToAction("Doc", new { id = maChuong });
        }

        // ==========================================
        // POST: ADMIN ẨN BÌNH LUẬN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnBinhLuan(int id, int? maTruyen)
        {
            string vaiTro = HttpContext.Session.GetString("VaiTro") ?? "";
            bool laAdmin = vaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                           vaiTro.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase) ||
                           vaiTro.Equals("Quan tri vien", StringComparison.OrdinalIgnoreCase);

            if (!laAdmin) return Forbid();

            var binhLuan = await _context.BinhLuanChuongs.FirstOrDefaultAsync(x => x.MaBinhLuan == id);
            if (binhLuan == null) return NotFound();

            int maChuong = binhLuan.MaChuong;
            binhLuan.IsAn = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã ẩn bình luận thành công.";
            if (maTruyen.HasValue && maTruyen.Value > 0)
            {
                return RedirectToAction("Details", new { id = maTruyen.Value });
            }
            return RedirectToAction("Doc", new { id = maChuong });
        }

        // ==========================================
        // POST: BÁO CÁO LỖI CHƯƠNG (US23)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BaoCaoLoi(int maTruyen, int maChuong, string? loaiLoi, string? moTaChiTiet)
        {
            var loaiLoiHopLe = new[]
            {
                "Lỗi nội dung / Thiếu chữ", "Ảnh bị lỗi / Không tải được", "Sai thứ tự chương",
                "Chương trống / Không có nội dung", "Lỗi chính tả / Dịch thuật", "Khác"
            };
            loaiLoi = loaiLoi?.Trim();
            moTaChiTiet = moTaChiTiet?.Trim();
            if (maTruyen <= 0 || maChuong <= 0 || string.IsNullOrEmpty(loaiLoi) || !loaiLoiHopLe.Contains(loaiLoi))
            {
                TempData["ErrorMessage"] = "Thông tin báo cáo không hợp lệ. Vui lòng chọn loại lỗi.";
                return RedirectToAction("Doc", new { id = maChuong > 0 ? maChuong : (int?)null });
            }
            if (moTaChiTiet?.Length > 2000)
            {
                TempData["ErrorMessage"] = "Mô tả chi tiết không được vượt quá 2000 ký tự.";
                return RedirectToAction("Doc", new { id = maChuong });
            }
            bool chuongHopLe = await _context.ChuongTruyens.AnyAsync(c => c.MaChuong == maChuong && c.MaTruyen == maTruyen);
            if (!chuongHopLe)
            {
                TempData["ErrorMessage"] = "Chương truyện không tồn tại hoặc không thuộc truyện này.";
                return RedirectToAction("Index", "Home");
            }

            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            int? userId = HttpContext.Session.GetInt32("UserId");

            string tenNguoiBaoCao = "Độc giả ẩn danh";
            if (!string.IsNullOrEmpty(taiKhoan))
            {
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == taiKhoan);
                if (user != null)
                {
                    userId = user.MaNguoiDung;
                    tenNguoiBaoCao = user.HoTen ?? user.TenDangNhap;
                }
            }

            if (userId.HasValue && !await _context.NguoiDungs.AnyAsync(u => u.MaNguoiDung == userId.Value))
                userId = null;

            var report = new BaoCaoLoi
            {
                MaTruyen = maTruyen,
                MaChuong = maChuong,
                MaNguoiDung = userId,
                TenNguoiBaoCao = tenNguoiBaoCao,
                LoaiLoi = loaiLoi,
                MoTaChiTiet = moTaChiTiet,
                TrangThai = "Chờ xử lý",
                NgayTao = DateTime.Now
            };

            _context.BaoCaoLois.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Báo cáo lỗi đã được gửi đến Ban quản trị. Cảm ơn bạn đã đóng góp!";
            return RedirectToAction("Doc", new { id = maChuong });
        }
    }
}
