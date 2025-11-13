using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class ChiTietHd
{
    public int MaCt { get; set; }

    public int MaHd { get; set; }

    public int MaHh { get; set; }

    public decimal DonGia { get; set; }

    public short SoLuong { get; set; }

    public decimal GiamGia { get; set; }

    public virtual HoaDon MaHdNavigation { get; set; } = null!;

    public virtual HangHoa MaHhNavigation { get; set; } = null!;
}
