using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang1.Services;
using WebBanHang1.Models;
using WebBanHang1.Data;

namespace WebBanHang1.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly QuanLiHangContext _context;

        public NotificationController(INotificationService notificationService, QuanLiHangContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKh))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tự động đánh dấu tất cả thông báo đã đọc khi vào trang
            await _notificationService.MarkAllNotificationsAsReadAsync(maKh);

            var notifications = await _notificationService.GetUserNotificationsAsync(maKh, page, 10);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)await _notificationService.GetUnreadNotificationCountAsync(maKh) / 10);

            return View(notifications);
        }

        public async Task<IActionResult> Admin()
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKh))
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem user có phải admin không
            var user = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaKh == maKh);
            if (user == null || user.VaiTro != 2) // 2 = Admin
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKh))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            await _notificationService.MarkNotificationAsReadAsync(notificationId, maKh);
            
            // Trả về số thông báo chưa đọc mới
            var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(maKh);
            return Json(new { success = true, unreadCount = unreadCount });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKh))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            await _notificationService.MarkAllNotificationsAsReadAsync(maKh);
            
            // Trả về số thông báo chưa đọc mới (sẽ là 0)
            return Json(new { success = true, unreadCount = 0 });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int notificationId)
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKh))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            await _notificationService.DeleteNotificationAsync(notificationId, maKh);
            
            // Trả về số thông báo chưa đọc mới
            var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(maKh);
            return Json(new { success = true, unreadCount = unreadCount });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKh))
            {
                return Json(new { count = 0 });
            }

            var count = await _notificationService.GetUnreadNotificationCountAsync(maKh);
            return Json(new { count });
        }
        
        [HttpGet]
        public IActionResult GetCurrentUser()
        {
            var maKh = HttpContext.Session.GetString("MaKH");
            var hoTen = HttpContext.Session.GetString("HoTen");
            
            return Json(new { 
                maKh = maKh, 
                hoTen = hoTen,
                isLoggedIn = !string.IsNullOrEmpty(maKh)
            });
        }




    }
} 