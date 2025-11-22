using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Services.Hubs
{
    public class PermissionHub : Hub
    {
        public async Task JoinRoleGroups(List<string> roleNames)
        {
            if (roleNames == null) return;

            foreach (var roleName in roleNames)
            {
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, roleName);
                }
            }
        }

        /// <summary>
        /// FE gọi khi logout / user switch / role changed
        /// </summary>
        public async Task LeaveRoleGroups(List<string> roleNames)
        {
            if (roleNames == null) return;

            foreach (var roleName in roleNames)
            {
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roleName);
                }
            }
        }
    }
}
