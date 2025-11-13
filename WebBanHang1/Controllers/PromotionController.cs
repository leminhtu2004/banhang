using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebBanHang1.Models;
using WebBanHang1.Services;

namespace WebBanHang1.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiamGium>>> GetAllPromotions()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return Ok(promotions);
        }

        [HttpGet("{maGiamGia}")]
        public async Task<ActionResult<GiamGium>> GetPromotion(string maGiamGia)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(maGiamGia);
            if (promotion == null)
                return NotFound();

            return Ok(promotion);
        }

        [HttpPost]
        public async Task<ActionResult<GiamGium>> CreatePromotion(GiamGium promotion)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdPromotion = await _promotionService.CreatePromotionAsync(promotion);
            return CreatedAtAction(nameof(GetPromotion), new { maGiamGia = createdPromotion.MaGiamGia }, createdPromotion);
        }

        [HttpPut("{maGiamGia}")]
        public async Task<IActionResult> UpdatePromotion(string maGiamGia, GiamGium promotion)
        {
            if (maGiamGia != promotion.MaGiamGia)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedPromotion = await _promotionService.UpdatePromotionAsync(promotion);
            return Ok(updatedPromotion);
        }

        [HttpDelete("{maGiamGia}")]
        public async Task<IActionResult> DeletePromotion(string maGiamGia)
        {
            await _promotionService.DeletePromotionAsync(maGiamGia);
            return NoContent();
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ValidatePromotion([FromBody] ValidatePromotionRequest request)
        {
            var isValid = await _promotionService.ValidatePromotionAsync(request.MaGiamGia, request.TotalAmount);
            return Ok(isValid);
        }

        [HttpPost("calculate")]
        [AllowAnonymous]
        public async Task<ActionResult<decimal>> CalculateDiscount([FromBody] CalculateDiscountRequest request)
        {
            var discount = await _promotionService.CalculateDiscountAsync(request.MaGiamGia, request.TotalAmount);
            return Ok(discount);
        }
    }

    public class ValidatePromotionRequest
    {
        public string MaGiamGia { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CalculateDiscountRequest
    {
        public string MaGiamGia { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 