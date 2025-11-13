using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class GoogleAuth
{
    public int Id { get; set; }

    public string MaKh { get; set; } = null!;

    public string GoogleId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Name { get; set; }

    public string? Picture { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public bool HieuLuc { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
