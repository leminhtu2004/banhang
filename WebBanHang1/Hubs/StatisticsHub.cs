// Hubs/StatisticsHub.cs
using Microsoft.AspNetCore.SignalR;

namespace WebBanHang1.Hubs
{
    public class StatisticsHub : Hub
    {
        public async Task NotifyStatisticsUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveStatisticsUpdate", message);
        }
    }
}
