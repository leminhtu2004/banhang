using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebBanHang1.Models;
using WebBanHang1.Data;

namespace WebBanHang1.Services
{
    public interface IPromotionService
    {
        Task<List<GiamGium>> GetAllPromotionsAsync();
        Task<GiamGium> GetPromotionByIdAsync(string maGiamGia);
        Task<GiamGium> CreatePromotionAsync(GiamGium promotion);
        Task<GiamGium> UpdatePromotionAsync(GiamGium promotion);
        Task DeletePromotionAsync(string maGiamGia);
        Task<(bool isValid, string message)> ValidatePromotionAsync(string maGiamGia, decimal totalAmount, string maKH = null, List<int> productIds = null);
        Task<decimal> CalculateDiscountAsync(string maGiamGia, decimal totalAmount);
        Task<bool> ApplyPromotionToOrderAsync(string maGiamGia, int maHD, string maKH);
        Task<bool> CancelPromotionFromOrderAsync(int maHD);
        Task<List<PromotionUsageReport>> GetPromotionUsageReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task DeactivateExpiredPromotionsAsync();
        Task DeleteInactivePromotionsAsync();
        Task<List<GiamGium>> GetActivePromotionsForCustomerAsync(string maKH);
        Task<KhachHangSuDungMa> GetCustomerPromotionUsageAsync(string maGiamGia, string maKH);
    }

