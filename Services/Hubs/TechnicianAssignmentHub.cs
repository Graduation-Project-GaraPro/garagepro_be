using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class TechnicianAssignmentHub : Hub
    {
        // Method to notify when jobs are assigned to a technician
        public async Task NotifyJobAssignment(Guid technicianId, string technicianName, int jobCount, string[] jobNames)
        {
            // Notify all clients about the job assignment
            await Clients.All.SendAsync("JobAssigned", technicianId, technicianName, jobCount, jobNames);
        }

        // Method to notify when inspections are assigned to a technician
        public async Task NotifyInspectionAssignment(Guid technicianId, string technicianName, int inspectionCount, string[] inspectionNames)
        {
            // Notify all clients about the inspection assignment
            await Clients.All.SendAsync("InspectionAssigned", technicianId, technicianName, inspectionCount, inspectionNames);
        }

        // Method to notify when jobs are reassigned to a technician
        public async Task NotifyJobReassignment(Guid jobId, Guid oldTechnicianId, Guid newTechnicianId, string jobName)
        {
            // Notify all clients about the job reassignment
            await Clients.All.SendAsync("JobReassigned", jobId, oldTechnicianId, newTechnicianId, jobName);
        }

        // Method to notify when inspections are reassigned to a technician
        public async Task NotifyInspectionReassignment(Guid inspectionId, Guid oldTechnicianId, Guid newTechnicianId, string inspectionName)
        {
            // Notify all clients about the inspection reassignment
            await Clients.All.SendAsync("InspectionReassigned", inspectionId, oldTechnicianId, newTechnicianId, inspectionName);
        }

        // Method called when a client connects
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        }

        // Method called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}