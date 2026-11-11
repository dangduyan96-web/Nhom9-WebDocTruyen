using Microsoft.AspNetCore.Mvc;

namespace WebDocTruyen.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            // Nếu chưa đăng nhập hoặc không phải admin thì đá về trang đăng nhập chung
            if (string.IsNullOrEmpty(taiKhoan))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            return View();
        }
    }
}