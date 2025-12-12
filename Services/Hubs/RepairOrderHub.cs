using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Dtos.RoBoard;

namespace Services.Hubs // Update namespace
{
    public class RepairOrderHub : Hub
    {
        // Group management methods
        public async Task JoinManagersGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
            Console.WriteLine($"[RepairOrderHub] Connection {Context.ConnectionId} joined Managers group");
        }

        public async Task LeaveManagersGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Managers");
        }

        public async Task JoinRepairOrderGroup(string repairOrderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
            Console.WriteLine($"[RepairOrderHub] Connection {Context.ConnectionId} joined RepairOrder_{repairOrderId} group");
        }

        public async Task LeaveRepairOrderGroup(string repairOrderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
        }

        public async Task JoinCustomerGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
            Console.WriteLine($"[RepairOrderHub] Connection {Context.ConnectionId} joined Customer_{userId} group");
        }

        public async Task LeaveCustomerGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{userId}");
        }

        public async Task JoinBranchGroup(string branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
            Console.WriteLine($"[RepairOrderHub] Connection {Context.ConnectionId} joined Branch_{branchId} group");
        }

        public async Task LeaveBranchGroup(string branchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
        }

        // Method to move a repair order between statuses
        public async Task MoveRepairOrder(string repairOrderId, string newStatusId)
        {
            await Clients.All.SendAsync("RepairOrderMoved", repairOrderId, newStatusId);
        }

        public async Task CreateRepairOrder(RoBoardCardDto repairOrder)
        {
            await Clients.All.SendAsync("RepairOrderCreated", repairOrder);
        }

        public async Task UpdateRepairOrder(RoBoardCardDto repairOrder)
        {
            await Clients.All.SendAsync("RepairOrderUpdated", repairOrder);
        }

        public async Task ArchiveRepairOrder(string repairOrderId)
        {
            await Clients.All.SendAsync("RepairOrderArchived", repairOrderId);
        }

        public async Task PayRepairOrder(string repairOrderId)
        {
            await Clients.All.SendAsync("RepairOrderPaid", repairOrderId);
        }

        public async Task DeleteRepairOrder(string repairOrderId)
        {
            await Clients.All.SendAsync("RepairOrderDeleted", repairOrderId);
        }

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
    }
}