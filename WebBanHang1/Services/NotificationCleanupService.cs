using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebBanHang1.Services
{
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Chạy mỗi 6 giờ

        public NotificationCleanupService(IServiceProvider serviceProvider, ILogger<NotificationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationCleanupService đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notificationService.DeleteOldReadNotificationsAsync();
                    }

                    _logger.LogInformation("Đã hoàn thành việc dọn dẹp thông báo cũ");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi dọn dẹp thông báo cũ");
                }

                // Chờ đến lần chạy tiếp theo
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
} 