using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class InspectionHub : Hub
    {
        // Technician join vào group để nhận notification khi được assign
        public async Task JoinTechnicianGroup(string technicianId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Technician_{technicianId}");
        }

        public async Task LeaveTechnicianGroup(string technicianId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Technician_{technicianId}");
        }
    }
}