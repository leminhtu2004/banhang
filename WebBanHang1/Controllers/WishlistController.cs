using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang1.Data;
using WebBanHang1.Models;
using System.Security.Claims;

namespace WebBanHang1.Controllers
{
    public class WishlistController : Controller
    {
        private readonly QuanLiHangContext _context;

        public WishlistController(QuanLiHangContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách yêu thích
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await _context.Wishlists
     .Include(w => w.MaHhNavigation) // Corrected navigation property
     .ThenInclude(h => h.MaLoaiNavigation)
     .Where(w => w.MaKh == userId)
     .OrderByDescending(w => w.NgayThem)
     .ToListAsync();


            return View(wishlist);
        }

        // Thêm sản phẩm vào wishlist
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào danh sách yêu thích" });
            }

            // Kiểm tra sản phẩm đã tồn tại trong wishlist chưa
            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.MaKh == userId && w.MaHh == productId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích" });
            }

            var wishlistItem = new Wishlist
            {
                MaKh = userId,
                MaHh = productId,
                NgayThem = DateTime.Now
            };

            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích" });
        }

        // Xóa sản phẩm khỏi wishlist
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int wishlistId)
        {
            var userId = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.MaWishlist == wishlistId && w.MaKh == userId);

            if (wishlistItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong danh sách yêu thích" });
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích" });
        }

        // Lấy sản phẩm tương tự
        public async Task<IActionResult> GetSimilarProducts(int productId, int count = 4)
        {
            var product = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .FirstOrDefaultAsync(h => h.MaHh == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            var similarProducts = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Where(h => h.MaLoai == product.MaLoai && h.MaHh != productId)
                .OrderByDescending(h => h.SoLanXem)
                .Take(count)
                .Select(h => new
                {
                    h.MaHh,
                    h.TenHh,
                    h.DonGia,
                    h.Hinh,
                    h.GiamGia,
                    CategoryName = h.MaLoaiNavigation.TenLoai
                })
                .ToListAsync();

            return Json(new { success = true, data = similarProducts });
        }
    }
} 