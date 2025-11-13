using Microsoft.AspNetCore.Mvc;
using WebBanHang1.Data;
using WebBanHang1.Models;
using WebBanHang1.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using WebBanHang1.Services;

namespace WebBanHang1.Controllers
{
    public class CartController : Controller
    {
        private readonly QuanLiHangContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<VnPayLibrary> _logger;
        private readonly IPromotionService _promotionService;

        public CartController(QuanLiHangContext context, IHttpContextAccessor httpContextAccessor, IVnPayService vnPayService, ILogger<VnPayLibrary> logger, IPromotionService promotionService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _vnPayService = vnPayService;
            _logger = logger;
            _promotionService = promotionService;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _context.GioHangs
                .Include(g => g.MaHhNavigation)
                .Where(g => g.MaKh == maKH)
                .OrderBy(g => g.MaHh)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalItems = _context.GioHangs.Count(g => g.MaKh == maKH);
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;

            return View(cartItems);
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm vào giỏ hàng";
                return RedirectToAction("Index", "Products");
            }

            try
            {
                var product = _context.HangHoas.Find(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("Index", "Products");
                }

                if (product.SoLuong < quantity)
                {
                    TempData["ErrorMessage"] = "Sản phẩm đã hết hàng hoặc không đủ số lượng";
                    return RedirectToAction("Index", "Products");
                }

                var existingCartItem = _context.GioHangs
                    .FirstOrDefault(g => g.MaKh == maKH && g.MaHh == productId);

                if (existingCartItem != null)
                {
                    // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
                    existingCartItem.SoLuong += (short)quantity; // Chuyển đổi rõ ràng
                    _context.Update(existingCartItem);
                }
                else
                {
                    // Nếu sản phẩm chưa có trong giỏ hàng, tạo mới
                    var newCartItem = new GioHang
                    {
                        MaKh = maKH,
                        MaHh = productId,
                        SoLuong = (short)quantity, // Chuyển đổi rõ ràng
                        DonGia = product.DonGia,
                        MaHhNavigation = product
                    };
                    _context.GioHangs.Add(newCartItem);
                }

                // Cập nhật số lượng tồn kho
                product.SoLuong -= quantity;
                _context.Update(product);

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng");
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            // Chuyển hướng đến trang giỏ hàng
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                return RedirectToAction("Index", "Products");
            }

            try
            {
                var cartItem = _context.GioHangs
                    .FirstOrDefault(g => g.MaKh == maKH && g.MaHh == productId);

                if (cartItem == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng";
                    return RedirectToAction("Index");
                }

                if (quantity <= 0)
                {
                    _context.GioHangs.Remove(cartItem);
                }
                else
                {
                    if (quantity > short.MaxValue)
                    {
                        TempData["ErrorMessage"] = "Số lượng vượt quá giới hạn cho phép";
                        return RedirectToAction("Index");
                    }
                    cartItem.SoLuong = (short)quantity;
                    _context.Update(cartItem);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật số lượng thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng sản phẩm");
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập";
                return RedirectToAction("Index", "Products");
            }

            try
            {
                var cartItem = _context.GioHangs
                    .FirstOrDefault(g => g.MaKh == maKH && g.MaHh == productId);

                if (cartItem == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng";
                    return RedirectToAction("Index");
                }

                _context.GioHangs.Remove(cartItem);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng");
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ThanhToan()
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin giỏ hàng
            var cartItems = _context.GioHangs
                .Include(g => g.MaHhNavigation)
                .Where(g => g.MaKh == maKH)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }

            // Lấy thông tin khách hàng
            var khachHang = _context.KhachHangs.Find(maKH);

            // Tính tổng tiền hàng
            decimal tongTienHang = cartItems.Sum(item => item.DonGia * item.SoLuong);
            decimal phiVanChuyen = 30000; // Phí vận chuyển cố định

            // Tạo model cho view
            var hoaDon = new HoaDon
            {
                MaKh = maKH,
                DiaChi = khachHang?.DiaChi ?? string.Empty, // Fix for CS8601
                PhiVanChuyen = phiVanChuyen,
                MaTrangThai = 1 // Trạng thái mặc định: Đang chờ xử lý
            };

            // Truyền các giá trị tạm thời qua ViewBag
            ViewBag.SubTotal = tongTienHang;
            ViewBag.ShippingFee = phiVanChuyen;
            ViewBag.TotalAmount = tongTienHang + phiVanChuyen;

            return View(hoaDon);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(HoaDon hoaDon)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng và tính toán
            var cartItems = _context.GioHangs
                .Include(g => g.MaHhNavigation)
                .Where(g => g.MaKh == maKH)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }

