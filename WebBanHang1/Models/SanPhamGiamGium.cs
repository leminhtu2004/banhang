using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class SanPhamGiamGium
{
    public int Id { get; set; }

    public string MaGiamGia { get; set; } = null!;

    public int MaHh { get; set; }

    public virtual GiamGium MaGiamGiaNavigation { get; set; } = null!;

    public virtual HangHoa MaHhNavigation { get; set; } = null!;
}