    public class PromotionService : IPromotionService
    {
        private readonly QuanLiHangContext _context;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(QuanLiHangContext context, ILogger<PromotionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GiamGium>> GetAllPromotionsAsync()
        {
            return await _context.GiamGia
                .Include(p => p.SanPhamGiamGia)
                .OrderByDescending(p => p.NgayBatDau)
                .ToListAsync();
        }

        public async Task<GiamGium> GetPromotionByIdAsync(string maGiamGia)
        {
            return await _context.GiamGia
                .Include(p => p.SanPhamGiamGia)
                .FirstOrDefaultAsync(p => p.MaGiamGia == maGiamGia);
        }

        public async Task<GiamGium> CreatePromotionAsync(GiamGium promotion)
        {
            promotion.NgayTao = DateTime.Now;
            promotion.HieuLuc = true;
            _context.GiamGia.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<GiamGium> UpdatePromotionAsync(GiamGium promotion)
        {
            promotion.NgayCapNhat = DateTime.Now;
            _context.Entry(promotion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task DeletePromotionAsync(string maGiamGia)
        {
            var promotion = await _context.GiamGia.FindAsync(maGiamGia);
            if (promotion != null)
            {
                _context.GiamGia.Remove(promotion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(bool isValid, string message)> ValidatePromotionAsync(string maGiamGia, decimal totalAmount, string maKH = null, List<int> productIds = null)
        {
            var promotion = await _context.GiamGia
                .Include(p => p.SanPhamGiamGia)
                .FirstOrDefaultAsync(p => p.MaGiamGia == maGiamGia);

            if (promotion == null)
                return (false, "Mã giảm giá không tồn tại");

            if (!promotion.HieuLuc)
                return (false, "Mã giảm giá đã bị vô hiệu hóa");

            var now = DateTime.Now;
            if (now < promotion.NgayBatDau)
                return (false, "Mã giảm giá chưa đến thời gian áp dụng");
            if (now > promotion.NgayKetThuc)
                return (false, "Mã giảm giá đã hết hạn");

            if (promotion.GiaTriToiThieu.HasValue && totalAmount < promotion.GiaTriToiThieu.Value)
                return (false, $"Đơn hàng phải có giá trị tối thiểu {promotion.GiaTriToiThieu.Value:N0} VNĐ");

            if (promotion.GiaTriToiDa.HasValue && totalAmount > promotion.GiaTriToiDa.Value)
                return (false, $"Đơn hàng không được vượt quá {promotion.GiaTriToiDa.Value:N0} VNĐ");

            if (promotion.SoLuongSuDung.HasValue && promotion.SoLuongDaSuDung >= promotion.SoLuongSuDung)
                return (false, "Mã giảm giá đã hết lượt sử dụng toàn cầu");

            if (productIds != null && promotion.SanPhamGiamGia.Any())
            {
                var validProducts = promotion.SanPhamGiamGia.Select(p => p.MaHh).ToList();
                if (!productIds.Any(p => validProducts.Contains(p)))
                    return (false, "Mã giảm giá không áp dụng cho sản phẩm trong giỏ hàng");
            }

            if (!string.IsNullOrEmpty(maKH))
            {
                var customerUsage = await _context.KhachHangSuDungMas
                    .FirstOrDefaultAsync(k => k.MaGiamGia == maGiamGia && k.MaKh == maKH);

                if (promotion.MotLanSuDung && customerUsage != null && customerUsage.SoLanSuDung > 0)
                    return (false, "Mã giảm giá chỉ được sử dụng một lần cho mỗi khách hàng");

                if (promotion.SoLanSuDungToiDaMoiKhachHang.HasValue && 
                    customerUsage != null && 
                    customerUsage.SoLanSuDung >= promotion.SoLanSuDungToiDaMoiKhachHang)
                    return (false, $"Bạn đã sử dụng hết số lần cho phép ({promotion.SoLanSuDungToiDaMoiKhachHang} lần)");
            }

            return (true, "Mã giảm giá hợp lệ");
        }

        public async Task<decimal> CalculateDiscountAsync(string maGiamGia, decimal totalAmount)
        {
            var promotion = await _context.GiamGia.FindAsync(maGiamGia);
            if (promotion == null)
                return 0;

            // Validate without customer/product context for general calculation
            var validation = await ValidatePromotionAsync(maGiamGia, totalAmount);
            if (!validation.isValid)
                return 0;

            decimal discount = 0;
            switch (promotion.LoaiGiamGia.ToLower())
            {
                case "phantram":
                    discount = totalAmount * (promotion.GiaTriGiam / 100);
                    break;
                case "tien":
                    discount = promotion.GiaTriGiam;
                    break;
                case "flashsale":
                    discount = totalAmount * (promotion.GiaTriGiam / 100);
                    break;
            }

            return Math.Min(discount, totalAmount); // Đảm bảo giảm giá không vượt quá tổng tiền
        }

        public async Task<bool> ApplyPromotionToOrderAsync(string maGiamGia, int maHD, string maKH)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.HoaDons.FindAsync(maHD);
                if (order == null)
                    return false;

                var totalAmount = await _context.ChiTietHds
                    .Where(c => c.MaHd == maHD)
                    .SumAsync(c => c.DonGia * c.SoLuong);

                // Validate with customer context
                var validation = await ValidatePromotionAsync(maGiamGia, totalAmount, maKH);
                if (!validation.isValid)
                    return false;

                var discount = await CalculateDiscountAsync(maGiamGia, totalAmount);
                
                // Cập nhật đơn hàng
                order.MaGiamGia = maGiamGia;
                order.GiamGia = discount;
                _context.Entry(order).State = EntityState.Modified;

                // Cập nhật số lượng sử dụng toàn cầu
                var promotion = await _context.GiamGia.FindAsync(maGiamGia);
                if (promotion.SoLuongDaSuDung.HasValue)
                {
                    promotion.SoLuongDaSuDung++;
                    _context.Entry(promotion).State = EntityState.Modified;
                }

                // Cập nhật lịch sử sử dụng của khách hàng
                var customerUsage = await _context.KhachHangSuDungMas
                    .FirstOrDefaultAsync(k => k.MaGiamGia == maGiamGia && k.MaKh == maKH);

                if (customerUsage == null)
                {
                    customerUsage = new KhachHangSuDungMa
                    {
                        MaGiamGia = maGiamGia,
                        MaKh = maKH,
                        SoLanSuDung = 1,
                        LanSuDungCuoi = DateTime.Now
                    };
                    _context.KhachHangSuDungMas.Add(customerUsage);
                }
                else
                {
                    customerUsage.SoLanSuDung++;
                    customerUsage.LanSuDungCuoi = DateTime.Now;
                    _context.Entry(customerUsage).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi áp dụng mã giảm giá cho đơn hàng {MaHD}", maHD);
                return false;
            }
        }

        public async Task<bool> CancelPromotionFromOrderAsync(int maHD)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.HoaDons.FindAsync(maHD);
                if (order == null || string.IsNullOrEmpty(order.MaGiamGia))
                    return false;

                var promotion = await _context.GiamGia.FindAsync(order.MaGiamGia);
                if (promotion != null && promotion.SoLuongDaSuDung.HasValue)
                {
                    promotion.SoLuongDaSuDung--;
                    _context.Entry(promotion).State = EntityState.Modified;
                }

                // Cập nhật lịch sử sử dụng của khách hàng
                var customerUsage = await _context.KhachHangSuDungMas
                    .FirstOrDefaultAsync(k => k.MaGiamGia == order.MaGiamGia && k.MaKh == order.MaKh);

                if (customerUsage != null && customerUsage.SoLanSuDung > 0)
                {
                    customerUsage.SoLanSuDung--;
                    _context.Entry(customerUsage).State = EntityState.Modified;
                }

                // Xóa mã giảm giá khỏi đơn hàng
                order.MaGiamGia = null;
                order.GiamGia = 0;
                _context.Entry(order).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi hủy mã giảm giá khỏi đơn hàng {MaHD}", maHD);
                return false;
            }
        }

        public async Task<List<GiamGium>> GetActivePromotionsForCustomerAsync(string maKH)
        {
             var now = DateTime.Now;
             var activePromotions = await _context.GiamGia
                 .Include(p => p.SanPhamGiamGia)
                 .Where(p => p.HieuLuc && p.NgayBatDau <= now && p.NgayKetThuc >= now
                            && (!p.SoLuongSuDung.HasValue || p.SoLuongDaSuDung < p.SoLuongSuDung))
                 .ToListAsync();

            // Filter based on customer-specific usage limit
            var promotionsForCustomer = new List<GiamGium>();
            foreach (var promo in activePromotions)
            {
                if (promo.SoLanSuDungToiDaMoiKhachHang.HasValue || promo.MotLanSuDung)
                {
                    var customerUsage = await _context.KhachHangSuDungMas
                        .FirstOrDefaultAsync(k => k.MaGiamGia == promo.MaGiamGia && k.MaKh == maKH);

                    if (promo.MotLanSuDung)
                    {
                        if (customerUsage == null || customerUsage.SoLanSuDung == 0)
                        {
                             promotionsForCustomer.Add(promo);
                        }
                    }
                    else if (promo.SoLanSuDungToiDaMoiKhachHang.HasValue)
                    {
                         if (customerUsage == null || customerUsage.SoLanSuDung < promo.SoLanSuDungToiDaMoiKhachHang)
                         {
                             promotionsForCustomer.Add(promo);
                         }
                    }
                }
                else
                {
                    // Promotion does not have customer-specific limits
                     promotionsForCustomer.Add(promo);
                }
            }

             return promotionsForCustomer;
        }

         public async Task<KhachHangSuDungMa> GetCustomerPromotionUsageAsync(string maGiamGia, string maKH)
         {
             return await _context.KhachHangSuDungMas
                 .FirstOrDefaultAsync(k => k.MaGiamGia == maGiamGia && k.MaKh == maKH);
         }

        public async Task<List<PromotionUsageReport>> GetPromotionUsageReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.HoaDons
                .Where(h => h.MaGiamGia != null);

            if (startDate.HasValue)
                query = query.Where(q => q.NgayDat >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(q => q.NgayDat <= endDate.Value);

            var reportData = await query
                .GroupBy(h => h.MaGiamGia)
                .Select(g => new PromotionUsageReport
                {
                    MaGiamGia = g.Key,
                    SoDonHang = g.Count(),
                    TongGiaTriGiamGia = g.Sum(h => h.GiamGia ?? 0),
                    TongDoanhThu = g.Sum(h => h.ChiTietHds.Sum(c => c.DonGia * c.SoLuong))
                }).ToListAsync();

            return reportData;
        }

        public async Task DeactivateExpiredPromotionsAsync()
        {
            var expiredPromotions = await _context.GiamGia
                .Where(p => p.HieuLuc && (p.NgayKetThuc < DateTime.Now || (p.SoLuongSuDung.HasValue && p.SoLuongDaSuDung >= p.SoLuongSuDung)))
                .ToListAsync();

            foreach (var promotion in expiredPromotions)
            {
                promotion.HieuLuc = false;
                promotion.NgayCapNhat = DateTime.Now;
            }

            if (expiredPromotions.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã vô hiệu hóa {Count} mã giảm giá hết hạn hoặc hết lượt sử dụng toàn cầu", expiredPromotions.Count);
            }
        }

         public async Task DeleteInactivePromotionsAsync()
        {
            var promotionsToDelete = await _context.GiamGia
                 .Where(p => !p.HieuLuc && (p.NgayCapNhat ?? p.NgayTao).Value.AddHours(1) <= DateTime.Now)
                 .ToListAsync();

            if (promotionsToDelete.Any())
            {
                 _context.GiamGia.RemoveRange(promotionsToDelete);
                await _context.SaveChangesAsync();
                 _logger.LogInformation("Đã xóa {Count} mã giảm giá không còn hiệu lực sau 1 giờ", promotionsToDelete.Count);
            }
        }
    }

    public class PromotionUsageReport
    {
        public string MaGiamGia { get; set; }
        public int SoDonHang { get; set; }
        public decimal TongGiaTriGiamGia { get; set; }
        public decimal TongDoanhThu { get; set; }
        // No NgayDat needed here as it's a grouped report
    }
} 