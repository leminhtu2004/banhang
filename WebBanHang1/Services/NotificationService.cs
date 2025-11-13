using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebBanHang1.Data;
using WebBanHang1.Hubs;
using WebBanHang1.Models;

namespace WebBanHang1.Services
{
    public class NotificationService : INotificationService
    {
        private readonly QuanLiHangContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(QuanLiHangContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Notification> CreateNotificationAsync(string title, string message, string type, string? imageUrl = null, string? linkUrl = null, string? maKh = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                ImageUrl = imageUrl,
                LinkUrl = linkUrl,
                MaKh = maKh,
                CreatedAt = DateTime.Now,
                IsRead = false,
                IsActive = true
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Nếu là thông báo cho tất cả người dùng (maKh = null)
            if (string.IsNullOrEmpty(maKh))
            {
                // Lấy danh sách tất cả khách hàng
                var allUsers = await _context.KhachHangs
                    .Where(k => k.HieuLuc == true)
                    .Select(k => k.MaKh)
                    .ToListAsync();

                // Tạo UserNotification cho từng user
                var userNotifications = allUsers.Select(userId => new UserNotification
                {
                    MaKh = userId,
                    NotificationId = notification.Id,
                    IsRead = false
                }).ToList();

                _context.UserNotifications.AddRange(userNotifications);
                await _context.SaveChangesAsync();

                // Gửi thông báo real-time đến tất cả người dùng
                await _hubContext.Clients.Group("all").SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    imageUrl = notification.ImageUrl,
                    linkUrl = notification.LinkUrl,
                    createdAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    isNew = true // Đánh dấu đây là thông báo mới
                });
            }
            else
            {
                // Tạo UserNotification cho user cụ thể
                var userNotification = new UserNotification
                {
                    MaKh = maKh,
                    NotificationId = notification.Id,
                    IsRead = false
                };

                _context.UserNotifications.Add(userNotification);
                await _context.SaveChangesAsync();

                // Gửi thông báo real-time đến user cụ thể
                await _hubContext.Clients.Group(maKh).SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    imageUrl = notification.ImageUrl,
                    linkUrl = notification.LinkUrl,
                    createdAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    isNew = true // Đánh dấu đây là thông báo mới
                });
            }

            return notification;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string maKh, int page = 1, int pageSize = 10)
        {
            var notifications = await _context.UserNotifications
                .Include(un => un.Notification)
                .Where(un => un.MaKh == maKh && un.Notification.IsActive)
                .OrderByDescending(un => un.Notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(un => un.Notification)
                .ToListAsync();

            return notifications;
        }

        public async Task<int> GetUnreadNotificationCountAsync(string maKh)
        {
            return await _context.UserNotifications
                .CountAsync(un => un.MaKh == maKh && !un.IsRead && un.Notification.IsActive);
        }

        public async Task<int> GetReadNotificationCountAsync(string maKh)
        {
            return await _context.UserNotifications
                .CountAsync(un => un.MaKh == maKh && un.IsRead && un.Notification.IsActive);
        }

        public async Task MarkNotificationAsReadAsync(int notificationId, string maKh)
        {
            var userNotification = await _context.UserNotifications
                .FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.MaKh == maKh);

            if (userNotification != null)
            {
                userNotification.IsRead = true;
                userNotification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string maKh)
        {
            var userNotifications = await _context.UserNotifications
                .Where(un => un.MaKh == maKh && !un.IsRead)
                .ToListAsync();

            foreach (var userNotification in userNotifications)
            {
                userNotification.IsRead = true;
                userNotification.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId, string maKh)
        {
            var userNotification = await _context.UserNotifications
                .FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.MaKh == maKh);

            if (userNotification != null)
            {
                _context.UserNotifications.Remove(userNotification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldReadNotificationsAsync()
        {
            // Xóa thông báo đã đọc sau 1 ngày
            var oneDayAgo = DateTime.Now.AddDays(-1);
            
            var oldReadNotifications = await _context.UserNotifications
                .Include(un => un.Notification)
                .Where(un => un.IsRead && un.ReadAt.HasValue && un.ReadAt.Value < oneDayAgo)
                .ToListAsync();

            if (oldReadNotifications.Any())
            {
                _context.UserNotifications.RemoveRange(oldReadNotifications);
                await _context.SaveChangesAsync();
                
                // Log số lượng thông báo đã xóa
                Console.WriteLine($"Đã xóa {oldReadNotifications.Count} thông báo cũ đã đọc");
            }
        }

        public async Task BroadcastProductAddedNotificationAsync(HangHoa product)
        {
            var title = "Sản phẩm mới!";
            var message = $"Sản phẩm '{product.TenHh}' vừa được thêm vào cửa hàng. Hãy khám phá ngay!";
            var imageUrl = !string.IsNullOrEmpty(product.Hinh) ? $"/images/{product.Hinh}" : null;
            var linkUrl = $"/Products/Details/{product.MaHh}";

            await CreateNotificationAsync(title, message, "PRODUCT_ADDED", imageUrl, linkUrl);
        }
    }
} 