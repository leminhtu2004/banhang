using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class GioHang
{
    public int MaGh { get; set; }

    public string MaKh { get; set; } = null!;

    public int MaHh { get; set; }

    public short SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public virtual HangHoa MaHhNavigation { get; set; } = null!;
}
