using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class PasswordReset
{
    public int Id { get; set; }

    public string MaKh { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string ResetToken { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime NgayHetHan { get; set; }

    public bool DaSuDung { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
