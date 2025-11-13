using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class VwProductRatingSummary
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public int FiveStarCount { get; set; }

    public int FourStarCount { get; set; }

    public int ThreeStarCount { get; set; }

    public int TwoStarCount { get; set; }

    public int OneStarCount { get; set; }

    public int TotalLikes { get; set; }

    public int TotalDislikes { get; set; }

    public int TotalImages { get; set; }

    public DateTime LastUpdated { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductImage { get; set; }

    public decimal DonGia { get; set; }

    public decimal GiamGia { get; set; }

    public decimal? FiveStarPercentage { get; set; }

    public decimal? FourStarPercentage { get; set; }

    public decimal? ThreeStarPercentage { get; set; }

    public decimal? TwoStarPercentage { get; set; }

    public decimal? OneStarPercentage { get; set; }
}
