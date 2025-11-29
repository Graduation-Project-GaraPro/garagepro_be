using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs
{
    public class PromotionalHub : Hub
    {
        public const string HubUrl = "/hubs/promotions";

        private static string GetPromotionGroupName(Guid promotionId)
            => $"promotion-{promotionId}";

        private const string DashboardGroup = "promotions-dashboard";

        
        public Task JoinDashboard()
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, DashboardGroup);
        }

        public Task LeaveDashboard()
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, DashboardGroup);
        }

        
        public Task JoinPromotionGroup(Guid promotionId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, GetPromotionGroupName(promotionId));
        }

        public Task LeavePromotionGroup(Guid promotionId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetPromotionGroupName(promotionId));
        }
    }
}
