using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebBanHang1.Services
{
    public class PromotionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PromotionBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Kiểm tra mỗi giờ

        public PromotionBackgroundService(
            IServiceProvider services,
            ILogger<PromotionBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Promotion Background Service đang chạy");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running daily promotion maintenance task.");
                    await DeactivateExpiredPromotionsAsync();
                    await DeleteInactivePromotionsAsync();
                    _logger.LogInformation("Promotion maintenance task finished.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running promotion maintenance task.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task DeactivateExpiredPromotionsAsync()
        {
            using var scope = _services.CreateScope();
            var promotionService = scope.ServiceProvider.GetRequiredService<IPromotionService>();
            await promotionService.DeactivateExpiredPromotionsAsync();
        }

        private async Task DeleteInactivePromotionsAsync()
        {
             using var scope = _services.CreateScope();
             var promotionService = scope.ServiceProvider.GetRequiredService<IPromotionService>();
             await promotionService.DeleteInactivePromotionsAsync();
        }
    }
} 