using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class KhachHangSuDungMa
{
    public int Id { get; set; }

    public string MaGiamGia { get; set; } = null!;

    public string MaKh { get; set; } = null!;

    public int SoLanSuDung { get; set; }

    public DateTime? LanSuDungCuoi { get; set; }

    public virtual GiamGium MaGiamGiaNavigation { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
