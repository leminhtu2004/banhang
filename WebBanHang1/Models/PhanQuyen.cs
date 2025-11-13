using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class PhanQuyen
{
    public byte VaiTro { get; set; }

    public string TenVaiTro { get; set; } = null!;

    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
}
