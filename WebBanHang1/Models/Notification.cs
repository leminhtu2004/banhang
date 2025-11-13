using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string Type { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? LinkUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public bool IsActive { get; set; }

    public string? MaKh { get; set; }

    public virtual KhachHang? MaKhNavigation { get; set; }

    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
