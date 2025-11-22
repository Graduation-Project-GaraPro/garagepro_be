using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs
{
    public class QuotationHub : Hub
    {
        // Client join theo UserId (khách hàng)
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        // Client join theo QuotationId (theo dõi 1 báo giá cụ thể)
        public async Task JoinQuotationGroup(Guid quotationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Quotation_{quotationId}");
        }

        public async Task LeaveQuotationGroup(Guid quotationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Quotation_{quotationId}");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // optional: log, hoặc gửi lại ConnectionId cho client
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
