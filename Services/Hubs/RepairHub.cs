using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class RepairHub : Hub
    {
        public async Task JoinRepairOrderGroup(string repairOrderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
            Console.WriteLine($"[RepairHub] Connection {Context.ConnectionId} joined RepairOrder_{repairOrderId} group");
        }

        public async Task LeaveRepairOrderGroup(string repairOrderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
        }

        public async Task JoinJobGroup(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Job_{jobId}");
            Console.WriteLine($"[RepairHub] Connection {Context.ConnectionId} joined Job_{jobId} group");
        }

        public async Task LeaveJobGroup(string jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Job_{jobId}");
        }

        // Managers join to monitor all repair activities
        public async Task JoinManagersGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
            Console.WriteLine($"[RepairHub] Connection {Context.ConnectionId} joined Managers group");
        }

        public async Task LeaveManagersGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Managers");
        }
    }
}
