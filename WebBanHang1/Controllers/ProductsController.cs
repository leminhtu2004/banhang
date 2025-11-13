using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;
using WebBanHang1.Data;
using WebBanHang1.Models;
using WebBanHang1.Services;

namespace WebBanHang1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly QuanLiHangContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        // Constructor sử dụng Dependency Injection (DI)
        public ProductsController(QuanLiHangContext context, IWebHostEnvironment webHostEnvironment, INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        public IActionResult Index(string searchString, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10)
        {
            try
            {
                var products = _context.HangHoas
                    .AsNoTracking() // Tối ưu hiệu suất khi chỉ đọc dữ liệu
                    .Select(p => p); // Khởi tạo truy vấn

                // Tìm kiếm theo tên sản phẩm - Sử dụng ToLower() để tìm kiếm không phân biệt hoa thường
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var searchTerm = searchString.Trim().ToLower();
                    products = products.Where(p => p.TenHh.ToLower().Contains(searchTerm));
                }

                // Lọc theo khoảng giá
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    // Hoán đổi giá trị nếu minPrice > maxPrice
                    var temp = minPrice;
                    minPrice = maxPrice;
                    maxPrice = temp;
                }

                products = products.Where(p =>
                    (!minPrice.HasValue || p.DonGia >= minPrice.Value) &&
                    (!maxPrice.HasValue || p.DonGia <= maxPrice.Value));

                var totalProducts = products.Count();
                var result = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Thêm thông tin tìm kiếm và phân trang vào ViewBag để hiển thị trên view
                ViewBag.SearchString = searchString;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.ResultCount = totalProducts;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
                ViewBag.CurrentPage = page;

                if (!result.Any())
                {
                    ViewBag.Message = "Không tìm thấy sản phẩm nào phù hợp với tiêu chí tìm kiếm.";
                }

                return View(result);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                ViewBag.Error = "Đã xảy ra lỗi trong quá trình tìm kiếm sản phẩm.";
                return View(new List<HangHoa>());
            }
        }

        [HttpGet]
        public IActionResult GetProductsByCategory(int maLoai)
        {
            var products = _context.HangHoas
                .Where(h => h.MaLoai == maLoai)
                .ToList();

            return PartialView("_ProductList", products);
        }

        // GET: Products/Details/5
        public IActionResult Details(int id)
        {
            var product = _context.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .Include(p => p.MaNccNavigation)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.User) // Load thông tin người dùng
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.InverseParentReview) // Load replies
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.ReviewImages)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.ReviewReactions)
                .FirstOrDefault(p => p.MaHh == id);

            if (product == null)
            {
                return NotFound();
            }

            // Tính trung bình rating và số lượng
            var ratingValues = product.ProductReviews
                .Where(r => r.ParentReviewId == null && r.Rating.HasValue)
                .Select(r => r.Rating!.Value)
                .ToList();
            ViewBag.RatingCount = ratingValues.Count;
            ViewBag.AverageRating = ratingValues.Count > 0 ? Math.Round(ratingValues.Average(), 1) : 0;

            return View(product);
        }

        // GET: Admin/Products
        public async Task<IActionResult> Products()
        {
            var products = await _context.HangHoas
                .Include(h => h.MaNccNavigation)
                .Include(h => h.MaLoaiNavigation) // Include MaLoaiNavigation
                .ToListAsync();
            return View(products);
        }

        // GET: Admin/Create
        public async Task<IActionResult> Create()
        {
            var nhaCungCaps = await _context.NhaCungCaps.ToListAsync();
            ViewBag.MaNcc = new SelectList(nhaCungCaps, "MaNcc", "TenCongTy");

            // Lấy danh sách loại sản phẩm
            var loaiSanPhams = await _context.Loais.ToListAsync();
            ViewBag.MaLoai = new SelectList(loaiSanPhams, "MaLoai", "TenLoai");

            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HangHoa hangHoa, IFormFile? imageFile)
        {
            ModelState.Remove("MaNccNavigation");
            ModelState.Remove("MaLoaiNavigation");

            if (string.IsNullOrEmpty(hangHoa.MaNcc))
            {
                ModelState.AddModelError("MaNcc", "Vui lòng chọn nhà cung cấp");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Tạo tên file độc nhất
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        hangHoa.Hinh = uniqueFileName; // Lưu tên file vào model
                    }

                    _context.Add(hangHoa);
                    await _context.SaveChangesAsync();

                    // Gửi thông báo real-time cho tất cả khách hàng
                    await _notificationService.BroadcastProductAddedNotificationAsync(hangHoa);

                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    // Chuyển hướng đến Admin/Index
                    return RedirectToAction("Index", "Admin");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi không xác định: {ex.Message}");
                }
            }

            // Nếu ModelState không hợp lệ, lấy lại danh sách nhà cung cấp và loại sản phẩm
            ViewBag.MaNcc = new SelectList(await _context.NhaCungCaps.ToListAsync(), "MaNcc", "TenCongTy", hangHoa.MaNcc);
            ViewBag.MaLoai = new SelectList(await _context.Loais.ToListAsync(), "MaLoai", "TenLoai", hangHoa.MaLoai);
            return View(hangHoa);
        }

        // GET: Admin/Edit/5
        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            ViewBag.MaNcc = new SelectList(await _context.NhaCungCaps.ToListAsync(), "MaNcc", "TenCongTy", hangHoa.MaNcc);
            ViewBag.MaLoai = new SelectList(await _context.Loais.ToListAsync(), "MaLoai", "TenLoai", hangHoa.MaLoai);
            return View(hangHoa);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HangHoa hangHoa, IFormFile? imageFile)
        {
            if (id != hangHoa.MaHh)
            {
                return NotFound();
            }

            ModelState.Remove("MaNccNavigation");
            ModelState.Remove("MaLoaiNavigation");

            if (string.IsNullOrEmpty(hangHoa.MaNcc))
            {
                ModelState.AddModelError("MaNcc", "Vui lòng chọn nhà cung cấp");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Xóa hình cũ nếu có
                        if (!string.IsNullOrEmpty(hangHoa.Hinh))
                        {
                            string oldImagePath = Path.Combine(uploadsFolder, hangHoa.Hinh);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Upload hình mới
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        hangHoa.Hinh = uniqueFileName; // Cập nhật tên file vào model
                    }

                    _context.Update(hangHoa);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    // Chuyển hướng đến Admin/Index
                    return RedirectToAction("Index", "Admin");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi không xác định: {ex.Message}");
                }
            }

            ViewBag.MaNcc = new SelectList(await _context.NhaCungCaps.ToListAsync(), "MaNcc", "TenCongTy", hangHoa.MaNcc);
            ViewBag.MaLoai = new SelectList(await _context.Loais.ToListAsync(), "MaLoai", "TenLoai", hangHoa.MaLoai);
            return View(hangHoa);
        }



        public async Task<IActionResult> Delete(int id)
        {
            var hangHoa = await _context.HangHoas
                .Include(h => h.MaNccNavigation)
                .Include(h => h.MaLoaiNavigation) // Include MaLoaiNavigation
                .FirstOrDefaultAsync(h => h.MaHh == id);
            if (hangHoa == null)
            {
                return NotFound();
            }
            return View(hangHoa);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var hangHoa = await _context.HangHoas.FindAsync(id);
                if (hangHoa == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem hàng hóa có liên quan đến hóa đơn hoặc giỏ hàng không
                var hasOrders = await _context.ChiTietHds.AnyAsync(c => c.MaHh == id);
                var hasCart = await _context.GioHangs.AnyAsync(g => g.MaHh == id);

                if (hasOrders || hasCart)
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì đã có trong đơn hàng hoặc giỏ hàng!";
                    return RedirectToAction("Index", "Admin");
                }

                // Xóa sản phẩm mà không xóa hình ảnh
                _context.HangHoas.Remove(hangHoa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                // Chuyển hướng đến Admin/Index
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa sản phẩm: {ex.Message}";
                return RedirectToAction("Index", "Admin");
            }
        }
        [Authorize]
        [HttpPost]
        public IActionResult AddReview(int productId, int rating, string comment, List<IFormFile>? images)
        {
            var maKH = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đánh giá sản phẩm";
                return RedirectToAction("Details", new { id = productId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Details", new { id = productId });
            }

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = maKH,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.ProductReviews.Add(review);
                _context.SaveChanges();

                // Upload ảnh đánh giá (nếu có)
                if (images != null && images.Count > 0)
                {
                    string reviewsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "review-images");
                    if (!Directory.Exists(reviewsFolder))
                    {
                        Directory.CreateDirectory(reviewsFolder);
                    }

                    foreach (var file in images)
                    {
                        if (file == null || file.Length == 0) continue;
                        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(reviewsFolder, uniqueName);
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fs);
                        }

                        _context.ReviewImages.Add(new ReviewImage
                        {
                            ReviewId = review.Id,
                            ImagePath = uniqueName,
                            ImageName = file.FileName,
                            UploadedAt = DateTime.Now,
                            IsMain = false
                        });
                    }

                    _context.SaveChanges();
                }
                TempData["SuccessMessage"] = "Đã thêm đánh giá thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm đánh giá: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = productId });
        }


        [Authorize]
        [HttpPost]
        public IActionResult AddReply(int reviewId, string comment)
        {
            var maKH = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để trả lời đánh giá";
                return RedirectToAction("Details", new { id = reviewId });
            }

            var parentReview = _context.ProductReviews
                .Include(r => r.Product)
                .FirstOrDefault(r => r.Id == reviewId);
            if (parentReview == null)
            {
                return NotFound();
            }

            var reply = new ProductReview
            {
                ProductId = parentReview.ProductId,
                UserId = maKH,
                Comment = comment,
                CreatedAt = DateTime.Now,
                ParentReviewId = reviewId,
                Rating = null // Set a default valid rating value
            };

            try
            {
                _context.ProductReviews.Add(reply);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã thêm trả lời thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm trả lời: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = parentReview.ProductId });
        }



        private bool HangHoaExists(int id)
        {
            return _context.HangHoas.Any(e => e.MaHh == id);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ToggleReviewReaction(int reviewId, int type)
        {
            // type: 1 = like, -1 = dislike
            var maKH = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var review = _context.ProductReviews
                .Include(r => r.ReviewReactions)
                .FirstOrDefault(r => r.Id == reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            var existing = _context.ReviewReactions.FirstOrDefault(rr => rr.ReviewId == reviewId && rr.UserId == maKH);

            if (existing == null)
            {
                if (type == 1 || type == -1)
                {
                    _context.ReviewReactions.Add(new ReviewReaction
                    {
                        ReviewId = reviewId,
                        UserId = maKH,
                        Type = type,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            else
            {
                if (existing.Type == type)
                {
                    _context.ReviewReactions.Remove(existing);
                }
                else
                {
                    existing.Type = type;
                    _context.ReviewReactions.Update(existing);
                }
            }

            _context.SaveChanges();

            var like = _context.ReviewReactions.Count(rr => rr.ReviewId == reviewId && rr.Type == 1);
            var dislike = _context.ReviewReactions.Count(rr => rr.ReviewId == reviewId && rr.Type == -1);
            var userReaction = _context.ReviewReactions
                .Where(rr => rr.ReviewId == reviewId && rr.UserId == maKH)
                .Select(rr => rr.Type)
                .FirstOrDefault();

            return Json(new { success = true, like, dislike, userReaction });
        }
    }
}
