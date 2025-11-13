using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class UserNotification
{
    public int Id { get; set; }

    public string MaKh { get; set; } = null!;

    public int NotificationId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual Notification Notification { get; set; } = null!;
}
