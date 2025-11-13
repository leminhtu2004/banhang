using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class EmailConfig
{
    public int Id { get; set; }

    public string TenCauHinh { get; set; } = null!;

    public string GiaTri { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime NgayCapNhat { get; set; }
}
