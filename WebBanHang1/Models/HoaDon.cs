using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class HoaDon
{
    public int MaHd { get; set; }

    public string MaKh { get; set; } = null!;

    public DateTime NgayDat { get; set; }

    public DateTime? NgayGiao { get; set; }

    public string DiaChi { get; set; } = null!;

    public string CachThanhToan { get; set; } = null!;

    public decimal PhiVanChuyen { get; set; }

    public int MaTrangThai { get; set; }

    public string? MaGiamGia { get; set; }

    public decimal? GiamGia { get; set; }

    public virtual ICollection<ChiTietHd> ChiTietHds { get; set; } = new List<ChiTietHd>();

    public virtual GiamGium? MaGiamGiaNavigation { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual TrangThai MaTrangThaiNavigation { get; set; } = null!;
}
