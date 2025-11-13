using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class VwProductReviewsWithDetail
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string UserId { get; set; } = null!;

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ParentReviewId { get; set; }

    public int Likes { get; set; }

    public int Dislikes { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    public bool IsHelpful { get; set; }

    public string? ReviewImages { get; set; }

    public string UserName { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public decimal? AverageRating { get; set; }

    public int? TotalReviews { get; set; }
}
