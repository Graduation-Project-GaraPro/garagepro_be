using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Dtos.RoBoard;

namespace Services.Hubs // Update namespace
{
    public class RepairOrderHub : Hub
    {
        // Method to move a repair order between statuses
        public async Task MoveRepairOrder(string repairOrderId, string newStatusId)
        {
            // Notify all clients about the move
            await Clients.All.SendAsync("RepairOrderMoved", repairOrderId, newStatusId);
        }

        // Method to create a new repair order
        public async Task CreateRepairOrder(RoBoardCardDto repairOrder)
        {
            // Notify all clients about the new repair order
            await Clients.All.SendAsync("RepairOrderCreated", repairOrder);
        }

        // Method to update an existing repair order
        public async Task UpdateRepairOrder(RoBoardCardDto repairOrder)
        {
            // Notify all clients about the updated repair order
            await Clients.All.SendAsync("RepairOrderUpdated", repairOrder);
        }

        public async Task PayRepairOrder(string repairOrderId )
        {
            // Notify all clients about the updated repair order
            await Clients.All.SendAsync("RepairOrderPaid", repairOrderId);
        }

        // Method to delete a repair order
        public async Task DeleteRepairOrder(string repairOrderId)
        {
            // Notify all clients about the deleted repair order
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