using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs
{
   
        // Agent / nhân viên ở chi nhánh join theo branch
        public class RepairRequestHub : Hub
        {
            public async Task JoinBranchGroup(Guid branchId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"branch_{branchId}");
            }

            public async Task LeaveBranchGroup(Guid branchId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch_{branchId}");
            }

            // JOIN theo RepairRequest
            public async Task JoinRepairRequestGroup(Guid repairRequestId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetRepairRequestGroup(repairRequestId));
            }

            public async Task LeaveRepairRequestGroup(Guid repairRequestId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetRepairRequestGroup(repairRequestId));
            }

            public async Task JoinRepairRequestUserGroup(Guid userId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetRepairRequestUserGroup(userId));
            }

            public async Task LeaveRepairRequestUserGroup(Guid userId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetRepairRequestUserGroup(userId));
            }

        private static string GetRepairRequestUserGroup(Guid userId)
            => $"RepairRequestUser_{userId}";

        private static string GetRepairRequestGroup(Guid repairRequestId)
                => $"RepairRequest_{repairRequestId}";
        }
    }

