using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs
{
    public class RepairOrderArchiveHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        }

        // User join group riêng theo userId
        public Task JoinArchiveGroup(string userId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"RepairOrderArchive_{userId}");
        }

        public Task LeaveArchiveGroup(string userId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"RepairOrderArchive_{userId}");
        }
    }
}
