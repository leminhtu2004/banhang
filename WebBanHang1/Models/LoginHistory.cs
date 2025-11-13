using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class LoginHistory
{
    public int Id { get; set; }

    public string MaKh { get; set; } = null!;

    public string LoaiDangNhap { get; set; } = null!;

    public string? Ipaddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime NgayDangNhap { get; set; }

    public bool ThanhCong { get; set; }

    public string? GhiChu { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
