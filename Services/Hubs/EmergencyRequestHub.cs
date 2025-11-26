using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Dtos.Emergency;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using Repositories;

namespace Services.Hubs
{
    public class EmergencyRequestHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _connectionGroups = new();
        private readonly IUserRepository _userRepository;

        public EmergencyRequestHub(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        // Method called when a client connects
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);

            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
            if (!string.IsNullOrEmpty(userId))
            {
                _connectionGroups[Context.ConnectionId] = new HashSet<string>();

                var roles = Context.User?.FindAll(ClaimTypes.Role);
                if (roles != null)
                {
                    foreach (var r in roles)
                    {
                        if (string.Equals(r.Value, "Customer", StringComparison.OrdinalIgnoreCase))
                        {
                            var grp = $"customer-{userId}";
                            await Groups.AddToGroupAsync(Context.ConnectionId, grp);
                            _connectionGroups[Context.ConnectionId].Add(grp);
                            await Clients.Caller.SendAsync("JoinedCustomerGroup", grp);
                        }
                        else if (string.Equals(r.Value, "Manager", StringComparison.OrdinalIgnoreCase))
                        {
                            var user = await _userRepository.GetByIdAsync(userId);
                            if (user?.BranchId != null)
                            {
                                var grp = $"branch-{user.BranchId}";
                                await Groups.AddToGroupAsync(Context.ConnectionId, grp);
                                _connectionGroups[Context.ConnectionId].Add(grp);
                                await Clients.Caller.SendAsync("JoinedBranchGroup", grp);
                            }
                        }
                    }
                }
            }
        }

        // Method called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Client có thể join vào group để nhận thông báo theo customerId
        public async Task JoinCustomerGroup(string customerId)
        {
            var grp = $"customer-{customerId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, grp);
            _connectionGroups.AddOrUpdate(Context.ConnectionId, _ => new HashSet<string> { grp }, (_, set) => { set.Add(grp); return set; });
            await Clients.Caller.SendAsync("JoinedCustomerGroup", grp);
        }

        // Client có thể leave group
        public async Task LeaveCustomerGroup(string customerId)
        {
            var grp = $"customer-{customerId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grp);
            if (_connectionGroups.TryGetValue(Context.ConnectionId, out var set)) set.Remove(grp);
            await Clients.Caller.SendAsync("LeftCustomerGroup", grp);
        }

        // Client có thể join vào group để nhận thông báo theo branchId
        public async Task JoinBranchGroup(string branchId)
        {
            var grp = $"branch-{branchId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, grp);
            _connectionGroups.AddOrUpdate(Context.ConnectionId, _ => new HashSet<string> { grp }, (_, set) => { set.Add(grp); return set; });
            await Clients.Caller.SendAsync("JoinedBranchGroup", grp);
        }

        // Client có thể leave branch group
        public async Task LeaveBranchGroup(string branchId)
        {
            var grp = $"branch-{branchId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grp);
            if (_connectionGroups.TryGetValue(Context.ConnectionId, out var set)) set.Remove(grp);
            await Clients.Caller.SendAsync("LeftBranchGroup", grp);
        }

        public Task<IEnumerable<string>> GetJoinedGroups()
        {
            if (_connectionGroups.TryGetValue(Context.ConnectionId, out var set))
                return Task.FromResult<IEnumerable<string>>(set);
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
    }
}