            decimal tongTienHang = cartItems.Sum(item => item.DonGia * item.SoLuong);
            decimal phiVanChuyen = 30000;
            decimal giamGia = 0;
            string promotionMessage = "";

            // Xử lý mã giảm giá sử dụng PromotionService
            if (!string.IsNullOrEmpty(hoaDon.MaGiamGia))
            {
                // Lấy danh sách ID sản phẩm trong giỏ hàng
                var productIds = cartItems.Select(item => item.MaHh).ToList();

                // Validate mã giảm giá với context khách hàng và sản phẩm
                var validation = await _promotionService.ValidatePromotionAsync(
                    hoaDon.MaGiamGia, tongTienHang, maKH, productIds);

                if (validation.isValid)
                {
                    // Tính toán giảm giá chính xác
                    giamGia = await _promotionService.CalculateDiscountAsync(hoaDon.MaGiamGia, tongTienHang);
                    promotionMessage = $"Áp dụng mã giảm giá thành công! Giảm {giamGia:N0} VNĐ";
                }
                else
                {
                    ModelState.AddModelError("MaGiamGia", validation.message);
                    // Xóa mã giảm giá khỏi hóa đơn nếu không hợp lệ
                    hoaDon.MaGiamGia = null;
                }
            }

            decimal tongTienThanhToan = tongTienHang - giamGia + phiVanChuyen;

            // Gán giá trị để hiển thị lại View nếu có lỗi
            ViewBag.SubTotal = tongTienHang;
            ViewBag.ShippingFee = phiVanChuyen;
            ViewBag.DiscountAmount = giamGia;
            ViewBag.TotalAmount = tongTienThanhToan;
            ViewBag.PromotionMessage = promotionMessage;

            // Bỏ qua validation cho các trường không cần thiết
            ModelState.Remove("MaKh");
            ModelState.Remove("MaKhNavigation");
            ModelState.Remove("MaTrangThaiNavigation");

