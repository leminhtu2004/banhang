using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class ReviewEmote
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public string UserId { get; set; } = null!;

    public string EmoteType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ProductReview Review { get; set; } = null!;

    public virtual KhachHang User { get; set; } = null!;
}
