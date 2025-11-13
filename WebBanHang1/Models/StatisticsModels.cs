using System;

namespace WebBanHang1.Models
{
    public class RevenueStatistics
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal ShippingFee { get; set; }
    }

    public class ProductStatistics
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ViewCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class CustomerStatistics
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
    }

    public class OrderStatusStatistics
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class CategoryStatistics
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SupplierStatistics
    {
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ProductCount { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DiscountStatistics
    {
        public string DiscountCode { get; set; }
        public decimal DiscountValue { get; set; }
        public int UsageCount { get; set; }
        public decimal TotalDiscount { get; set; }
    }
} 