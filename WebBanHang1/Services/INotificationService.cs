using WebBanHang1.Models;

namespace WebBanHang1.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string title, string message, string type, string? imageUrl = null, string? linkUrl = null, string? maKh = null);
        Task<List<Notification>> GetUserNotificationsAsync(string maKh, int page = 1, int pageSize = 10);
        Task<int> GetUnreadNotificationCountAsync(string maKh);
        Task<int> GetReadNotificationCountAsync(string maKh);
        Task MarkNotificationAsReadAsync(int notificationId, string maKh);
        Task MarkAllNotificationsAsReadAsync(string maKh);
        Task DeleteNotificationAsync(int notificationId, string maKh);
        Task DeleteOldReadNotificationsAsync();
        Task BroadcastProductAddedNotificationAsync(HangHoa product);
    }
} 