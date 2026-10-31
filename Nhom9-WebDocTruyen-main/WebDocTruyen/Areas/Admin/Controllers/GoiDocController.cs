using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDocTruyen.Models;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GoiDocController : Controller
    {
        private readonly AppDbContext _context;

        public GoiDocController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách gói đọc
        public async Task<IActionResult> Index()
        {
            var listGoiDoc = await _context.GoiDocs.ToListAsync();
            return View(listGoiDoc);
        }

        // 2. Giao diện Tạo gói mới
        public IActionResult Create()
        {
            return View();
        }

        // 3. Xử lý Thêm mới gói đọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GoiDoc goiDoc)
        {
            if (ModelState.IsValid)
            {
                // Mặc định gói mới tạo sẽ được hoạt động
                goiDoc.IsHoatDong = true;
                _context.GoiDocs.Add(goiDoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goiDoc);
        }

        // 4. Giao diện Chỉnh sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var goiDoc = await _context.GoiDocs.FindAsync(id);
            if (goiDoc == null) return NotFound();

            return View(goiDoc);
        }

        // 5. Xử lý Lưu chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GoiDoc goiDoc)
        {
            // So sánh id trên URL với MaGoiDoc
            if (id != goiDoc.MaGoiDoc) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(goiDoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(goiDoc);
        }
    }
}