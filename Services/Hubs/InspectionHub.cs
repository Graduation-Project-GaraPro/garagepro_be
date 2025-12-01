using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class InspectionHub : Hub
    {
        // Technician joins group to receive notifications when assigned
        public async Task JoinTechnicianGroup(string technicianId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Technician_{technicianId}");
        }

        public async Task LeaveTechnicianGroup(string technicianId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Technician_{technicianId}");
        }

        public async Task JoinManagersGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
            Console.WriteLine($"[InspectionHub] Connection {Context.ConnectionId} joined Managers group");
        }

        public async Task LeaveManagersGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Managers");
        }

        // Join inspection-specific group to receive updates for a specific inspection
        public async Task JoinInspectionGroup(string inspectionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Inspection_{inspectionId}");
        }

        public async Task LeaveInspectionGroup(string inspectionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Inspection_{inspectionId}");
        }
    }
}