using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Services.LogServices
{
    public class LogHub : Hub
    {
        private static readonly HashSet<string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            ConnectedUsers.Add(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUsers.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinLogGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveLogGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public int GetConnectedUsersCount()
        {
            return ConnectedUsers.Count;
        }
    }
}