            if (!ModelState.IsValid)
            {
                return View(hoaDon);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Tạo hóa đơn mới
                    var newHoaDon = new HoaDon
                    {
                        MaKh = maKH,
                        NgayDat = DateTime.Now,
                        DiaChi = hoaDon.DiaChi,
                        CachThanhToan = hoaDon.CachThanhToan,
                        PhiVanChuyen = phiVanChuyen,
                        MaGiamGia = hoaDon.MaGiamGia,
                        GiamGia = giamGia, // Lưu số tiền giảm giá
                        MaTrangThai = 1 // Trạng thái "Chờ xử lý"
                    };

                    _context.HoaDons.Add(newHoaDon);
                    _context.SaveChanges();

                    // Thêm chi tiết hóa đơn
                    foreach (var item in cartItems)
                    {
                        _context.ChiTietHds.Add(new ChiTietHd
                        {
                            MaHd = newHoaDon.MaHd,
                            MaHh = item.MaHh,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            GiamGia = 0 // Giảm giá sản phẩm riêng (nếu có)
                        });

                        // Cập nhật số lượng tồn kho
                        var product = _context.HangHoas.Find(item.MaHh);
                        if (product != null)
                        {
                            product.SoLuong -= item.SoLuong;
                            _context.Update(product);
                        }
                    }

                    // Áp dụng mã giảm giá và cập nhật số lượng sử dụng
                    if (!string.IsNullOrEmpty(hoaDon.MaGiamGia))
                    {
                        var applySuccess = await _promotionService.ApplyPromotionToOrderAsync(
                            hoaDon.MaGiamGia, newHoaDon.MaHd, maKH);
                        
                        if (!applySuccess)
                        {
                            throw new Exception("Không thể áp dụng mã giảm giá");
                        }
                    }

                    // Xóa giỏ hàng
                    _context.GioHangs.RemoveRange(cartItems);
                    _context.SaveChanges();

                    transaction.Commit();

                    // Lấy thông tin khách hàng
                    var khachHang = _context.KhachHangs.Find(maKH);

                    // Truyền thông tin qua TempData
                    TempData["SuccessMessage"] = "Đặt hàng thành công!";
                    TempData["CustomerName"] = khachHang?.HoTen;
                    TempData["CustomerEmail"] = khachHang?.Email;
                    TempData["CustomerPhone"] = khachHang?.DienThoai;
                    TempData["CustomerAddress"] = khachHang?.DiaChi;
                    TempData["PaymentMethod"] = hoaDon.CachThanhToan;
                    TempData["DiscountCode"] = hoaDon.MaGiamGia;
                    TempData["TotalAmount"] = tongTienThanhToan.ToString();
                    TempData["OrderId"] = newHoaDon.MaHd;

                    // Xử lý thanh toán VNPAY
                    if (hoaDon.CachThanhToan == "Chuyển khoản")
                    {
                        var paymentUrl = _vnPayService.CreatePaymentUrl(
                            new PaymentInformationModel
                            {
                                OrderId = newHoaDon.MaHd.ToString(),
                                Amount = (double)tongTienThanhToan,
                                OrderDescription = $"Thanh toán đơn hàng {newHoaDon.MaHd}",
                                ReturnUrl = Url.Action("PaymentCallback", "Cart", null, Request.Scheme)
                            },
                            HttpContext
                        );

                        return Redirect(paymentUrl);
                    }

                    // Trả về trang xác nhận nếu thanh toán COD
                    return RedirectToAction("OrderSuccess");
                }
                catch (Exception ex)
                {
                    if (transaction.GetDbTransaction().Connection != null)
                    {
                        transaction.Rollback();
                    }
                    _logger.LogError(ex, "Lỗi trong quá trình thanh toán: {Message}", ex.Message);
                    ModelState.AddModelError(string.Empty, "Lỗi hệ thống: " + ex.Message);
                    return View(hoaDon);
                }
            }
        }


        [Authorize]
        public IActionResult OrderDetails(int id)
        {
            var order = _context.HoaDons
                .Include(h => h.ChiTietHds)
                .ThenInclude(c => c.MaHhNavigation)
                .Include(h => h.MaKhNavigation) // Thêm thông tin khách hàng
                .Include(h => h.MaTrangThaiNavigation) // Thêm thông tin trạng thái
                .Include(h => h.MaGiamGiaNavigation) // Thêm thông tin mã giảm giá
                .FirstOrDefault(h => h.MaHd == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [Authorize]
        public IActionResult OrderHistory(int page = 1, int pageSize = 10)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử mua hàng";
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.HoaDons
                .Include(h => h.MaTrangThaiNavigation)
                .Where(h => h.MaKh == maKH)
                .OrderByDescending(h => h.NgayDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalOrders = _context.HoaDons.Count(h => h.MaKh == maKH);
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.CurrentPage = page;

            return View(orders);
        }


        [Authorize]
        public IActionResult OrderSuccess()
        {
            if (TempData["TotalAmount"] != null)
            {
                ViewBag.TotalAmount = decimal.Parse(TempData["TotalAmount"].ToString()); // Chuyển đổi lại thành Decimal
            }
            return View();
        }
        public IActionResult GenerateInvoice(int orderId)
        {
            var order = _context.HoaDons
                .Include(h => h.ChiTietHds)
                .ThenInclude(c => c.MaHhNavigation)
                .Include(h => h.MaKhNavigation)
                .Include(h => h.MaTrangThaiNavigation)
                .FirstOrDefault(h => h.MaHd == orderId);

            if (order == null)
            {
                return NotFound();
            }

            using (var memoryStream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream).CloseStream = false;
                document.Open();

                // Add content to the PDF
                document.Add(new Paragraph("Hóa Đơn"));
                document.Add(new Paragraph($"Mã Đơn Hàng: {order.MaHd}"));
                document.Add(new Paragraph($"Ngày Đặt: {order.NgayDat.ToString("dd/MM/yyyy")}"));
                document.Add(new Paragraph($"Khách Hàng: {order.MaKhNavigation.HoTen}"));
                document.Add(new Paragraph($"Địa Chỉ: {order.DiaChi}"));
                document.Add(new Paragraph($"Trạng Thái: {order.MaTrangThaiNavigation.TenTrangThai}"));
                document.Add(new Paragraph($"Phương Thức Thanh Toán: {order.CachThanhToan}"));
                document.Add(new Paragraph(" "));

                var table = new PdfPTable(5);
                table.AddCell("Sản Phẩm");
                table.AddCell("Số Lượng");
                table.AddCell("Đơn Giá");
                table.AddCell("Giảm Giá");
                table.AddCell("Thành Tiền");

                foreach (var item in order.ChiTietHds)
                {
                    table.AddCell(item.MaHhNavigation.TenHh);
                    table.AddCell(item.SoLuong.ToString());
                    table.AddCell(item.DonGia.ToString("N0"));
                    table.AddCell(item.GiamGia.ToString("N0"));
                    table.AddCell((item.DonGia * item.SoLuong - item.GiamGia).ToString("N0"));
                }

                document.Add(table);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph($"Tổng Tiền: {order.ChiTietHds.Sum(i => i.DonGia * i.SoLuong - i.GiamGia).ToString("N0")} đ"));

                document.Close();

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/pdf", $"HoaDon_{order.MaHd}.pdf");
            }
        }

        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["ErrorMessage"] = "Thanh toán thất bại: " + response?.VnPayResponseCode;
                return RedirectToAction("PaymentFail");
            }

            if (!int.TryParse(response.OrderId, out int orderId))
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ";
                return RedirectToAction("PaymentFail");
            }

            var order = _context.HoaDons.FirstOrDefault(o => o.MaHd == orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("PaymentFail");
            }

            // Cập nhật trạng thái đơn hàng
            order.MaTrangThai = 2;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán thành công!";
            return RedirectToAction("PaymentSuccess");
        }

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View("Fail");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidatePromotionCode(string promotionCode)
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (string.IsNullOrEmpty(promotionCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
            }

            try
            {
                // Lấy giỏ hàng hiện tại để tính tổng tiền
                var cartItems = _context.GioHangs
                    .Include(g => g.MaHhNavigation)
                    .Where(g => g.MaKh == maKH)
                    .ToList();

                if (!cartItems.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống" });
                }

                decimal tongTienHang = cartItems.Sum(item => item.DonGia * item.SoLuong);
                var productIds = cartItems.Select(item => item.MaHh).ToList();

                // Validate mã giảm giá
                var validation = await _promotionService.ValidatePromotionAsync(
                    promotionCode, tongTienHang, maKH, productIds);

                if (validation.isValid)
                {
                    // Tính toán giảm giá
                    var discount = await _promotionService.CalculateDiscountAsync(promotionCode, tongTienHang);
                    var promotion = await _promotionService.GetPromotionByIdAsync(promotionCode);

                    return Json(new
                    {
                        success = true,
                        message = validation.message,
                        discount = discount,
                        promotion = new
                        {
                            maGiamGia = promotion.MaGiamGia,
                            loaiGiamGia = promotion.LoaiGiamGia,
                            giaTriGiam = promotion.GiaTriGiam,
                            moTa = promotion.MoTa,
                            ngayKetThuc = promotion.NgayKetThuc.ToString("dd/MM/yyyy HH:mm")
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = validation.message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra mã giảm giá: {Message}", ex.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra mã giảm giá" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAvailablePromotions()
        {
            string? maKH = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var availablePromotions = await _promotionService.GetActivePromotionsForCustomerAsync(maKH);
                
                var promotions = availablePromotions.Select(p => new
                {
                    maGiamGia = p.MaGiamGia,
                    loaiGiamGia = p.LoaiGiamGia,
                    giaTriGiam = p.GiaTriGiam,
                    moTa = p.MoTa,
                    ngayKetThuc = p.NgayKetThuc.ToString("dd/MM/yyyy HH:mm"),
                    giaTriToiThieu = p.GiaTriToiThieu,
                    giaTriToiDa = p.GiaTriToiDa
                }).ToList();

                return Json(new { success = true, promotions = promotions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách mã giảm giá: {Message}", ex.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách mã giảm giá" });
            }
        }
    }
}
