using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class JobHub : Hub
    {
        // Technician join vào group để nhận notification khi được assign job
        public async Task JoinTechnicianGroup(string technicianId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Technician_{technicianId}");
            Console.WriteLine($"[JobHub] Connection {Context.ConnectionId} joined Technician_{technicianId} group");
        }

        public async Task LeaveTechnicianGroup(string technicianId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Technician_{technicianId}");
        }

        // Join vào group theo JobId để nhận updates
        public async Task JoinJobGroup(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Job_{jobId}");
            Console.WriteLine($"[JobHub] Connection {Context.ConnectionId} joined Job_{jobId} group");
        }

        public async Task LeaveJobGroup(string jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Job_{jobId}");
        }

        public async Task JoinRepairOrderGroup(string repairOrderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
            Console.WriteLine($"[JobHub] Connection {Context.ConnectionId} joined RepairOrder_{repairOrderId} group");
        }

        public async Task LeaveRepairOrderGroup(string repairOrderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
        }

        public async Task JoinRepairOrderUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"RepairOrder_{userId}");
            Console.WriteLine($"[JobHub] Connection {Context.ConnectionId} joined RepairOrder_{userId} group");
        }

        public async Task LeaveRepairOrderUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"RepairOrder_{userId}");
        }

        // Managers join to monitor all job status changes
        public async Task JoinManagersGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
            Console.WriteLine($"[JobHub] Connection {Context.ConnectionId} joined Managers group");
        }

        public async Task LeaveManagersGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Managers");
        }
    }
}
