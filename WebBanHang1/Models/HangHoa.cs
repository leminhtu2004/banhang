using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class HangHoa
{
    public int MaHh { get; set; }

    public string TenHh { get; set; } = null!;

    public int MaLoai { get; set; }

    public string? Hinh { get; set; }

    public decimal DonGia { get; set; }

    public string? MoTa { get; set; }

    public DateOnly NgaySx { get; set; }

    public decimal GiamGia { get; set; }

    public int SoLanXem { get; set; }

    public string MaNcc { get; set; } = null!;

    public int SoLuong { get; set; }

    public virtual ICollection<ChiTietHd> ChiTietHds { get; set; } = new List<ChiTietHd>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual Loai MaLoaiNavigation { get; set; } = null!;

    public virtual NhaCungCap MaNccNavigation { get; set; } = null!;

    public virtual ICollection<ProductRatingSummary> ProductRatingSummaries { get; set; } = new List<ProductRatingSummary>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<SanPhamGiamGium> SanPhamGiamGia { get; set; } = new List<SanPhamGiamGium>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
