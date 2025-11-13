using System;
using System.Collections.Generic;

namespace WebBanHang1.Models;

public partial class KhachHang
{
    public string MaKh { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public bool GioiTinh { get; set; }

    public DateOnly NgaySinh { get; set; }

    public string? DiaChi { get; set; }

    public string? DienThoai { get; set; }

    public string Email { get; set; } = null!;

    public string? Hinh { get; set; }

    public bool HieuLuc { get; set; }

    public byte VaiTro { get; set; }

    public string? RandomKey { get; set; }

    public bool EmailVerified { get; set; }

    public DateTime? NgayXacThucEmail { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public int LoginAttempts { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string? TwoFactorSecret { get; set; }

    public virtual ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();

    public virtual ICollection<GoogleAuth> GoogleAuths { get; set; } = new List<GoogleAuth>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<KhachHangSuDungMa> KhachHangSuDungMas { get; set; } = new List<KhachHangSuDungMa>();

    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ReviewReaction> ReviewReactions { get; set; } = new List<ReviewReaction>();

    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();

    public virtual PhanQuyen VaiTroNavigation { get; set; } = null!;

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
