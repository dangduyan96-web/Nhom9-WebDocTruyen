using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Controllers;

public sealed class DocController : Controller
{
    private readonly AppDbContext _context;

    public DocController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> TiepTuc(int maTruyen)
    {
        var truyenExists = await _context.Truyens.AnyAsync(t => t.MaTruyen == maTruyen);
        if (!truyenExists)
            return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var savedChapterId = userId.HasValue
            ? await _context.TienDoDocs
                .Where(p => p.MaNguoiDung == userId.Value && p.MaTruyen == maTruyen)
                .Select(p => (int?)p.MaChuongGanNhat)
                .FirstOrDefaultAsync()
            : null;

        var chapterId = savedChapterId ?? await _context.ChuongTruyens
            .Where(c => c.MaTruyen == maTruyen)
            .OrderBy(c => c.SoThuTuChuong)
            .ThenBy(c => c.MaChuong)
            .Select(c => (int?)c.MaChuong)
            .FirstOrDefaultAsync();

        if (!chapterId.HasValue)
            return RedirectToAction("Details", "Home", new { id = maTruyen });

        return RedirectToAction(nameof(Chuong), new { maTruyen, maChuong = chapterId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Chuong(int maTruyen, int? maChuong)
    {
        var truyen = await _context.Truyens
            .Include(t => t.ChuongTruyens)
            .FirstOrDefaultAsync(t => t.MaTruyen == maTruyen);
        if (truyen == null)
            return NotFound();

        var chapters = truyen.ChuongTruyens
            .OrderBy(c => c.SoThuTuChuong)
            .ThenBy(c => c.MaChuong)
            .ToList();

        var userId = HttpContext.Session.GetInt32("UserId");
        var selectedId = maChuong;
        if (!selectedId.HasValue && userId.HasValue)
        {
            selectedId = await _context.TienDoDocs
                .Where(p => p.MaNguoiDung == userId.Value && p.MaTruyen == maTruyen)
                .Select(p => (int?)p.MaChuongGanNhat)
                .FirstOrDefaultAsync();
        }

        var chapter = chapters.FirstOrDefault(c => c.MaChuong == selectedId)
            ?? chapters.FirstOrDefault();
        if (chapter == null)
        {
            TempData["LoiDoc"] = "Truyện này chưa có chương để đọc.";
            return RedirectToAction("Details", "Home", new { id = maTruyen });
        }

        if (!chapter.MienPhi)
        {
            var hasAccess = false;
            if (userId.HasValue)
            {
                var now = DateTime.Now;
                hasAccess = await _context.TruyenDaMuas
                    .AnyAsync(p => p.MaNguoiDung == userId.Value && p.MaTruyen == maTruyen)
                    || await _context.NguoiDungGoiDocs.AnyAsync(p =>
                        p.MaNguoiDung == userId.Value
                        && p.TrangThai == "Active"
                        && p.NgayBatDau <= now
                        && p.NgayKetThuc >= now);
            }

            if (!hasAccess)
            {
                if (!userId.HasValue)
                {
                    var returnUrl = Url.Action(nameof(Chuong), "Doc", new { maTruyen, maChuong = chapter.MaChuong });
                    return RedirectToAction("Login", "Account", new { returnUrl });
                }

                TempData["LoiDoc"] = "Chương này yêu cầu mua truyện hoặc có gói đọc còn hiệu lực.";
                return RedirectToAction("Details", "Home", new { id = maTruyen });
            }
        }

        var chapterIndex = chapters.FindIndex(c => c.MaChuong == chapter.MaChuong);
        return View(new DocChuongViewModel
        {
            Truyen = truyen,
            Chuong = chapter,
            MaChuongTruoc = chapterIndex > 0 ? chapters[chapterIndex - 1].MaChuong : null,
            MaChuongTiepTheo = chapterIndex < chapters.Count - 1 ? chapters[chapterIndex + 1].MaChuong : null,
            DaDangNhap = userId.HasValue
        });
    }
}
