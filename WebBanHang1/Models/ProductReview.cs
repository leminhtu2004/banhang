using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class ProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string UserId { get; set; } = null!;

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ParentReviewId { get; set; }

    public virtual ICollection<ProductReview> InverseParentReview { get; set; } = new List<ProductReview>();

    public virtual ProductReview? ParentReview { get; set; }

    public virtual HangHoa Product { get; set; } = null!;

    public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();

    public virtual ICollection<ReviewReaction> ReviewReactions { get; set; } = new List<ReviewReaction>();

    public virtual KhachHang User { get; set; } = null!;
}
