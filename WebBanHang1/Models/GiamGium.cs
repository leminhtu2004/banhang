using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class GiamGium
{
    public string MaGiamGia { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public string LoaiGiamGia { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal? GiaTriToiThieu { get; set; }

    public decimal? GiaTriToiDa { get; set; }

    public int? SoLuongSuDung { get; set; }

    public int? SoLuongDaSuDung { get; set; }

    public bool HieuLuc { get; set; }

    public bool MotLanSuDung { get; set; }

    public int? SoLanSuDungToiDaMoiKhachHang { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? NguoiTao { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<KhachHangSuDungMa> KhachHangSuDungMas { get; set; } = new List<KhachHangSuDungMa>();

    public virtual ICollection<SanPhamGiamGium> SanPhamGiamGia { get; set; } = new List<SanPhamGiamGium>();
}
