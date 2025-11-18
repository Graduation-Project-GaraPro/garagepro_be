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
        }

        public async Task LeaveRepairOrderGroup(string repairOrderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"RepairOrder_{repairOrderId}");
        }

        public async Task JoinJobGroup(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Job_{jobId}");
        }

        public async Task LeaveJobGroup(string jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Job_{jobId}");
        }



    }
}
