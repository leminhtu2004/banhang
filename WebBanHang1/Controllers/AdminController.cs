using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang1.Data;
using WebBanHang1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebBanHang1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly QuanLiHangContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(QuanLiHangContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Phương thức Index
        public async Task<IActionResult> Index()
        {
            // Nếu chưa đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            // Nếu đã đăng nhập nhưng không phải Admin
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var products = await _context.HangHoas.Include(h => h.MaNccNavigation).ToListAsync();
                var loaiSanPhams = await _context.Loais.ToListAsync();
                var orders = await _context.HoaDons
                    .Include(h => h.MaTrangThaiNavigation)
                    .Include(h => h.MaKhNavigation)
                    .Include(h => h.ChiTietHds)
                    .Include(h => h.MaGiamGiaNavigation)
                    .ToListAsync();

                var totalRevenue = await CalculateTotalRevenue(DateTime.Now.Year, DateTime.Now.Month);

                // Thêm tổng số khách hàng và top 5 khách hàng mới nhất
                ViewBag.TotalCustomers = await _context.KhachHangs.CountAsync();
                ViewBag.LatestCustomers = await _context.KhachHangs
                    .OrderByDescending(k => k.NgaySinh) // Nếu có trường NgayTao thì nên dùng trường đó
                    .Take(5)
                    .ToListAsync();
                ViewBag.AllCustomers = await _context.KhachHangs.ToListAsync();
                // Lấy tất cả đơn đã xử lý thành công và chi tiết của chúng
                var processedOrders = await _context.HoaDons
                    .Where(hd => hd.MaTrangThai == 2)
                    .Include(hd => hd.ChiTietHds)
                    .ToListAsync();
                ViewBag.TotalOrders = processedOrders.Count;
                ViewBag.TotalProductsSold = processedOrders.SelectMany(hd => hd.ChiTietHds).Sum(ct => ct.SoLuong);

                var viewModel = new AdminViewModel
                {
                    Products = products,
                    LoaiSanPhams = loaiSanPhams,
                    Orders = orders,
                    TotalRevenue = totalRevenue
                };

                ViewBag.Categories = loaiSanPhams;

                await UpdateStatisticsData();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy dữ liệu cho trang Index.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải dữ liệu.";
                return RedirectToAction("Error", "Home");
            }
        }

        // Phương thức quản lý khách hàng
        public async Task<IActionResult> Customers()
        {
            try
            {
                var customers = await _context.KhachHangs.Include(k => k.VaiTroNavigation).ToListAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách khách hàng.");
                TempData["ErrorMessage"] = "Không thể tải danh sách khách hàng.";
                return RedirectToAction("Error", "Home");
            }
        }

        // Phương thức xem chi tiết khách hàng
        [HttpGet]
        public async Task<IActionResult> CustomerDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.KhachHangs
                .Include(k => k.VaiTroNavigation)
                .FirstOrDefaultAsync(k => k.MaKh == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // Phương thức thêm khách hàng
        [HttpGet]
        public IActionResult AddCustomer()
        {
            ViewBag.Roles = new SelectList(_context.PhanQuyens, "VaiTro", "TenVaiTro");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer(KhachHang customer)
        {
            // Bỏ validate MaKh và VaiTroNavigation khi thêm khách hàng
            ModelState.Remove("MaKh");
            ModelState.Remove("VaiTroNavigation");
            if (ModelState.IsValid)
            {
                try
                {
                    // Tự động sinh MaKh nếu chưa có
                    if (string.IsNullOrEmpty(customer.MaKh))
                    {
                        var lastCustomer = _context.KhachHangs.OrderByDescending(u => u.MaKh).FirstOrDefault();
                        int lastCustomerNumber = 0;
                        if (lastCustomer != null)
                        {
                            int.TryParse(lastCustomer.MaKh, out lastCustomerNumber);
                        }
                        customer.MaKh = (lastCustomerNumber + 1).ToString("D3");
                    }
                    customer.HieuLuc = true; // Mặc định hiệu lực là true
                    _context.KhachHangs.Add(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm khách hàng.");
                    TempData["ErrorMessage"] = "Không thể thêm khách hàng.";
                }
            }

            ViewBag.Roles = new SelectList(_context.PhanQuyens, "VaiTro", "TenVaiTro");
            return View(customer);
        }

        // Phương thức chỉnh sửa khách hàng
        [HttpGet]
        public async Task<IActionResult> EditCustomer(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new SelectList(_context.PhanQuyens, "VaiTro", "TenVaiTro");
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(string id, KhachHang customer)
        {
            // Bỏ validate MatKhau và VaiTroNavigation khi chỉnh sửa
            ModelState.Remove("MatKhau");
            ModelState.Remove("VaiTroNavigation");
            if (id != customer.MaKh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.KhachHangs.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }
                    // Cập nhật các trường cho phép sửa
                    existingCustomer.HoTen = customer.HoTen;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.DienThoai = customer.DienThoai;
                    existingCustomer.DiaChi = customer.DiaChi;
                    existingCustomer.VaiTro = customer.VaiTro;
                    existingCustomer.GioiTinh = customer.GioiTinh;
                    existingCustomer.NgaySinh = customer.NgaySinh;
                    existingCustomer.HieuLuc = customer.HieuLuc;
                    // Không cập nhật MatKhau nếu không nhập mới

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.MaKh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Roles = new SelectList(_context.PhanQuyens, "VaiTro", "TenVaiTro");
            return View(customer);
        }

        // Phương thức xóa khách hàng (GET)
        [HttpGet]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.KhachHangs
                .Include(k => k.VaiTroNavigation)
                .FirstOrDefaultAsync(k => k.MaKh == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // Phương thức xóa khách hàng (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomerConfirmed(string id)
        {
            try
            {
                var customer = await _context.KhachHangs.FindAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                _context.KhachHangs.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa khách hàng.");
                TempData["ErrorMessage"] = "Không thể xóa khách hàng.";
                return RedirectToAction("Index");
            }
        }

        // Phương thức kiểm tra khách hàng tồn tại
        private bool CustomerExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaKh == id);
        }

        // Phương thức tính tổng doanh thu
        public async Task<decimal> CalculateTotalRevenue(int year, int month)
        {
            _logger.LogInformation($"Calculating revenue for {year}/{month}");
            
            var orders = await _context.HoaDons
                .Where(hd => hd.MaTrangThai == 2 && hd.NgayDat.Year == year && hd.NgayDat.Month == month)
                .Include(hd => hd.ChiTietHds)
                .ToListAsync();

            _logger.LogInformation($"Found {orders.Count} processed orders for {year}/{month}");

            decimal totalRevenue = 0;
            foreach (var order in orders)
            {
                foreach (var detail in order.ChiTietHds)
                {
                    decimal itemRevenue = detail.SoLuong * detail.DonGia * (1 - detail.GiamGia / 100);
                    _logger.LogInformation($"Order {order.MaHd}, Product {detail.MaHh}: Quantity={detail.SoLuong}, Price={detail.DonGia}, Discount={detail.GiamGia}%, Revenue={itemRevenue}");
                    totalRevenue += itemRevenue;
                }
            }

            _logger.LogInformation($"Total revenue for {year}/{month}: {totalRevenue}");
            return totalRevenue;
        }

        // Phương thức cập nhật dữ liệu thống kê
        private async Task UpdateStatisticsData()
        {
            var statisticsData = await GetUpdatedStatisticsData();

            ViewBag.RevenueData = statisticsData.RevenueData;
            ViewBag.TopSellingProducts = statisticsData.TopSellingProducts;
            ViewBag.OrderStatusData = statisticsData.OrderStatusData;

            ViewBag.TotalRevenue = await CalculateTotalRevenue(DateTime.Now.Year, DateTime.Now.Month);
        }

        // Phương thức lấy dữ liệu thống kê
        private async Task<StatisticsData> GetUpdatedStatisticsData()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var revenueData = await _context.HoaDons
                .Where(hd => hd.MaTrangThai == 2 && hd.NgayDat.Year == year && hd.NgayDat.Month == month)
                .GroupBy(hd => hd.NgayDat.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalRevenue = g.Sum(hd => hd.ChiTietHds.Sum(ct => ct.SoLuong * ct.DonGia * (1 - ct.GiamGia / 100)))
                })
                .ToListAsync();

            var topSellingProducts = await _context.ChiTietHds
                .Where(ct => ct.MaHdNavigation.MaTrangThai == 2 &&
                             ct.MaHdNavigation.NgayDat.Year == year &&
                             ct.MaHdNavigation.NgayDat.Month == month)
                .GroupBy(ct => ct.MaHh)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(ct => ct.SoLuong),
                    ProductName = g.First().MaHhNavigation.TenHh
                })
                .OrderByDescending(g => g.TotalQuantity)
                .Take(10)
                .ToListAsync();

            var orderStatusData = await _context.HoaDons
                .Where(hd => hd.NgayDat.Year == year && hd.NgayDat.Month == month)
                .GroupBy(hd => hd.MaTrangThaiNavigation.TenTrangThai)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new StatisticsData
            {
                RevenueData = revenueData,
                TopSellingProducts = topSellingProducts,
                OrderStatusData = orderStatusData
            };
        }

        // Lớp chứa dữ liệu thống kê
        private class StatisticsData
        {
            public dynamic RevenueData { get; set; }
            public dynamic TopSellingProducts { get; set; }
            public dynamic OrderStatusData { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryStatistics(int year, int month, string category = "")
        {
            var query = _context.Loais
                .Select(l => new CategoryStatistics
                {
                    CategoryId = l.MaLoai,
                    CategoryName = l.TenLoai,
                    ProductCount = l.HangHoas.Count,
                    TotalSold = l.HangHoas
                        .SelectMany(h => h.ChiTietHds)
                        .Where(c => c.MaHdNavigation.MaTrangThai == 2 && c.MaHdNavigation.NgayDat.Year == year && c.MaHdNavigation.NgayDat.Month == month)
                        .Sum(c => c.SoLuong),
                    TotalRevenue = l.HangHoas
                        .SelectMany(h => h.ChiTietHds)
                        .Where(c => c.MaHdNavigation.MaTrangThai == 2 && c.MaHdNavigation.NgayDat.Year == year && c.MaHdNavigation.NgayDat.Month == month)
                        .Sum(c => c.SoLuong * c.DonGia * (1 - c.GiamGia / 100))
                });

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.CategoryId.ToString() == category);
            }

            var statistics = await query.ToListAsync();
            return Json(statistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierStatistics(int year, int month, string category = "")
        {
            var query = _context.NhaCungCaps
                .Select(n => new SupplierStatistics
                {
                    SupplierId = n.MaNcc,
                    SupplierName = n.TenCongTy,
                    ProductCount = n.HangHoas.Count,
                    TotalSold = n.HangHoas
                        .SelectMany(h => h.ChiTietHds)
                        .Where(c => c.MaHdNavigation.MaTrangThai == 2 && c.MaHdNavigation.NgayDat.Year == year && c.MaHdNavigation.NgayDat.Month == month)
                        .Sum(c => c.SoLuong),
                    TotalRevenue = n.HangHoas
                        .SelectMany(h => h.ChiTietHds)
                        .Where(c => c.MaHdNavigation.MaTrangThai == 2 && c.MaHdNavigation.NgayDat.Year == year && c.MaHdNavigation.NgayDat.Month == month)
                        .Sum(c => c.SoLuong * c.DonGia * (1 - c.GiamGia / 100))
                });

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.ProductCount > 0);
            }

            var statistics = await query.ToListAsync();
            return Json(statistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetDiscountStatistics(int year, int month)
        {
            var discounts = await _context.GiamGia
                .Include(g => g.HoaDons)
                .ThenInclude(h => h.ChiTietHds)
                .ToListAsync();

            var statistics = discounts.Select(g => new DiscountStatistics
            {
                DiscountCode = g.MaGiamGia,
                DiscountValue = g.GiaTriGiam,
                UsageCount = g.HoaDons.Count(h => h.MaTrangThai == 2 && h.NgayDat.Year == year && h.NgayDat.Month == month),
                TotalDiscount = g.HoaDons
                    .Where(h => h.MaTrangThai == 2 && h.NgayDat.Year == year && h.NgayDat.Month == month)
                    .Sum(h => h.ChiTietHds.Sum(c => c.SoLuong * c.DonGia * g.GiaTriGiam / 100))
            }).ToList();

            return Json(statistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailedRevenueData(int year, int month, string category = "")
        {
            var query = _context.HoaDons
                .Where(h => h.MaTrangThai == 2 && h.NgayDat.Year == year && h.NgayDat.Month == month)
                .GroupBy(h => h.NgayDat.Date)
                .Select(g => new RevenueStatistics
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(h => h.ChiTietHds.Sum(c => c.SoLuong * c.DonGia * (1 - c.GiamGia / 100))),
                    TotalOrders = g.Count(),
                    AverageOrderValue = g.Average(h => h.ChiTietHds.Sum(c => c.SoLuong * c.DonGia * (1 - c.GiamGia / 100))),
                    TotalDiscount = g.Sum(h => h.ChiTietHds.Sum(c => c.SoLuong * c.DonGia * c.GiamGia / 100)),
                    ShippingFee = g.Sum(h => h.PhiVanChuyen)
                });

            var statistics = await query.OrderBy(r => r.Date).ToListAsync();
            return Json(statistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailedProductData(int? month, int? year)
        {
            var query = _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .AsQueryable();

            // Lấy tổng số lượng đã bán từ các đơn hàng đã xử lý thành công
            var soldQuantities = await _context.ChiTietHds
                .Include(ct => ct.MaHdNavigation)
                .Where(ct => ct.MaHdNavigation.MaTrangThai == 2) // Chỉ tính đơn hàng đã xử lý thành công
                .GroupBy(ct => ct.MaHh)
                .Select(g => new { 
                    MaHh = g.Key, 
                    TotalSold = g.Sum(ct => ct.SoLuong) 
                })
                .ToDictionaryAsync(x => x.MaHh, x => x.TotalSold);

            var products = await query.ToListAsync();

            var result = products.Select(p => new
            {
                p.MaHh,
                p.TenHh,
                p.DonGia,
                p.SoLuong, // Số lượng trong kho
                SoldQuantity = soldQuantities.ContainsKey(p.MaHh) ? soldQuantities[p.MaHh] : 0, // Số lượng đã bán
                AvailableQuantity = p.SoLuong - (soldQuantities.ContainsKey(p.MaHh) ? soldQuantities[p.MaHh] : 0), // Số lượng tồn kho thực tế
                CategoryName = p.MaLoaiNavigation?.TenLoai ?? "N/A",
                SupplierName = p.MaNccNavigation?.TenCongTy ?? "N/A"
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            var order = _context.HoaDons
                .Include(h => h.ChiTietHds)
                .ThenInclude(c => c.MaHhNavigation)
                .Include(h => h.MaKhNavigation)
                .Include(h => h.MaTrangThaiNavigation)
                .Include(h => h.MaGiamGiaNavigation)
                .FirstOrDefault(h => h.MaHd == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        public class UpdateOrderStatusModel
        {
            public int OrderId { get; set; }
            public int StatusId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusModel model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var order = await _context.HoaDons.FindAsync(model.OrderId);
            if (order == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            order.MaTrangThai = model.StatusId;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
        }
    }
}
