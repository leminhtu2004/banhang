using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class ReviewImage
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public string ImagePath { get; set; } = null!;

    public string ImageName { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public bool IsMain { get; set; }

    public virtual ProductReview Review { get; set; } = null!;
}
