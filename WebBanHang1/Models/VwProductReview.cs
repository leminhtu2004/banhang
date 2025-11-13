using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class VwProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? UserAvatar { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ParentReviewId { get; set; }

    public string? ParentUserName { get; set; }

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }

    public int? ImageCount { get; set; }

    public int IsMainReview { get; set; }
}
