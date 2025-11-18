using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Dtos.Emergency;

namespace Services.Hubs
{
    public class EmergencyRequestHub : Hub
    {
        // Method called when a client connects
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        }

        // Method called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Client có thể join vào group để nhận thông báo theo customerId
        public async Task JoinCustomerGroup(string customerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
        }

        // Client có thể leave group
        public async Task LeaveCustomerGroup(string customerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer-{customerId}");
        }

        // Client có thể join vào group để nhận thông báo theo branchId
        public async Task JoinBranchGroup(string branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch-{branchId}");
        }

        // Client có thể leave branch group
        public async Task LeaveBranchGroup(string branchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch-{branchId}");
        }
    }
}

