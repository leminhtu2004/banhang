using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebBanHang1.Services;
using System.Collections.Generic;
using WebBanHang1.Models;
using Microsoft.Extensions.Logging;
using System; // Added for Exception

namespace WebBanHang1.Controllers
{
    [Authorize(Roles = "Admin")] // Ensure only admins can access
    [Route("[controller]")] // Set base route for this controller
    public class AdminPromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<AdminPromotionController> _logger;

        public AdminPromotionController(IPromotionService promotionService, ILogger<AdminPromotionController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        // GET: /AdminPromotion
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Attempting to load promotions for admin view.");
                var promotions = await _promotionService.GetAllPromotionsAsync();
                _logger.LogInformation($"Successfully retrieved {promotions?.Count ?? 0} promotions.");
                // Pass the list of GiamGium objects to the Partial View
                return PartialView("~/Views/Promotion/_PromotionsListPartial.cshtml", promotions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading promotions for admin view.");
                // Return an empty partial view or an error message partial view
                return Content("Error loading promotions."); // Return simple error message for now
            }
        }

        // GET: /AdminPromotion/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Return the Create view for promotions
            return View("~/Views/Promotion/Create.cshtml");
        }

        // POST: /AdminPromotion/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken] // Add anti-forgery token validation
        public async Task<IActionResult> Create([Bind("MaGiamGia,GiaTriGiam,NgayBatDau,NgayKetThuc,LoaiGiamGia")] GiamGium promotion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _promotionService.CreatePromotionAsync(promotion);
                    _logger.LogInformation($"Promotion created successfully: {promotion.MaGiamGia}");
                    // Redirect back to the index page after creation
                    return RedirectToAction("Index", "Admin");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating promotion: {promotion.MaGiamGia}");
                    ModelState.AddModelError("", "An error occurred while creating the promotion.");
                }
            }
            // If model state is not valid or an error occurred, return to the Create view with errors
            return View("~/Views/Promotion/Create.cshtml", promotion);
        }

        // GET: /AdminPromotion/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View("~/Views/Promotion/Edit.cshtml", promotion);
        }

        // POST: /AdminPromotion/Edit/{id}
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaGiamGia,GiaTriGiam,NgayBatDau,NgayKetThuc,LoaiGiamGia")] GiamGium promotion)
        {
            if (id != promotion.MaGiamGia)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _promotionService.UpdatePromotionAsync(promotion);
                    _logger.LogInformation("Promotion updated successfully: { MaGiamGia }", promotion.MaGiamGia);
                    return RedirectToAction("Index", "Admin");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating promotion: { MaGiamGia }", promotion.MaGiamGia);
                    ModelState.AddModelError("", "An error occurred while updating the promotion.");
                }
            }
            return View("~/Views/Promotion/Edit.cshtml", promotion);
        }

        // GET: /AdminPromotion/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View("~/Views/Promotion/Details.cshtml", promotion);
        }

        // GET: /AdminPromotion/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View("~/Views/Promotion/Delete.cshtml", promotion);
        }

        // POST: /AdminPromotion/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _promotionService.DeletePromotionAsync(id);
                _logger.LogInformation("Promotion deleted successfully: { Id }", id);
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting promotion: { Id }", id);
                // Optionally, add a message to indicate deletion failure
                TempData["ErrorMessage"] = "An error occurred while deleting the promotion.";
                return RedirectToAction("Index", "Admin"); // Redirect back to index even on error
            }
        }

        // POST: /AdminPromotion/DeleteConfirmedPermanently/{id}
        [HttpPost("DeleteConfirmedPermanently/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedPermanently(string id)
        {
            try
            {
                await _promotionService.DeletePromotionAsync(id);
                _logger.LogInformation("Promotion permanently deleted: { Id }", id);
                return Json(new { success = true, message = "Xóa khuyến mãi thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting promotion: { Id }", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa khuyến mãi" });
            }
        }
    }
} 