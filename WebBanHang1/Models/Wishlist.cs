using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class Wishlist
{
    public int MaWishlist { get; set; }

    public string MaKh { get; set; } = null!;

    public int MaHh { get; set; }

    public DateTime NgayThem { get; set; }

    public virtual HangHoa MaHhNavigation { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
