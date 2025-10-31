using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Garage_pro_api.Hubs
{
    public class OnlineUserHub : Hub
    {
        // Danh sách các connectionId đang online
        private static ConcurrentDictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            ConnectedUsers.TryAdd(Context.ConnectionId, Context.ConnectionId);

            // Thông báo tới tất cả client rằng số lượng user đã thay đổi
            await Clients.All.SendAsync("UserCountUpdated", ConnectedUsers.Count);

            // Gửi lại connectionId cho client mới
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUsers.TryRemove(Context.ConnectionId, out _);

            // Thông báo tới tất cả client rằng số lượng user đã thay đổi
            await Clients.All.SendAsync("UserCountUpdated", ConnectedUsers.Count);

            await base.OnDisconnectedAsync(exception);
        }

        // Client có thể gọi hàm này để lấy số lượng online hiện tại
        public Task<int> GetOnlineUserCount()
        {
            return Task.FromResult(ConnectedUsers.Count);
        }

        // Tuỳ chọn: gửi danh sách ConnectionId đang online (debug hoặc quản trị)
        public Task<string[]> GetAllConnections()
        {
            return Task.FromResult(ConnectedUsers.Keys.ToArray());
        }
    }
}
