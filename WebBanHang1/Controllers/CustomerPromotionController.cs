using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using WebBanHang1.Services;
using System.Security.Claims;
using System.Collections.Generic;
using WebBanHang1.Models;

namespace WebBanHang1.Controllers
{
    [Authorize] // Yêu cầu khách hàng đăng nhập
    public class CustomerPromotionController : Controller
    {
        private readonly IPromotionService _promotionService;

        public CustomerPromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // GET: /CustomerPromotion/Index
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy MaKH từ Claims
            if (userId == null)
            {
                // Redirect đến trang đăng nhập nếu MaKH không tồn tại (mặc dù đã có [Authorize])
                return RedirectToAction("Login", "Account"); // Giả định có Account controller và Login action
            }

            var activePromotions = await _promotionService.GetActivePromotionsForCustomerAsync(userId);
            var promotionViewModels = new List<CustomerPromotionViewModel>();

            foreach (var promo in activePromotions)
            {
                var customerUsage = await _promotionService.GetCustomerPromotionUsageAsync(promo.MaGiamGia, userId);
                promotionViewModels.Add(new CustomerPromotionViewModel
                {
                    Promotion = promo,
                    CustomerUsage = customerUsage
                });
            }

            return View(promotionViewModels);
        }

        // Có thể thêm các action khác nếu cần, ví dụ: GetPromotionDetails, ApplyPromotion (trong quá trình đặt hàng)
    }
} 