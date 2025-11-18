using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Hubs
{
    [Authorize] // Bắt buộc phải login
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Tự động join vào group của chính user khi connect
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"[NotificationHub] User {userId} connected and joined group");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"[NotificationHub] User {userId} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method để mobile client join group thủ công (optional)
        public async Task JoinMyGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"[NotificationHub] User {userId} manually joined group");
            }
        }
    }
}
