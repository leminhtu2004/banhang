using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class EmailVerification
{
    public int Id { get; set; }

    public string MaKh { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string VerificationCode { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime NgayHetHan { get; set; }

    public bool DaSuDung { get; set; }

    public string LoaiXacThuc { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
