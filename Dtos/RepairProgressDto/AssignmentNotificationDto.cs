using System;

namespace Dtos.RepairProgressDto
{
    public class AssignmentNotificationDto
    {
        public Guid TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public int AssignmentCount { get; set; }
        public string[] AssignmentNames { get; set; } = Array.Empty<string>();
        public string AssignmentType { get; set; } = string.Empty; // "Job" or "Inspection"
    }
}