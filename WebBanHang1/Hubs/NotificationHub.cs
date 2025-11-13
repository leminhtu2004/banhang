using Microsoft.AspNetCore.SignalR;
using WebBanHang1.Models;

namespace WebBanHang1.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            // Thêm vào group "all" cho tất cả users
            await Groups.AddToGroupAsync(Context.ConnectionId, "all");
            
            // Nếu có user đăng nhập, thêm vào group riêng
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
            {
                _userConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
        }

        public async Task LeaveUserGroup(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
        }
    }
} 